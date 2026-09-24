using HouseBills.Application.Persistence;

namespace HouseBills.Application.Bills;

/// <summary>Checks that the payee and category a bill (or recurring bill) points to exist.</summary>
internal static class BillReferenceValidation
{
    public static async Task ValidateAsync(
        List<string> errors,
        int payeeId,
        int categoryId,
        IPayeeRepository payees,
        ICategoryRepository categories,
        CancellationToken cancellationToken)
    {
        if (payeeId <= 0 || !await payees.ExistsAsync(payeeId, cancellationToken))
        {
            errors.Add("Select a payee.");
        }

        if (categoryId <= 0 || !await categories.ExistsAsync(categoryId, cancellationToken))
        {
            errors.Add("Select a category.");
        }
    }
}