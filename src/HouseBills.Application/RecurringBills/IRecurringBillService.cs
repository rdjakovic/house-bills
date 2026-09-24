using HouseBills.Application.Common;

namespace HouseBills.Application.RecurringBills;

/// <summary>Manages recurring bill templates and generates bills from them.</summary>
public interface IRecurringBillService
{
    /// <summary>All templates ordered by name, with <see cref="RecurringBillDto.NextDueDate"/> populated.</summary>
    Task<IReadOnlyList<RecurringBillDto>> ListAsync(CancellationToken cancellationToken);

    /// <summary>Creates or updates a template. Changes apply to bills generated afterwards. Returns the template id.</summary>
    Task<Result<int>> SaveAsync(SaveRecurringBillRequest request, CancellationToken cancellationToken);

    /// <summary>Pauses or resumes a template. Resuming does not back-fill the paused period.</summary>
    Task<Result> SetActiveAsync(int id, bool isActive, byte[] rowVersion, CancellationToken cancellationToken);

    /// <summary>Deletes a template. Bills already generated from it are kept.</summary>
    Task<Result> DeleteAsync(int id, byte[] rowVersion, CancellationToken cancellationToken);

    /// <summary>
    /// Creates bills for all active templates up to <see cref="BillingOptions.GenerationLookaheadDays"/> from today.
    /// Templates changed concurrently by another user are skipped (they will be picked up next time).
    /// Returns the number of bills created.
    /// </summary>
    Task<int> GenerateUpcomingBillsAsync(CancellationToken cancellationToken);
}