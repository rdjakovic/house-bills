using HouseBills.Application.Common;
using HouseBills.Application.Persistence;
using HouseBills.Domain;

namespace HouseBills.Application.Payees;

internal sealed class PayeeService(IPayeeRepository repository) : IPayeeService
{
    public Task<IReadOnlyList<PayeeDto>> ListAsync(CancellationToken cancellationToken)
    {
        return repository.ListAsync(cancellationToken);
    }

    public async Task<Result<int>> SaveAsync(SavePayeeRequest request, CancellationToken cancellationToken)
    {
        var errors = new List<string>();
        FieldValidation.Text(errors, request.Name, Payee.NameMaxLength, "Name", required: true);
        FieldValidation.Text(errors, request.AccountReference, Payee.AccountReferenceMaxLength, "Account reference", required: false);
        FieldValidation.Text(errors, request.Notes, Payee.NotesMaxLength, "Notes", required: false);
        if (FieldValidation.ToError(errors) is { } validationError)
        {
            return validationError;
        }

        var name = request.Name.Trim();
        if (await repository.NameExistsAsync(name, request.Id, cancellationToken))
        {
            return Error.Validation($"A payee named '{name}' already exists.");
        }

        if (request.Id is not { } id)
        {
            var payee = new Payee(name, request.AccountReference, request.Notes);
            await repository.AddAsync(payee, cancellationToken);
            return payee.Id;
        }

        var existing = await repository.GetAsync(id, cancellationToken);
        if (existing is null)
        {
            return Error.NotFound("The payee no longer exists.");
        }

        existing.Update(name, request.AccountReference, request.Notes);
        var result = await repository.TryUpdateAsync(existing, request.RowVersion, cancellationToken);
        return result.IsSuccess ? id : result.Error!;
    }

    public async Task<Result> DeleteAsync(int id, byte[] rowVersion, CancellationToken cancellationToken)
    {
        if (await repository.IsInUseAsync(id, cancellationToken))
        {
            return Error.Validation("This payee is used by bills or recurring bills and cannot be deleted.");
        }

        return await repository.TryDeleteAsync(id, rowVersion, cancellationToken);
    }
}