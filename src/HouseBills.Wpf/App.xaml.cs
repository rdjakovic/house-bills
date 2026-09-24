using System.Globalization;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Threading;

using HouseBills.Wpf.Hosting;
using HouseBills.Wpf.Views;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace HouseBills.Wpf;

public partial class App : System.Windows.Application
{
    private const string UnexpectedErrorMessage = "Something went wrong. The error has been logged; please try again.";

    private IHost? _host;

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        RegisterGlobalExceptionHandlers();

        // Make bindings (dates, amounts) format and parse using the user's regional settings instead of en-US.
        FrameworkElement.LanguageProperty.OverrideMetadata(
            typeof(FrameworkElement),
            new FrameworkPropertyMetadata(XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag)));

        try
        {
            _host = HostBuilderExtensions.CreateHost(e.Args);
            await _host.StartAsync();
        }
        catch (Exception ex) when (ex is InvalidOperationException or Microsoft.Extensions.Options.OptionsValidationException)
        {
            // The host (and therefore logging) is not available; report configuration errors directly.
            MessageBox.Show(
                $"HouseBills could not start because its configuration is invalid.{Environment.NewLine}{Environment.NewLine}{ex.Message}",
                "HouseBills",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            Shutdown(1);
            return;
        }

        var window = _host.Services.GetRequiredService<MainWindow>();
        MainWindow = window;
        window.Show();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _host?.Dispose();
        base.OnExit(e);
    }

    private void RegisterGlobalExceptionHandlers()
    {
        DispatcherUnhandledException += OnDispatcherUnhandledException;
        TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;
        AppDomain.CurrentDomain.UnhandledException += OnAppDomainUnhandledException;
    }

    private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        Logger?.LogError(e.Exception, "Unhandled exception on the UI thread.");
        MessageBox.Show(UnexpectedErrorMessage, "HouseBills", MessageBoxButton.OK, MessageBoxImage.Error);
        e.Handled = true;
    }

    private void OnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        Logger?.LogError(e.Exception, "Unobserved task exception.");
        e.SetObserved();
    }

    private void OnAppDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        Logger?.LogCritical(e.ExceptionObject as Exception, "Unhandled exception; the application will terminate.");
        MessageBox.Show(UnexpectedErrorMessage, "HouseBills", MessageBoxButton.OK, MessageBoxImage.Error);
    }

    private ILogger? Logger => _host?.Services.GetService<ILogger<App>>();
}