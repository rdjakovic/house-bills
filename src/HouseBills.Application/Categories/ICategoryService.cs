using HouseBills.Application.Common;

namespace HouseBills.Application.Categories;

/// <summary>Manages bill categories.</summary>
public interface ICategoryService
{
    /// <summary>All categories ordered by name.</summary>
    Task<IReadOnlyList<CategoryDto>> ListAsync(CancellationToken cancellationToken);

    /// <summary>Creates or updates a category. Names must be unique. Returns the category id.</summary>
    Task<Result<int>> SaveAsync(SaveCategoryRequest request, CancellationToken cancellationToken);

    /// <summary>Deletes a category that is not used by any bill or recurring bill.</summary>
    Task<Result> DeleteAsync(int id, byte[] rowVersion, CancellationToken cancellationToken);
}