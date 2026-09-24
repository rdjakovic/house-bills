namespace HouseBills.Domain;

/// <summary>
/// Base type for persisted aggregates: identity plus an optimistic-concurrency token.
/// </summary>
public abstract class Entity
{
    public int Id { get; private set; }

    /// <summary>Server-generated concurrency token (SQL Server <c>rowversion</c>).</summary>
    public byte[] RowVersion { get; private set; } = [];
}