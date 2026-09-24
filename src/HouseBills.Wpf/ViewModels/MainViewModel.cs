using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

using HouseBills.Presentation.Resources;
using HouseBills.Wpf.Localization;
using HouseBills.Wpf.Services;

namespace HouseBills.Wpf.ViewModels;

public sealed partial class MainViewModel : ObservableObject, IRecipient<LanguageChangedMessage>
{
    private readonly INavigationService _navigation;

    public MainViewModel(INavigationService navigation, IMessenger messenger)
    {
        _navigation = navigation;
        _navigation.CurrentPageChanged += (_, _) => CurrentPage = _navigation.CurrentPage;
        Items =
        [
            new NavigationItem(() => Strings.Page_Bills, n => n.NavigateToAsync<BillsViewModel>()),
            new NavigationItem(() => Strings.Page_RecurringBills, n => n.NavigateToAsync<RecurringBillsViewModel>()),
            new NavigationItem(() => Strings.Page_Payees, n => n.NavigateToAsync<PayeesViewModel>()),
            new NavigationItem(() => Strings.Page_Categories, n => n.NavigateToAsync<CategoriesViewModel>()),
            new NavigationItem(() => Strings.Page_Reports, n => n.NavigateToAsync<ReportsViewModel>()),
            new NavigationItem(() => Strings.Page_Settings, n => n.NavigateToAsync<SettingsViewModel>(), isFooter: true),
        ];
        messenger.RegisterAll(this);
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

    public void Receive(LanguageChangedMessage message)
    {
        foreach (var item in Items)
        {
            item.RefreshTitle();
        }
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