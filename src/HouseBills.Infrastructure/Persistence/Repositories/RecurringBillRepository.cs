using HouseBills.Application.Persistence;
using HouseBills.Application.RecurringBills;
using HouseBills.Domain;

using Microsoft.EntityFrameworkCore;

namespace HouseBills.Infrastructure.Persistence.Repositories;

internal sealed class RecurringBillRepository(IDbContextFactory<AppDbContext> contextFactory)
    : EfRepository<RecurringBill>(contextFactory), IRecurringBillRepository
{
    public async Task<IReadOnlyList<RecurringBillDto>> ListAsync(CancellationToken cancellationToken)
    {
        await using var db = await ContextFactory.CreateDbContextAsync(cancellationToken);
        return await (
                from r in db.RecurringBills.AsNoTracking()
                join p in db.Payees on r.PayeeId equals p.Id
                join c in db.Categories on r.CategoryId equals c.Id
                orderby r.Name
                select new RecurringBillDto(
                    r.Id,
                    r.Name,
                    r.PayeeId,
                    p.Name,
                    r.CategoryId,
                    c.Name,
                    r.Amount,
                    r.Frequency,
                    r.StartDate,
                    r.EndDate,
                    r.Notes,
                    r.IsActive,
                    r.GeneratedThrough,
                    r.RowVersion))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<RecurringBill>> ListActiveAsync(CancellationToken cancellationToken)
    {
        await using var db = await ContextFactory.CreateDbContextAsync(cancellationToken);
        return await db.RecurringBills.AsNoTracking().Where(r => r.IsActive).ToListAsync(cancellationToken);
    }

    public async Task SaveGeneratedBillsAsync(RecurringBill template, byte[] expectedRowVersion, IReadOnlyList<Bill> bills, CancellationToken cancellationToken)
    {
        await using var db = await ContextFactory.CreateDbContextAsync(cancellationToken);
        AttachModified(db, template, expectedRowVersion);
        db.Bills.AddRange(bills);

        // SaveChanges wraps the template update and the inserts in one transaction.
        await SaveChangesAsync(db, cancellationToken);
    }
}