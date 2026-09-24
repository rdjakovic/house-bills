using HouseBills.Application.Payees;
using HouseBills.Domain;

namespace HouseBills.Application.Persistence;

/// <summary>Persistence for <see cref="Payee"/>.</summary>
public interface IPayeeRepository : IRepository<Payee>
{
    /// <summary>All payees ordered by name.</summary>
    Task<IReadOnlyList<PayeeDto>> ListAsync(CancellationToken cancellationToken);

    /// <summary>Whether another payee (other than <paramref name="excludeId"/>) already uses <paramref name="name"/>.</summary>
    Task<bool> NameExistsAsync(string name, int? excludeId, CancellationToken cancellationToken);

    /// <summary>Whether any bill or recurring bill references the payee.</summary>
    Task<bool> IsInUseAsync(int id, CancellationToken cancellationToken);
}