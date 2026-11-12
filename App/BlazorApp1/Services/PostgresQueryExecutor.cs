using System.Data;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using QueryPilot.Web.Data;
using QueryPilot.Web.Models;

namespace QueryPilot.Web.Services;

public sealed class PostgresQueryExecutor(
    ApplicationDbContext dbContext,
    IConfiguration configuration)
{
    private const int MaxRows = 200;
    private const int CommandTimeoutSeconds = 12;
    private static readonly Regex ReadQueryStart = new(
        @"\A\s*(SELECT|WITH)\b",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

    public async Task<SqlQueryResult> ExecuteReadOnlyAsync(
        string sql,
        CancellationToken cancellationToken = default)
    {
        ValidateSql(sql);
        EnsureDatabaseConfigured();

        var connection = dbContext.Database.GetDbConnection();
        var closeConnection = connection.State != ConnectionState.Open;
        if (closeConnection)
        {
            await connection.OpenAsync(cancellationToken);
        }

        try
        {
            await using var transaction = await connection.BeginTransactionAsync(cancellationToken);
            await ExecuteTransactionSettingAsync(connection, transaction, "SET TRANSACTION READ ONLY", cancellationToken);
            await ExecuteTransactionSettingAsync(connection, transaction, "SET LOCAL statement_timeout = '10s'", cancellationToken);

            await using var command = connection.CreateCommand();
            command.Transaction = transaction;
            command.CommandTimeout = CommandTimeoutSeconds;
            command.CommandText = sql.Trim().TrimEnd(';').TrimEnd();

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            var columns = Enumerable.Range(0, reader.FieldCount)
                .Select(reader.GetName)
                .ToArray();
            var rows = new List<IReadOnlyList<object?>>();
            var isTruncated = false;

            while (await reader.ReadAsync(cancellationToken))
            {
                if (rows.Count == MaxRows)
                {
                    isTruncated = true;
                    break;
                }

                var row = new object?[reader.FieldCount];
                for (var index = 0; index < reader.FieldCount; index++)
                {
                    row[index] = await reader.IsDBNullAsync(index, cancellationToken)
                        ? null
                        : reader.GetValue(index);
                }

                rows.Add(row);
            }

            await transaction.CommitAsync(cancellationToken);
            return new SqlQueryResult(columns, rows, isTruncated);
        }
        finally
        {
            if (closeConnection)
            {
                await connection.CloseAsync();
            }
        }
    }

    private void EnsureDatabaseConfigured()
    {
        if (string.IsNullOrWhiteSpace(configuration.GetConnectionString("DefaultConnection")))
        {
            throw new InvalidOperationException(
                "The PostgreSQL connection is not configured. Set ConnectionStrings__DefaultConnection before querying Northwind.");
        }
    }

    private static void ValidateSql(string sql)
    {
        if (string.IsNullOrWhiteSpace(sql) || sql.Length > 20_000)
        {
            throw new InvalidOperationException("The generated SQL is empty or too long.");
        }

        var query = sql.Trim();
        if (query.EndsWith(';'))
        {
            query = query[..^1].TrimEnd();
        }

        if (!ReadQueryStart.IsMatch(query)
            || query.Contains(';')
            || query.Contains("--", StringComparison.Ordinal)
            || query.Contains("/*", StringComparison.Ordinal)
            || query.Contains("*/", StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Only a single read-only SELECT query can be executed.");
        }
    }

    private static async Task ExecuteTransactionSettingAsync(
        System.Data.Common.DbConnection connection,
        System.Data.Common.DbTransaction transaction,
        string sql,
        CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = sql;
        await command.ExecuteNonQueryAsync(cancellationToken);
    }
}
