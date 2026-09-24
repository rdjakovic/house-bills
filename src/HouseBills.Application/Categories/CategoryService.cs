using HouseBills.Application.Common;
using HouseBills.Application.Persistence;
using HouseBills.Application.Resources;
using HouseBills.Domain;

namespace HouseBills.Application.Categories;

internal sealed class CategoryService(ICategoryRepository repository) : ICategoryService
{
    public Task<IReadOnlyList<CategoryDto>> ListAsync(CancellationToken cancellationToken)
    {
        return repository.ListAsync(cancellationToken);
    }

    public async Task<Result<int>> SaveAsync(SaveCategoryRequest request, CancellationToken cancellationToken)
    {
        var errors = new List<string>();
        FieldValidation.Text(errors, request.Name, Category.NameMaxLength, Messages.Field_Name, required: true);
        if (FieldValidation.ToError(errors) is { } validationError)
        {
            return validationError;
        }

        var name = request.Name.Trim();
        if (await repository.NameExistsAsync(name, request.Id, cancellationToken))
        {
            return Error.Validation(FieldValidation.Format(Messages.Category_NameExists, name));
        }

        if (request.Id is not { } id)
        {
            var category = new Category(name);
            await repository.AddAsync(category, cancellationToken);
            return category.Id;
        }

        var existing = await repository.GetAsync(id, cancellationToken);
        if (existing is null)
        {
            return Error.NotFound(Messages.Category_NotFound);
        }

        existing.Rename(name);
        var result = await repository.TryUpdateAsync(existing, request.RowVersion, cancellationToken);
        return result.IsSuccess ? id : result.Error!;
    }

    public async Task<Result> DeleteAsync(int id, byte[] rowVersion, CancellationToken cancellationToken)
    {
        if (await repository.IsInUseAsync(id, cancellationToken))
        {
            return Error.Validation(Messages.Category_InUse);
        }

        return await repository.TryDeleteAsync(id, rowVersion, cancellationToken);
    }
}