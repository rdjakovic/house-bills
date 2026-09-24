using HouseBills.Application.Common;
using HouseBills.Application.Reports;
using HouseBills.Wpf.Services;
using HouseBills.Wpf.ViewModels;

using Microsoft.Extensions.Logging.Abstractions;

using NSubstitute;

namespace HouseBills.Wpf.Tests;

public sealed class ReportsViewModelTests
{
    [Fact]
    public async Task OnNavigatedToAsync_CurrentYear_LoadsTotalsAndScalesBars()
    {
        var clock = Substitute.For<IClock>();
        clock.Today.Returns(new DateOnly(2026, 9, 24));
        var reports = Substitute.For<IReportQueries>();
        reports.GetMonthlySummaryAsync(2026, Arg.Any<CancellationToken>()).Returns(
            Enumerable.Range(1, 12)
                .Select(m => new MonthlySummaryRow(m, 1, m == 3 ? 200m : 50m, 50m, m == 3 ? 150m : 0m, 10m))
                .ToList());
        reports.GetCategoryTotalsAsync(new DateOnly(2026, 1, 1), new DateOnly(2026, 12, 31), Arg.Any<CancellationToken>())
            .Returns([new CategoryTotalRow("Utilities", 12, 750m, 600m)]);
        var viewModel = new ReportsViewModel(reports, clock, Substitute.For<IDialogService>(), NullLogger<ReportsViewModel>.Instance);

        await viewModel.OnNavigatedToAsync();

        viewModel.SelectedYear.ShouldBe(2026);
        viewModel.YearTotal.ShouldBe(750m);
        viewModel.YearOutstanding.ShouldBe(150m);
        viewModel.PreviousYearTotal.ShouldBe(120m);
        viewModel.Months[2].BarFraction.ShouldBe(1d);
        viewModel.Months[0].BarFraction.ShouldBe(0.25d);
        viewModel.Months[2].ChangeFromPreviousYear.ShouldBe(190m);
        viewModel.CategoryTotals.ShouldHaveSingleItem();
    }
}