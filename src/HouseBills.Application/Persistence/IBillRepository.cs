using HouseBills.Application.Bills;
using HouseBills.Domain;

namespace HouseBills.Application.Persistence;

/// <summary>Persistence for <see cref="Bill"/>.</summary>
public interface IBillRepository : IRepository<Bill>
{
    /// <summary>
    /// Bills matching <paramref name="filter"/>, ordered by due date. <see cref="BillListItem.Status"/> is not populated.
    /// </summary>
    /// <param name="filter">Filter criteria.</param>
    /// <param name="today">Reference date for the <see cref="BillStatusFilter.Overdue"/> filter.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<IReadOnlyList<BillListItem>> ListAsync(BillFilter filter, DateOnly today, CancellationToken cancellationToken);
}