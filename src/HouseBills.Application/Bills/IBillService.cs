using HouseBills.Application.Common;

namespace HouseBills.Application.Bills;

/// <summary>Manages individual bills and their paid state.</summary>
public interface IBillService
{
    /// <summary>Bills matching <paramref name="filter"/>, ordered by due date, with <see cref="BillListItem.Status"/> populated.</summary>
    Task<IReadOnlyList<BillListItem>> ListAsync(BillFilter filter, CancellationToken cancellationToken);

    /// <summary>Creates or updates a bill. Returns the bill id.</summary>
    Task<Result<int>> SaveAsync(SaveBillRequest request, CancellationToken cancellationToken);

    /// <summary>Records a payment. The payment date may not be in the future.</summary>
    Task<Result> MarkPaidAsync(MarkBillPaidRequest request, CancellationToken cancellationToken);

    /// <summary>Clears a recorded payment.</summary>
    Task<Result> MarkUnpaidAsync(int id, byte[] rowVersion, CancellationToken cancellationToken);

    /// <summary>Deletes a bill. Deleting a generated bill does not cause it to be generated again.</summary>
    Task<Result> DeleteAsync(int id, byte[] rowVersion, CancellationToken cancellationToken);
}