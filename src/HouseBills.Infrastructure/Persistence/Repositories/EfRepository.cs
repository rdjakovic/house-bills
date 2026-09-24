using HouseBills.Application.Common;
using HouseBills.Application.Persistence;
using HouseBills.Domain;

using Microsoft.EntityFrameworkCore;

namespace HouseBills.Infrastructure.Persistence.Repositories;

/// <summary>
/// Common EF Core persistence. Each operation uses its own short-lived <see cref="AppDbContext"/>;
/// loaded aggregates are detached and re-attached on update with the caller's expected row version.
/// </summary>
internal abstract class EfRepository<T>(IDbContextFactory<AppDbContext> contextFactory) : IRepository<T>
    where T : Entity
{
    protected IDbContextFactory<AppDbContext> ContextFactory { get; } = contextFactory;

    public async Task<T?> GetAsync(int id, CancellationToken cancellationToken)
    {
        await using var db = await ContextFactory.CreateDbContextAsync(cancellationToken);
        return await db.Set<T>().AsNoTracking().SingleOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public async Task<bool> ExistsAsync(int id, CancellationToken cancellationToken)
    {
        await using var db = await ContextFactory.CreateDbContextAsync(cancellationToken);
        return await db.Set<T>().AnyAsync(e => e.Id == id, cancellationToken);
    }

    public async Task AddAsync(T entity, CancellationToken cancellationToken)
    {
        await using var db = await ContextFactory.CreateDbContextAsync(cancellationToken);
        db.Set<T>().Add(entity);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(T entity, byte[] expectedRowVersion, CancellationToken cancellationToken)
    {
        await using var db = await ContextFactory.CreateDbContextAsync(cancellationToken);
        AttachModified(db, entity, expectedRowVersion);
        await SaveChangesAsync(db, cancellationToken);
    }

    public async Task DeleteAsync(int id, byte[] expectedRowVersion, CancellationToken cancellationToken)
    {
        await using var db = await ContextFactory.CreateDbContextAsync(cancellationToken);
        var deleted = await db.Set<T>()
            .Where(e => e.Id == id && e.RowVersion == expectedRowVersion)
            .ExecuteDeleteAsync(cancellationToken);
        if (deleted == 0)
        {
            throw new ConcurrencyConflictException($"{typeof(T).Name} {id} was changed or deleted.");
        }
    }

    protected static void AttachModified<TEntity>(AppDbContext db, TEntity entity, byte[] expectedRowVersion)
        where TEntity : Entity
    {
        var entry = db.Set<TEntity>().Update(entity);
        entry.Property(e => e.RowVersion).OriginalValue = expectedRowVersion;
    }

    protected static async Task SaveChangesAsync(AppDbContext db, CancellationToken cancellationToken)
    {
        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            throw new ConcurrencyConflictException("The row was changed or deleted since it was loaded.", ex);
        }
    }
}