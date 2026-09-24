using HouseBills.Application.Common;

namespace HouseBills.Application.Payees;

/// <summary>Manages payees (who bills are paid to).</summary>
public interface IPayeeService
{
    /// <summary>All payees ordered by name.</summary>
    Task<IReadOnlyList<PayeeDto>> ListAsync(CancellationToken cancellationToken);

    /// <summary>Creates or updates a payee. Names must be unique. Returns the payee id.</summary>
    Task<Result<int>> SaveAsync(SavePayeeRequest request, CancellationToken cancellationToken);

    /// <summary>Deletes a payee that is not used by any bill or recurring bill.</summary>
    Task<Result> DeleteAsync(int id, byte[] rowVersion, CancellationToken cancellationToken);
}