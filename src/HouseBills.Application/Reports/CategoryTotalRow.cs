namespace HouseBills.Application.Reports;

/// <summary>Totals for bills of one category due within a date range.</summary>
public sealed record CategoryTotalRow(string CategoryName, int BillCount, decimal TotalAmount, decimal PaidAmount);