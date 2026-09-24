using HouseBills.Application.Common;
using HouseBills.Domain;

namespace HouseBills.Application.Persistence;

internal static class RepositoryExtensions
{
    public static async Task<Result> TryUpdateAsync<T>(this IRepository<T> repository, T entity, byte[]? expectedRowVersion, CancellationToken cancellationToken)
        where T : Entity
    {
        ArgumentNullException.ThrowIfNull(expectedRowVersion);
        try
        {
            await repository.UpdateAsync(entity, expectedRowVersion, cancellationToken);
            return Result.Success();
        }
        catch (ConcurrencyConflictException)
        {
            return Error.Conflict();
        }
    }

    public static async Task<Result> TryDeleteAsync<T>(this IRepository<T> repository, int id, byte[] expectedRowVersion, CancellationToken cancellationToken)
        where T : Entity
    {
        try
        {
            await repository.DeleteAsync(id, expectedRowVersion, cancellationToken);
            return Result.Success();
        }
        catch (ConcurrencyConflictException)
        {
            return Error.Conflict();
        }
    }
}