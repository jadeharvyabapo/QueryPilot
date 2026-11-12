namespace QueryPilot.Web.Models;

public enum DashboardMetricIcon
{
    NetSales,
    Orders,
    AverageOrderValue,
    ReturningCustomers
}

public sealed record DashboardMetric(
    string Label,
    string Value,
    DashboardMetricIcon Icon);

public sealed record ProductCategory(string Name, decimal Sales, decimal Share);

public sealed record DashboardSnapshot(
    IReadOnlyList<DashboardMetric> Metrics,
    decimal TotalSales,
    string[] MonthLabels,
    double[] MonthlySales,
    IReadOnlyList<ProductCategory> Categories);
