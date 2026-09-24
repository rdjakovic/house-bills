namespace HouseBills.Wpf.ViewModels;

public sealed record NavigationItem(string Title, Func<Services.INavigationService, Task> NavigateAsync);