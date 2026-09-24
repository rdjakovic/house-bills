using HouseBills.Application.Bills;
using HouseBills.Application.Common;
using HouseBills.Application.Persistence;
using HouseBills.Domain;

namespace HouseBills.Infrastructure.Tests;

[Collection(SqlServerCollection.Name)]
public sealed class RepositoryTests(SqlServerFixture fixture)
{
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task ListAsync_FreshDatabase_ContainsSeededCategories()
    {
        var categories = await fixture.Get<ICategoryRepository>().ListAsync(Ct);

        categories.Select(c => c.Name).ShouldContain("Utilities");
    }

    [Fact]
    public async Task UpdateAsync_StaleRowVersion_ThrowsConcurrencyConflict()
    {
        var repository = fixture.Get<IPayeeRepository>();
        var payee = new Payee(Unique("Water Co"), null, null);
        await repository.AddAsync(payee, Ct);
        var staleVersion = payee.RowVersion;

        var firstEdit = (await repository.GetAsync(payee.Id, Ct))!;
        firstEdit.Update(firstEdit.Name, "A-1", null);
        await repository.UpdateAsync(firstEdit, staleVersion, Ct);

        var secondEdit = (await repository.GetAsync(payee.Id, Ct))!;
        secondEdit.Update(secondEdit.Name, "B-2", null);
        await Should.ThrowAsync<ConcurrencyConflictException>(() => repository.UpdateAsync(secondEdit, staleVersion, Ct));
        (await repository.GetAsync(payee.Id, Ct))!.AccountReference.ShouldBe("A-1");
    }

    [Fact]
    public async Task DeleteAsync_StaleRowVersion_ThrowsConcurrencyConflictAndKeepsRow()
    {
        var repository = fixture.Get<ICategoryRepository>();
        var category = new Category(Unique("Garden"));
        await repository.AddAsync(category, Ct);

        await Should.ThrowAsync<ConcurrencyConflictException>(() => repository.DeleteAsync(category.Id, [1, 2, 3, 4, 5, 6, 7, 8], Ct));
        (await repository.ExistsAsync(category.Id, Ct)).ShouldBeTrue();

        await repository.DeleteAsync(category.Id, category.RowVersion, Ct);
        (await repository.ExistsAsync(category.Id, Ct)).ShouldBeFalse();
    }

    [Fact]
    public async Task ListAsync_OverdueFilter_ReturnsOnlyUnpaidPastDueBillsWithNames()
    {
        var payee = new Payee(Unique("Filter payee"), null, null);
        await fixture.Get<IPayeeRepository>().AddAsync(payee, Ct);
        var bills = fixture.Get<IBillRepository>();
        var overdue = new Bill("Overdue", payee.Id, 1, 10m, SqlServerFixture.Today.AddDays(-3), null);
        var paid = new Bill("Paid", payee.Id, 1, 10m, SqlServerFixture.Today.AddDays(-3), null);
        paid.MarkPaid(SqlServerFixture.Today, 10m);
        var upcoming = new Bill("Upcoming", payee.Id, 1, 10m, SqlServerFixture.Today.AddDays(3), null);
        foreach (var bill in new[] { overdue, paid, upcoming })
        {
            await bills.AddAsync(bill, Ct);
        }

        var result = await bills.ListAsync(new BillFilter(null, null, BillStatusFilter.Overdue, PayeeId: payee.Id), SqlServerFixture.Today, Ct);

        var item = result.ShouldHaveSingleItem();
        item.Description.ShouldBe("Overdue");
        item.PayeeName.ShouldBe(payee.Name);
        item.CategoryName.ShouldBe("Utilities");
    }

    [Fact]
    public async Task IsInUseAsync_PayeeWithBill_ReturnsTrue()
    {
        var payees = fixture.Get<IPayeeRepository>();
        var payee = new Payee(Unique("Used payee"), null, null);
        await payees.AddAsync(payee, Ct);
        await fixture.Get<IBillRepository>().AddAsync(new Bill("Gas", payee.Id, 1, 5m, SqlServerFixture.Today, null), Ct);

        (await payees.IsInUseAsync(payee.Id, Ct)).ShouldBeTrue();
    }

    private static string Unique(string name) => $"{name} {Guid.NewGuid():N}";
}