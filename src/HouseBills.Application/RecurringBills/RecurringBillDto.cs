using HouseBills.Domain;

namespace HouseBills.Application.RecurringBills;

public sealed record RecurringBillDto(
    int Id,
    string Name,
    int PayeeId,
    string PayeeName,
    int CategoryId,
    string CategoryName,
    decimal Amount,
    BillFrequency Frequency,
    DateOnly StartDate,
    DateOnly? EndDate,
    string? Notes,
    bool IsActive,
    DateOnly? GeneratedThrough,
    byte[] RowVersion)
{
    /// <summary>Next due date from today, or <c>null</c> if inactive or ended.</summary>
    public DateOnly? NextDueDate { get; init; }
}