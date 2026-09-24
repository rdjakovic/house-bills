using HouseBills.Application.Categories;
using HouseBills.Application.Persistence;
using HouseBills.Domain;

using Microsoft.EntityFrameworkCore;

namespace HouseBills.Infrastructure.Persistence.Repositories;

internal sealed class CategoryRepository(IDbContextFactory<AppDbContext> contextFactory)
    : EfRepository<Category>(contextFactory), ICategoryRepository
{
    public async Task<IReadOnlyList<CategoryDto>> ListAsync(CancellationToken cancellationToken)
    {
        await using var db = await ContextFactory.CreateDbContextAsync(cancellationToken);
        return await db.Categories
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .Select(c => new CategoryDto(c.Id, c.Name, c.RowVersion))
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> NameExistsAsync(string name, int? excludeId, CancellationToken cancellationToken)
    {
        await using var db = await ContextFactory.CreateDbContextAsync(cancellationToken);
        return await db.Categories.AnyAsync(c => c.Name == name && c.Id != excludeId, cancellationToken);
    }

    public async Task<bool> IsInUseAsync(int id, CancellationToken cancellationToken)
    {
        await using var db = await ContextFactory.CreateDbContextAsync(cancellationToken);
        return await db.Bills.AnyAsync(b => b.CategoryId == id, cancellationToken)
            || await db.RecurringBills.AnyAsync(r => r.CategoryId == id, cancellationToken);
    }
}