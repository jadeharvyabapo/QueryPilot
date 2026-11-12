using System.Net.Http.Json;
using QueryPilot.Web.Models;

namespace QueryPilot.Web.Services;

public sealed class GenerateSqlQueryService(HttpClient httpClient)
{
    public async Task<string> GenerateAsync(string question, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(question))
        {
            throw new ArgumentException("A question is required.", nameof(question));
        }

        using var response = await httpClient.PostAsJsonAsync(
            "generate-sql", new GenerateSqlRequest(question.Trim()), cancellationToken);
        response.EnsureSuccessStatusCode();
        var sql = (await response.Content.ReadAsStringAsync(cancellationToken)).Trim();
        if (string.IsNullOrWhiteSpace(sql))
        {
            throw new InvalidOperationException("The SQL generation service returned an empty query.");
        }

        return sql;
    }
}
