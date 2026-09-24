using CommunityToolkit.Mvvm.ComponentModel;

using HouseBills.Application.Common;
using HouseBills.Wpf.Services;

using Microsoft.Extensions.Logging;

namespace HouseBills.Wpf.ViewModels;

/// <summary>Base for pages shown in the main window.</summary>
public abstract partial class PageViewModel(IDialogService dialogs, ILogger logger) : ObservableObject
{
    public abstract string Title { get; }

    [ObservableProperty]
    public partial bool IsBusy { get; set; }

    protected IDialogService Dialogs { get; } = dialogs;

    /// <summary>Called each time the page is shown; reloads data so changes made on other pages are visible.</summary>
    public abstract Task OnNavigatedToAsync();

    /// <summary>
    /// Runs I/O while showing the busy state. Unexpected failures are logged and reported with a friendly message.
    /// Returns <c>false</c> if the action failed.
    /// </summary>
    protected async Task<bool> RunAsync(Func<Task> action, string failureMessage)
    {
        IsBusy = true;
        try
        {
            await action();
            return true;
        }
        catch (OperationCanceledException)
        {
            logger.LogDebug("Operation cancelled: {Operation}", failureMessage);
            return false;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Operation failed: {Operation}", failureMessage);
            Dialogs.ShowError($"{failureMessage} Check the connection to the database and try again.");
            return false;
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// Validates and submits an editor. Business errors are shown in the editor; after a successful save or a
    /// conflict the list is reloaded. Returns <c>true</c> on success.
    /// </summary>
    protected async Task<bool> SubmitAsync(EditorViewModel editor, Func<Task<Result>> submit, Func<Task> reload)
    {
        editor.ErrorMessage = null;
        if (!editor.Validate())
        {
            return false;
        }

        Result? result = null;
        var completed = await RunAsync(
            async () =>
            {
                result = await submit();
                if (result.Error?.Kind is not ErrorKind.Validation)
                {
                    await reload();
                }
            },
            "Could not save changes.");

        if (!completed || result is null)
        {
            return false;
        }

        editor.ErrorMessage = result.Error?.Message;
        return result.IsSuccess;
    }

    /// <summary>Runs a list action (delete, mark paid...), shows any business error, then reloads.</summary>
    protected Task<bool> ExecuteAndReloadAsync(Func<Task<Result>> action, Func<Task> reload, string failureMessage)
    {
        return RunAsync(
            async () =>
            {
                var result = await action();
                if (!result.IsSuccess)
                {
                    Dialogs.ShowError(result.Error!.Message);
                }

                await reload();
            },
            failureMessage);
    }
}