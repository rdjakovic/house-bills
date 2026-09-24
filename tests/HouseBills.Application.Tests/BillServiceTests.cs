using HouseBills.Application.Bills;
using HouseBills.Application.Common;
using HouseBills.Application.Persistence;
using HouseBills.Domain;

using NSubstitute;

namespace HouseBills.Application.Tests;

public sealed class BillServiceTests
{
    private readonly IBillRepository _bills = Substitute.For<IBillRepository>();
    private readonly IPayeeRepository _payees = Substitute.For<IPayeeRepository>();
    private readonly ICategoryRepository _categories = Substitute.For<ICategoryRepository>();
    private readonly BillService _service;

    public BillServiceTests()
    {
        var clock = Substitute.For<IClock>();
        clock.Today.Returns(TestData.Today);
        _payees.ExistsAsync(1, Arg.Any<CancellationToken>()).Returns(true);
        _categories.ExistsAsync(1, Arg.Any<CancellationToken>()).Returns(true);
        _service = new BillService(_bills, _payees, _categories, clock);
    }

    [Fact]
    public async Task ListAsync_Always_PopulatesStatusRelativeToToday()
    {
        _bills.ListAsync(Arg.Any<BillFilter>(), TestData.Today, Arg.Any<CancellationToken>()).Returns([
            Item(1, TestData.Today.AddDays(-1), paidOn: null),
            Item(2, TestData.Today.AddDays(30), paidOn: null),
            Item(3, TestData.Today.AddDays(-5), paidOn: TestData.Today.AddDays(-6)),
        ]);

        var items = await _service.ListAsync(new BillFilter(null, null), TestContext.Current.CancellationToken);

        items.Select(i => i.Status).ShouldBe([BillStatus.Overdue, BillStatus.Upcoming, BillStatus.Paid]);
    }

    [Fact]
    public async Task SaveAsync_UnknownPayeeAndInvalidAmount_ReturnsAllValidationErrors()
    {
        var request = new SaveBillRequest(null, "Water", PayeeId: 99, CategoryId: 1, Amount: 0m, TestData.Today, null, null);

        var result = await _service.SaveAsync(request, TestContext.Current.CancellationToken);

        result.Error!.Kind.ShouldBe(ErrorKind.Validation);
        result.Error.Message.ShouldContain("payee");
        result.Error.Message.ShouldContain("Amount");
        await _bills.DidNotReceiveWithAnyArgs().AddAsync(default!, TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task SaveAsync_ValidNewBill_AddsBill()
    {
        var request = new SaveBillRequest(null, "Water", 1, 1, 42.10m, TestData.Today, "Q3", null);

        var result = await _service.SaveAsync(request, TestContext.Current.CancellationToken);

        result.IsSuccess.ShouldBeTrue();
        await _bills.Received(1).AddAsync(Arg.Is<Bill>(b => b.Amount == 42.10m && b.Notes == "Q3"), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task MarkPaidAsync_FutureDate_ReturnsValidationError()
    {
        var request = new MarkBillPaidRequest(1, TestData.Today.AddDays(1), 10m, TestData.RowVersion);

        var result = await _service.MarkPaidAsync(request, TestContext.Current.CancellationToken);

        result.Error!.Kind.ShouldBe(ErrorKind.Validation);
        await _bills.DidNotReceiveWithAnyArgs().UpdateAsync(default!, default!, TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task MarkPaidAsync_ValidPayment_UpdatesWithCallersRowVersion()
    {
        var bill = TestData.Persisted(new Bill("Water", 1, 1, 42m, TestData.Today, null), 7);
        _bills.GetAsync(7, Arg.Any<CancellationToken>()).Returns(bill);
        byte[] callersVersion = [9, 9, 9, 9, 9, 9, 9, 9];

        var result = await _service.MarkPaidAsync(new MarkBillPaidRequest(7, TestData.Today, 40m, callersVersion), TestContext.Current.CancellationToken);

        result.IsSuccess.ShouldBeTrue();
        await _bills.Received(1).UpdateAsync(
            Arg.Is<Bill>(b => b.PaidOn == TestData.Today && b.PaidAmount == 40m),
            callersVersion,
            Arg.Any<CancellationToken>());
    }

    private static BillListItem Item(int id, DateOnly dueDate, DateOnly? paidOn) =>
        new(id, $"Bill {id}", 1, "Payee", 1, "Category", 10m, dueDate, paidOn, paidOn is null ? null : 10m, null, null, TestData.RowVersion);
}