namespace HouseBills.Application.Bills;

/// <summary>Criteria for listing bills. Date bounds apply to the due date and are inclusive.</summary>
public sealed record BillFilter(
    DateOnly? DueFrom,
    DateOnly? DueTo,
    BillStatusFilter Status = BillStatusFilter.All,
    int? CategoryId = null,
    int? PayeeId = null);