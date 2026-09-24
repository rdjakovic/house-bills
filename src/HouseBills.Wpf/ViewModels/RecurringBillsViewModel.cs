using System.Collections.ObjectModel;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using HouseBills.Application.Categories;
using HouseBills.Application.Common;
using HouseBills.Application.Payees;
using HouseBills.Application.RecurringBills;
using HouseBills.Presentation.Resources;
using HouseBills.Wpf.Localization;
using HouseBills.Wpf.Services;
using HouseBills.Wpf.ViewModels.RecurringBills;

using Microsoft.Extensions.Logging;

namespace HouseBills.Wpf.ViewModels;

public sealed partial class RecurringBillsViewModel : PageViewModel
{
    private readonly IRecurringBillService _recurringBills;
    private readonly IPayeeService _payees;
    private readonly ICategoryService _categories;
    private readonly IClock _clock;

    public RecurringBillsViewModel(
        IRecurringBillService recurringBills,
        IPayeeService payees,
        ICategoryService categories,
        IClock clock,
        IDialogService dialogs,
        ILogger<RecurringBillsViewModel> logger)
        : base(dialogs, logger)
    {
        _recurringBills = recurringBills;
        _payees = payees;
        _categories = categories;
        _clock = clock;
    }

    public override string Title => Strings.Page_RecurringBills;

    public ObservableCollection<RecurringBillDto> Items { get; } = [];

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(EditCommand), nameof(DeleteCommand), nameof(ToggleActiveCommand))]
    public partial RecurringBillDto? SelectedItem { get; set; }

    [ObservableProperty]
    public partial RecurringBillEditorViewModel? Editor { get; set; }

    private IReadOnlyList<PayeeDto> PayeeLookup { get; set; } = [];

    private IReadOnlyList<CategoryDto> CategoryLookup { get; set; } = [];

    public override Task OnNavigatedToAsync()
    {
        return RunAsync(
            async () =>
            {
                PayeeLookup = await _payees.ListAsync(CancellationToken.None);
                CategoryLookup = await _categories.ListAsync(CancellationToken.None);
                await LoadAsync(CancellationToken.None);
            },
            Strings.Recurring_LoadFailed);
    }

    [RelayCommand]
    private void New()
    {
        Editor = new RecurringBillEditorViewModel(null, _clock.Today, PayeeLookup, CategoryLookup);
    }

    [RelayCommand(CanExecute = nameof(HasSelection))]
    private void Edit()
    {
        Editor = new RecurringBillEditorViewModel(SelectedItem, _clock.Today, PayeeLookup, CategoryLookup);
    }

    [RelayCommand]
    private async Task SaveAsync(CancellationToken cancellationToken)
    {
        if (Editor is not { } editor)
        {
            return;
        }

        if (await SubmitAsync(editor, async () => await _recurringBills.SaveAsync(editor.ToRequest(), cancellationToken), () => LoadAsync(cancellationToken)))
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
    private Task ToggleActiveAsync(CancellationToken cancellationToken)
    {
        var item = SelectedItem!;
        return ExecuteAndReloadAsync(
            () => _recurringBills.SetActiveAsync(item.Id, !item.IsActive, item.RowVersion, cancellationToken),
            () => LoadAsync(cancellationToken),
            Strings.Recurring_UpdateFailed);
    }

    [RelayCommand(CanExecute = nameof(HasSelection))]
    private async Task DeleteAsync(CancellationToken cancellationToken)
    {
        if (SelectedItem is not { } item
            || !Dialogs.Confirm(Strings.Recurring_DeleteTitle, string.Format(LocalizedStrings.FormattingCulture, Strings.Recurring_DeleteConfirm, item.Name)))
        {
            return;
        }

        Editor = null;
        await ExecuteAndReloadAsync(() => _recurringBills.DeleteAsync(item.Id, item.RowVersion, cancellationToken), () => LoadAsync(cancellationToken), Strings.Recurring_DeleteFailed);
    }

    private bool HasSelection() => SelectedItem is not null;

    private async Task LoadAsync(CancellationToken cancellationToken)
    {
        var items = await _recurringBills.ListAsync(cancellationToken);
        Items.Clear();
        foreach (var item in items)
        {
            Items.Add(item);
        }
    }
}