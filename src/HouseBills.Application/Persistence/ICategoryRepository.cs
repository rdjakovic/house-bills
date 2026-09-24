using HouseBills.Application.Categories;
using HouseBills.Domain;

namespace HouseBills.Application.Persistence;

/// <summary>Persistence for <see cref="Category"/>.</summary>
public interface ICategoryRepository : IRepository<Category>
{
    /// <summary>All categories ordered by name.</summary>
    Task<IReadOnlyList<CategoryDto>> ListAsync(CancellationToken cancellationToken);

    /// <summary>Whether another category (other than <paramref name="excludeId"/>) already uses <paramref name="name"/>.</summary>
    Task<bool> NameExistsAsync(string name, int? excludeId, CancellationToken cancellationToken);

    /// <summary>Whether any bill or recurring bill references the category.</summary>
    Task<bool> IsInUseAsync(int id, CancellationToken cancellationToken);
}