using HouseBills.Application.Reports;

namespace HouseBills.Wpf.ViewModels.Reports;

/// <summary>A monthly summary row prepared for display.</summary>
/// <param name="MonthName">Localized month name.</param>
/// <param name="Row">The underlying totals.</param>
/// <param name="BarFraction">This month's total relative to the largest month (0..1), for the bar.</param>
public sealed record MonthlySummaryItem(string MonthName, MonthlySummaryRow Row, double BarFraction)
{
    public decimal ChangeFromPreviousYear => Row.TotalAmount - Row.PreviousYearTotalAmount;
}