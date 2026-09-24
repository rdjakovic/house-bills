using System.Collections.ObjectModel;
using System.Globalization;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using HouseBills.Application.Common;
using HouseBills.Application.Reports;
using HouseBills.Presentation.Resources;
using HouseBills.Wpf.Localization;
using HouseBills.Wpf.Services;
using HouseBills.Wpf.ViewModels.Reports;

using Microsoft.Extensions.Logging;

namespace HouseBills.Wpf.ViewModels;

public sealed partial class ReportsViewModel : PageViewModel
{
    private const int YearsBack = 5;

    private readonly IReportQueries _reports;

    public ReportsViewModel(IReportQueries reports, IClock clock, IDialogService dialogs, ILogger<ReportsViewModel> logger)
        : base(dialogs, logger)
    {
        _reports = reports;
        var currentYear = clock.Today.Year;
        Years = Enumerable.Range(currentYear - YearsBack, YearsBack + 2).Reverse().ToList();
        SelectedYear = currentYear;
    }

    public override string Title => Strings.Page_Reports;

    public IReadOnlyList<int> Years { get; }

    public ObservableCollection<MonthlySummaryItem> Months { get; } = [];

    public ObservableCollection<CategoryTotalRow> CategoryTotals { get; } = [];

    [ObservableProperty]
    public partial int SelectedYear { get; set; }

    [ObservableProperty]
    public partial decimal YearTotal { get; set; }

    [ObservableProperty]
    public partial decimal YearPaid { get; set; }

    [ObservableProperty]
    public partial decimal YearOutstanding { get; set; }

    [ObservableProperty]
    public partial decimal PreviousYearTotal { get; set; }

    public override Task OnNavigatedToAsync()
    {
        return RefreshAsync(CancellationToken.None);
    }

    [RelayCommand]
    private Task RefreshAsync(CancellationToken cancellationToken)
    {
        return RunAsync(() => LoadAsync(SelectedYear, cancellationToken), Strings.Reports_LoadFailed);
    }

    private async Task LoadAsync(int year, CancellationToken cancellationToken)
    {
        var months = await _reports.GetMonthlySummaryAsync(year, cancellationToken);
        var categories = await _reports.GetCategoryTotalsAsync(new DateOnly(year, 1, 1), new DateOnly(year, 12, 31), cancellationToken);

        var max = months.Count == 0 ? 0m : months.Max(m => m.TotalAmount);
        // Month names follow the UI language (Serbian names are lowercase, so capitalize for the table).
        var uiCulture = LocalizedStrings.Culture;
        var monthNames = uiCulture.DateTimeFormat;
        Months.Clear();
        foreach (var month in months)
        {
            var fraction = max == 0m ? 0d : (double)(month.TotalAmount / max);
            var monthName = monthNames.GetMonthName(month.Month);
            Months.Add(new MonthlySummaryItem(uiCulture.TextInfo.ToUpper(monthName[0]) + monthName[1..], month, fraction));
        }

        CategoryTotals.Clear();
        foreach (var category in categories)
        {
            CategoryTotals.Add(category);
        }

        YearTotal = months.Sum(m => m.TotalAmount);
        YearPaid = months.Sum(m => m.PaidAmount);
        YearOutstanding = months.Sum(m => m.OutstandingAmount);
        PreviousYearTotal = months.Sum(m => m.PreviousYearTotalAmount);
    }
}