using HouseBills.Domain;

namespace HouseBills.Application.Persistence;

/// <summary>Persistence operations shared by all aggregates.</summary>
/// <typeparam name="T">The aggregate type.</typeparam>
public interface IRepository<T>
    where T : Entity
{
    /// <summary>Loads an aggregate for modification, or returns <c>null</c> if it does not exist.</summary>
    Task<T?> GetAsync(int id, CancellationToken cancellationToken);

    /// <summary>Returns whether a row with the given id exists.</summary>
    Task<bool> ExistsAsync(int id, CancellationToken cancellationToken);

    /// <summary>Inserts a new aggregate; its <see cref="Entity.Id"/> is populated afterwards.</summary>
    Task AddAsync(T entity, CancellationToken cancellationToken);

    /// <summary>Saves changes to an aggregate loaded with <see cref="GetAsync"/>.</summary>
    /// <param name="entity">The modified aggregate.</param>
    /// <param name="expectedRowVersion">The row version the user's edit was based on.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <exception cref="Common.ConcurrencyConflictException">The row changed since <paramref name="expectedRowVersion"/>.</exception>
    Task UpdateAsync(T entity, byte[] expectedRowVersion, CancellationToken cancellationToken);

    /// <summary>Deletes a row if it is still at <paramref name="expectedRowVersion"/>.</summary>
    /// <exception cref="Common.ConcurrencyConflictException">The row changed or no longer exists.</exception>
    Task DeleteAsync(int id, byte[] expectedRowVersion, CancellationToken cancellationToken);
}