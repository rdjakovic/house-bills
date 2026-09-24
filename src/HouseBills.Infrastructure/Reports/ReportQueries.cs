using System.Data;

using Dapper;

using HouseBills.Application.Reports;

namespace HouseBills.Infrastructure.Reports;

internal sealed class ReportQueries(ISqlConnectionFactory connectionFactory) : IReportQueries
{
    // Range predicates on DueDate (parameters typed as `date`) are SARGable and use IX_Bills_DueDate,
    // which includes the aggregated columns. MONTH()/YEAR() are only applied after filtering.
    private const string MonthlySummarySql = """
        WITH Months AS (
            SELECT v.[Month] FROM (VALUES (1),(2),(3),(4),(5),(6),(7),(8),(9),(10),(11),(12)) AS v([Month])
        ),
        Totals AS (
            SELECT YEAR(b.DueDate) AS [Year],
                   MONTH(b.DueDate) AS [Month],
                   COUNT(*) AS BillCount,
                   SUM(b.Amount) AS TotalAmount,
                   SUM(CASE WHEN b.PaidOn IS NOT NULL THEN b.PaidAmount ELSE 0 END) AS PaidAmount,
                   SUM(CASE WHEN b.PaidOn IS NULL THEN b.Amount ELSE 0 END) AS OutstandingAmount
            FROM dbo.Bills AS b
            WHERE b.DueDate >= @PreviousYearStart AND b.DueDate < @NextYearStart
            GROUP BY YEAR(b.DueDate), MONTH(b.DueDate)
        )
        SELECT m.[Month],
               COALESCE(cur.BillCount, 0) AS BillCount,
               COALESCE(cur.TotalAmount, 0) AS TotalAmount,
               COALESCE(cur.PaidAmount, 0) AS PaidAmount,
               COALESCE(cur.OutstandingAmount, 0) AS OutstandingAmount,
               COALESCE(prev.TotalAmount, 0) AS PreviousYearTotalAmount
        FROM Months AS m
        LEFT JOIN Totals AS cur ON cur.[Month] = m.[Month] AND cur.[Year] = @Year
        LEFT JOIN Totals AS prev ON prev.[Month] = m.[Month] AND prev.[Year] = @Year - 1
        ORDER BY m.[Month];
        """;

    private const string CategoryTotalsSql = """
        SELECT c.Name AS CategoryName,
               COUNT(*) AS BillCount,
               SUM(b.Amount) AS TotalAmount,
               SUM(CASE WHEN b.PaidOn IS NOT NULL THEN b.PaidAmount ELSE 0 END) AS PaidAmount
        FROM dbo.Bills AS b
        INNER JOIN dbo.Categories AS c ON c.Id = b.CategoryId
        WHERE b.DueDate >= @From AND b.DueDate <= @To
        GROUP BY c.Id, c.Name
        ORDER BY TotalAmount DESC, c.Name;
        """;

    public async Task<IReadOnlyList<MonthlySummaryRow>> GetMonthlySummaryAsync(int year, CancellationToken cancellationToken)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(year, DateOnly.MinValue.Year + 1);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(year, DateOnly.MaxValue.Year - 1);

        var parameters = new DynamicParameters();
        parameters.Add("Year", year, DbType.Int32);
        parameters.Add("PreviousYearStart", new DateTime(year - 1, 1, 1), DbType.Date);
        parameters.Add("NextYearStart", new DateTime(year + 1, 1, 1), DbType.Date);

        await using var connection = connectionFactory.Create();
        var rows = await connection.QueryAsync<MonthlySummaryRow>(
            new CommandDefinition(MonthlySummarySql, parameters, cancellationToken: cancellationToken));
        return rows.AsList();
    }

    public async Task<IReadOnlyList<CategoryTotalRow>> GetCategoryTotalsAsync(DateOnly from, DateOnly to, CancellationToken cancellationToken)
    {
        var parameters = new DynamicParameters();
        parameters.Add("From", from.ToDateTime(TimeOnly.MinValue), DbType.Date);
        parameters.Add("To", to.ToDateTime(TimeOnly.MinValue), DbType.Date);

        await using var connection = connectionFactory.Create();
        var rows = await connection.QueryAsync<CategoryTotalRow>(
            new CommandDefinition(CategoryTotalsSql, parameters, cancellationToken: cancellationToken));
        return rows.AsList();
    }
}