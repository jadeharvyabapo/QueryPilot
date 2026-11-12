using System.Globalization;
using QueryPilot.Web.Models;

namespace QueryPilot.Web.Services;

public sealed class AnalystQueryService(
    GenerateSqlQueryService sqlGenerator,
    PostgresQueryExecutor queryExecutor)
{
    public async Task<AnalystResult> AskAsync(string question, CancellationToken cancellationToken = default)
    {
        var sql = await sqlGenerator.GenerateAsync(question, cancellationToken);
        var data = await queryExecutor.ExecuteReadOnlyAsync(sql, cancellationToken);
        var (seriesName, labels, values, categoryName) = BuildChart(data);
        var summary = BuildSummary(data, categoryName, seriesName);

        return new AnalystResult(sql, summary, data, seriesName, labels, values);
    }

    private static (string? SeriesName, string[] Labels, double[] Values, string? CategoryName) BuildChart(
        SqlQueryResult data)
    {
        if (data.Rows.Count == 0 || data.Columns.Count == 0)
        {
            return (null, [], [], null);
        }

        var categoryIndex = Enumerable.Range(0, data.Columns.Count)
            .FirstOrDefault(index => data.Rows.Any(row => row[index] is not null && !TryGetNumber(row[index], out _)), -1);
        var numericColumns = Enumerable.Range(0, data.Columns.Count)
            .Where(index => data.Rows.Any(row => TryGetNumber(row[index], out _)))
            .ToArray();
        var valueIndex = categoryIndex >= 0
            ? numericColumns.FirstOrDefault(index => index > categoryIndex, numericColumns.FirstOrDefault(-1))
            : numericColumns.FirstOrDefault(-1);
        if (valueIndex < 0)
        {
            return (null, [], [], null);
        }

        var chartRows = data.Rows.Take(50).ToArray();
        var labels = chartRows
            .Select((row, index) => categoryIndex >= 0
                ? FormatValue(row[categoryIndex])
                : (index + 1).ToString(CultureInfo.InvariantCulture))
            .ToArray();
        var values = chartRows
            .Select(row => TryGetNumber(row[valueIndex], out var value) ? value : 0d)
            .ToArray();
        var categoryName = categoryIndex >= 0 ? data.Columns[categoryIndex] : "Row";

        return (data.Columns[valueIndex], labels, values, categoryName);
    }

    private static string BuildSummary(SqlQueryResult data, string? categoryName, string? seriesName)
    {
        var rowDescription = data.IsTruncated
            ? $"Showing the first {data.Rows.Count} rows"
            : $"The query returned {data.Rows.Count} rows";

        return seriesName is not null
            ? $"{rowDescription} across {data.Columns.Count} columns. The chart compares {seriesName} by {categoryName}."
            : $"{rowDescription} across {data.Columns.Count} columns. No numeric column was available to chart.";
    }

    private static bool TryGetNumber(object? value, out double number)
    {
        number = value switch
        {
            byte numeric => numeric,
            short numeric => numeric,
            int numeric => numeric,
            long numeric => numeric,
            float numeric => numeric,
            double numeric => numeric,
            decimal numeric => (double)numeric,
            _ => double.NaN
        };

        return double.IsFinite(number);
    }

    private static string FormatValue(object? value) =>
        value switch
        {
            null or DBNull => "NULL",
            DateTime dateTime => dateTime.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            DateOnly date => date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            IFormattable formattable => formattable.ToString(null, CultureInfo.InvariantCulture),
            _ => value.ToString() ?? string.Empty
        };
}
