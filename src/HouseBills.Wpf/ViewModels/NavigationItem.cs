using CommunityToolkit.Mvvm.ComponentModel;

using HouseBills.Wpf.Services;

namespace HouseBills.Wpf.ViewModels;

/// <summary>An entry in the main navigation list; its title follows the UI language.</summary>
/// <param name="title">Returns the title in the current UI language.</param>
/// <param name="navigateAsync">Opens the page.</param>
/// <param name="isFooter">Shown at the bottom of the menu (e.g. Settings).</param>
public sealed class NavigationItem(Func<string> title, Func<INavigationService, Task> navigateAsync, bool isFooter = false) : ObservableObject
{
    public string Title => title();

    public Func<INavigationService, Task> NavigateAsync { get; } = navigateAsync;

    public bool IsFooter { get; } = isFooter;

    public void RefreshTitle() => OnPropertyChanged(nameof(Title));

    /// <summary>The visible title, which is also what screen readers announce for the list item.</summary>
    public override string ToString() => Title;
}