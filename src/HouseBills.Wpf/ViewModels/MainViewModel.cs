using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using HouseBills.Wpf.Services;

namespace HouseBills.Wpf.ViewModels;

public sealed partial class MainViewModel : ObservableObject
{
    private readonly INavigationService _navigation;

    public MainViewModel(INavigationService navigation)
    {
        _navigation = navigation;
        _navigation.CurrentPageChanged += (_, _) => CurrentPage = _navigation.CurrentPage;
        Items =
        [
            new NavigationItem("Bills", n => n.NavigateToAsync<BillsViewModel>()),
            new NavigationItem("Recurring bills", n => n.NavigateToAsync<RecurringBillsViewModel>()),
            new NavigationItem("Payees", n => n.NavigateToAsync<PayeesViewModel>()),
            new NavigationItem("Categories", n => n.NavigateToAsync<CategoriesViewModel>()),
            new NavigationItem("Reports", n => n.NavigateToAsync<ReportsViewModel>()),
        ];
    }

    public IReadOnlyList<NavigationItem> Items { get; }

    [ObservableProperty]
    public partial NavigationItem? SelectedItem { get; set; }

    [ObservableProperty]
    public partial PageViewModel? CurrentPage { get; set; }

    /// <summary>Opens the first page. Called once the window is shown.</summary>
    public void Start()
    {
        SelectedItem ??= Items[0];
    }

    partial void OnSelectedItemChanged(NavigationItem? value)
    {
        if (value is not null)
        {
            NavigateCommand.Execute(value);
        }
    }

    [RelayCommand]
    private Task NavigateAsync(NavigationItem item) => item.NavigateAsync(_navigation);
}