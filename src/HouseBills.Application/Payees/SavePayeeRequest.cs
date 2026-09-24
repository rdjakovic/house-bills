namespace HouseBills.Application.Payees;

/// <summary>Create (<paramref name="Id"/> is <c>null</c>) or update a payee.</summary>
public sealed record SavePayeeRequest(int? Id, string Name, string? AccountReference, string? Notes, byte[]? RowVersion);