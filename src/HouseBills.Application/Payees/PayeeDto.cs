namespace HouseBills.Application.Payees;

public sealed record PayeeDto(int Id, string Name, string? AccountReference, string? Notes, byte[] RowVersion);