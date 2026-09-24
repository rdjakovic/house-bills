namespace HouseBills.Application.Reports;

/// <summary>Totals for bills due in one month of a year, with the same month of the previous year for comparison.</summary>
public sealed record MonthlySummaryRow(
    int Month,
    int BillCount,
    decimal TotalAmount,
    decimal PaidAmount,
    decimal OutstandingAmount,
    decimal PreviousYearTotalAmount);