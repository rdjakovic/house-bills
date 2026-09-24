using HouseBills.Application.Payees;
using HouseBills.Application.Persistence;
using HouseBills.Domain;

using Microsoft.EntityFrameworkCore;

namespace HouseBills.Infrastructure.Persistence.Repositories;

internal sealed class PayeeRepository(IDbContextFactory<AppDbContext> contextFactory)
    : EfRepository<Payee>(contextFactory), IPayeeRepository
{
    public async Task<IReadOnlyList<PayeeDto>> ListAsync(CancellationToken cancellationToken)
    {
        await using var db = await ContextFactory.CreateDbContextAsync(cancellationToken);
        return await db.Payees
            .AsNoTracking()
            .OrderBy(p => p.Name)
            .Select(p => new PayeeDto(p.Id, p.Name, p.AccountReference, p.Notes, p.RowVersion))
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> NameExistsAsync(string name, int? excludeId, CancellationToken cancellationToken)
    {
        await using var db = await ContextFactory.CreateDbContextAsync(cancellationToken);
        return await db.Payees.AnyAsync(p => p.Name == name && p.Id != excludeId, cancellationToken);
    }

    public async Task<bool> IsInUseAsync(int id, CancellationToken cancellationToken)
    {
        await using var db = await ContextFactory.CreateDbContextAsync(cancellationToken);
        return await db.Bills.AnyAsync(b => b.PayeeId == id, cancellationToken)
            || await db.RecurringBills.AnyAsync(r => r.PayeeId == id, cancellationToken);
    }
}