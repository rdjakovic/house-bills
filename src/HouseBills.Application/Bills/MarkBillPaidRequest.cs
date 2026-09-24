namespace HouseBills.Application.Bills;

public sealed record MarkBillPaidRequest(int Id, DateOnly PaidOn, decimal PaidAmount, byte[] RowVersion);