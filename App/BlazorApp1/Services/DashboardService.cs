using System.Globalization;
using QueryPilot.Web.Models;

namespace QueryPilot.Web.Services;

public sealed class DashboardService(PostgresQueryExecutor queryExecutor)
{
    private static readonly CultureInfo CurrencyCulture = CultureInfo.GetCultureInfo("en-US");

    private const string SummaryQuery = """
        WITH order_sales AS (
            SELECT o.order_id, o.customer_id,
                   SUM(od.unit_price * od.quantity * (1 - od.discount)) AS total
            FROM northwind.orders AS o
            JOIN northwind.order_details AS od ON od.order_id = o.order_id
            GROUP BY o.order_id, o.customer_id
        ),
        customer_orders AS (
            SELECT customer_id, COUNT(*) AS order_count
            FROM northwind.orders
            WHERE customer_id IS NOT NULL
            GROUP BY customer_id
        )
        SELECT COALESCE(SUM(os.total), 0) AS total_sales,
               COUNT(*) AS total_orders,
               COALESCE(AVG(os.total), 0) AS average_order_value,
               (SELECT COALESCE(
                   100.0 * COUNT(*) FILTER (WHERE order_count > 1)
                   / NULLIF(COUNT(*), 0), 0
               ) FROM customer_orders) AS returning_customer_percent
        FROM order_sales AS os
        """;

    private const string MonthlySalesQuery = """
        WITH monthly_sales AS (
            SELECT DATE_TRUNC('month', TO_DATE(o.order_date, 'YYYY-MM-DD')) AS month,
                   SUM(od.unit_price * od.quantity * (1 - od.discount)) AS sales
            FROM northwind.orders AS o
            JOIN northwind.order_details AS od ON od.order_id = o.order_id
            WHERE o.order_date ~ '^[0-9]{4}-[0-9]{2}-[0-9]{2}$'
            GROUP BY DATE_TRUNC('month', TO_DATE(o.order_date, 'YYYY-MM-DD'))
        ),
        recent_months AS (
            SELECT month, sales
            FROM monthly_sales
            ORDER BY month DESC
            LIMIT 12
        )
        SELECT TO_CHAR(month, 'Mon YYYY') AS month_label, sales
        FROM recent_months
        ORDER BY month
        """;

    private const string CategorySalesQuery = """
        WITH category_sales AS (
            SELECT COALESCE(c.category_name, 'Uncategorized') AS category,
                   SUM(od.unit_price * od.quantity * (1 - od.discount)) AS sales
            FROM northwind.order_details AS od
            JOIN northwind.products AS p ON p.product_id = od.product_id
            LEFT JOIN northwind.categories AS c ON c.category_id = p.category_id
            GROUP BY c.category_name
        )
        SELECT category, sales,
               100.0 * sales / NULLIF(SUM(sales) OVER (), 0) AS share
        FROM category_sales
        ORDER BY sales DESC
        LIMIT 5
        """;

    public async Task<DashboardSnapshot> GetSnapshotAsync(CancellationToken cancellationToken = default)
    {
        var summary = await queryExecutor.ExecuteReadOnlyAsync(SummaryQuery, cancellationToken);
        var monthlySales = await queryExecutor.ExecuteReadOnlyAsync(MonthlySalesQuery, cancellationToken);
        var categorySales = await queryExecutor.ExecuteReadOnlyAsync(CategorySalesQuery, cancellationToken);

        var totalSales = GetDecimal(summary, 0, 0);
        var totalOrders = GetDecimal(summary, 0, 1);
        var averageOrderValue = GetDecimal(summary, 0, 2);
        var returningCustomerPercent = GetDecimal(summary, 0, 3);

        var metrics = new[]
        {
            new DashboardMetric("Net sales", totalSales.ToString("C0", CurrencyCulture), DashboardMetricIcon.NetSales),
            new DashboardMetric("Orders", totalOrders.ToString("N0", CurrencyCulture), DashboardMetricIcon.Orders),
            new DashboardMetric("Average order value", averageOrderValue.ToString("C2", CurrencyCulture), DashboardMetricIcon.AverageOrderValue),
            new DashboardMetric("Returning customers", $"{returningCustomerPercent:N1}%", DashboardMetricIcon.ReturningCustomers)
        };

        var monthLabels = monthlySales.Rows
            .Select(row => Convert.ToString(row[0], CultureInfo.InvariantCulture) ?? string.Empty)
            .ToArray();
        var monthlyValues = monthlySales.Rows
            .Select(row => (double)GetDecimal(row[1]))
            .ToArray();
        var categories = categorySales.Rows
            .Select(row => new ProductCategory(
                Convert.ToString(row[0], CultureInfo.InvariantCulture) ?? "Uncategorized",
                GetDecimal(row[1]),
                GetDecimal(row[2])))
            .ToArray();

        return new DashboardSnapshot(metrics, totalSales, monthLabels, monthlyValues, categories);
    }

    private static decimal GetDecimal(SqlQueryResult result, int rowIndex, int columnIndex) =>
        result.Rows.Count > rowIndex
            ? GetDecimal(result.Rows[rowIndex][columnIndex])
            : 0m;

    private static decimal GetDecimal(object? value) =>
        value is null or DBNull
            ? 0m
            : Convert.ToDecimal(value, CultureInfo.InvariantCulture);
}
