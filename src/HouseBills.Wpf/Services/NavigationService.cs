using HouseBills.Wpf.ViewModels;

using Microsoft.Extensions.DependencyInjection;

namespace HouseBills.Wpf.Services;

internal sealed class NavigationService(IServiceProvider services) : INavigationService
{
    public PageViewModel? CurrentPage { get; private set; }

    public event EventHandler? CurrentPageChanged;

    public async Task NavigateToAsync<TPage>()
        where TPage : PageViewModel
    {
        var page = services.GetRequiredService<TPage>();
        CurrentPage = page;
        CurrentPageChanged?.Invoke(this, EventArgs.Empty);
        await page.OnNavigatedToAsync();
    }
}