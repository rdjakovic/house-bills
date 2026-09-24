using HouseBills.Application.Bills;
using HouseBills.Application.Persistence;
using HouseBills.Domain;

using Microsoft.EntityFrameworkCore;

namespace HouseBills.Infrastructure.Persistence.Repositories;

internal sealed class BillRepository(IDbContextFactory<AppDbContext> contextFactory)
    : EfRepository<Bill>(contextFactory), IBillRepository
{
    public async Task<IReadOnlyList<BillListItem>> ListAsync(BillFilter filter, DateOnly today, CancellationToken cancellationToken)
    {
        await using var db = await ContextFactory.CreateDbContextAsync(cancellationToken);
        var bills = db.Bills.AsNoTracking();

        if (filter.DueFrom is { } from)
        {
            bills = bills.Where(b => b.DueDate >= from);
        }

        if (filter.DueTo is { } to)
        {
            bills = bills.Where(b => b.DueDate <= to);
        }

        if (filter.CategoryId is { } categoryId)
        {
            bills = bills.Where(b => b.CategoryId == categoryId);
        }

        if (filter.PayeeId is { } payeeId)
        {
            bills = bills.Where(b => b.PayeeId == payeeId);
        }

        bills = filter.Status switch
        {
            BillStatusFilter.Unpaid => bills.Where(b => b.PaidOn == null),
            BillStatusFilter.Overdue => bills.Where(b => b.PaidOn == null && b.DueDate < today),
            BillStatusFilter.Paid => bills.Where(b => b.PaidOn != null),
            _ => bills,
        };

        return await (
                from b in bills
                join p in db.Payees on b.PayeeId equals p.Id
                join c in db.Categories on b.CategoryId equals c.Id
                orderby b.DueDate, b.Description
                select new BillListItem(
                    b.Id,
                    b.Description,
                    b.PayeeId,
                    p.Name,
                    b.CategoryId,
                    c.Name,
                    b.Amount,
                    b.DueDate,
                    b.PaidOn,
                    b.PaidAmount,
                    b.Notes,
                    b.RecurringBillId,
                    b.RowVersion))
            .ToListAsync(cancellationToken);
    }
}