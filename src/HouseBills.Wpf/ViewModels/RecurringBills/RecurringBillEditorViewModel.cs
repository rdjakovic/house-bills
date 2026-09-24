using System.ComponentModel.DataAnnotations;

using CommunityToolkit.Mvvm.ComponentModel;

using HouseBills.Application.Categories;
using HouseBills.Application.Payees;
using HouseBills.Application.RecurringBills;
using HouseBills.Domain;

namespace HouseBills.Wpf.ViewModels.RecurringBills;

public sealed partial class RecurringBillEditorViewModel : EditorViewModel
{
    public RecurringBillEditorViewModel(RecurringBillDto? template, DateOnly defaultStartDate, IReadOnlyList<PayeeDto> payees, IReadOnlyList<CategoryDto> categories)
    {
        Id = template?.Id;
        RowVersion = template?.RowVersion;
        Payees = payees;
        Categories = categories;
        Name = template?.Name ?? string.Empty;
        PayeeId = template?.PayeeId;
        CategoryId = template?.CategoryId;
        Amount = template?.Amount;
        Frequency = template?.Frequency ?? BillFrequency.Monthly;
        StartDate = template?.StartDate ?? defaultStartDate;
        EndDate = template?.EndDate;
        Notes = template?.Notes;
        ResetValidation();
    }

    public int? Id { get; }

    public byte[]? RowVersion { get; }

    public IReadOnlyList<PayeeDto> Payees { get; }

    public IReadOnlyList<CategoryDto> Categories { get; }

    public IReadOnlyList<BillFrequency> Frequencies { get; } = Enum.GetValues<BillFrequency>();

    public override string Title => Id is null ? "New recurring bill" : "Edit recurring bill";

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required(ErrorMessage = "Name is required.")]
    [MaxLength(RecurringBill.NameMaxLength)]
    public partial string Name { get; set; }

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required(ErrorMessage = "Select a payee.")]
    public partial int? PayeeId { get; set; }

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required(ErrorMessage = "Select a category.")]
    public partial int? CategoryId { get; set; }

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required(ErrorMessage = "Amount is required.")]
    [Range(typeof(decimal), "0.01", "9999999999999999.99", ParseLimitsInInvariantCulture = true, ErrorMessage = "Amount must be greater than zero.")]
    public partial decimal? Amount { get; set; }

    [ObservableProperty]
    public partial BillFrequency Frequency { get; set; }

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required(ErrorMessage = "First due date is required.")]
    public partial DateOnly? StartDate { get; set; }

    [ObservableProperty]
    public partial DateOnly? EndDate { get; set; }

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [MaxLength(RecurringBill.NotesMaxLength)]
    public partial string? Notes { get; set; }

    /// <summary>Builds the request; call only after <see cref="EditorViewModel.Validate"/> succeeded.</summary>
    public SaveRecurringBillRequest ToRequest()
    {
        return new SaveRecurringBillRequest(Id, Name, PayeeId!.Value, CategoryId!.Value, Amount!.Value, Frequency, StartDate!.Value, EndDate, Notes, RowVersion);
    }
}