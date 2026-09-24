using System.Collections.ObjectModel;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using HouseBills.Application.Categories;
using HouseBills.Wpf.Services;
using HouseBills.Wpf.ViewModels.Categories;

using Microsoft.Extensions.Logging;

namespace HouseBills.Wpf.ViewModels;

public sealed partial class CategoriesViewModel(ICategoryService categories, IDialogService dialogs, ILogger<CategoriesViewModel> logger)
    : PageViewModel(dialogs, logger)
{
    public override string Title => "Categories";

    public ObservableCollection<CategoryDto> Items { get; } = [];

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(EditCommand), nameof(DeleteCommand))]
    public partial CategoryDto? SelectedItem { get; set; }

    [ObservableProperty]
    public partial CategoryEditorViewModel? Editor { get; set; }

    public override Task OnNavigatedToAsync()
    {
        return RunAsync(() => LoadAsync(CancellationToken.None), "Could not load categories.");
    }

    [RelayCommand]
    private void New()
    {
        Editor = new CategoryEditorViewModel(null);
    }

    [RelayCommand(CanExecute = nameof(HasSelection))]
    private void Edit()
    {
        Editor = new CategoryEditorViewModel(SelectedItem);
    }

    [RelayCommand]
    private async Task SaveAsync(CancellationToken cancellationToken)
    {
        if (Editor is not { } editor)
        {
            return;
        }

        if (await SubmitAsync(editor, async () => await categories.SaveAsync(editor.ToRequest(), cancellationToken), () => LoadAsync(cancellationToken)))
        {
            Editor = null;
        }
    }

    [RelayCommand]
    private void Cancel()
    {
        Editor = null;
    }

    [RelayCommand(CanExecute = nameof(HasSelection))]
    private async Task DeleteAsync(CancellationToken cancellationToken)
    {
        if (SelectedItem is not { } item || !Dialogs.Confirm("Delete category", $"Delete '{item.Name}'?"))
        {
            return;
        }

        Editor = null;
        await ExecuteAndReloadAsync(() => categories.DeleteAsync(item.Id, item.RowVersion, cancellationToken), () => LoadAsync(cancellationToken), "Could not delete the category.");
    }

    private bool HasSelection() => SelectedItem is not null;

    private async Task LoadAsync(CancellationToken cancellationToken)
    {
        var items = await categories.ListAsync(cancellationToken);
        Items.Clear();
        foreach (var item in items)
        {
            Items.Add(item);
        }
    }
}