using HouseBills.Domain;

namespace HouseBills.Application.RecurringBills;

/// <summary>Create (<paramref name="Id"/> is <c>null</c>) or update a recurring bill template.</summary>
public sealed record SaveRecurringBillRequest(
    int? Id,
    string Name,
    int PayeeId,
    int CategoryId,
    decimal Amount,
    BillFrequency Frequency,
    DateOnly StartDate,
    DateOnly? EndDate,
    string? Notes,
    byte[]? RowVersion);