namespace QueryPilot.Web.Models;

public sealed record SqlQueryResult(
    IReadOnlyList<string> Columns,
    IReadOnlyList<IReadOnlyList<object?>> Rows,
    bool IsTruncated);

public sealed record AnalystResult(
    string Sql,
    string Summary,
    SqlQueryResult Data,
    string? ChartSeriesName,
    string[] ChartLabels,
    double[] ChartValues);
