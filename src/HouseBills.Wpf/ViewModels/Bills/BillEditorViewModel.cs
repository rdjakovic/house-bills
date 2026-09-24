using System.ComponentModel.DataAnnotations;

using CommunityToolkit.Mvvm.ComponentModel;

using HouseBills.Application.Bills;
using HouseBills.Application.Categories;
using HouseBills.Application.Payees;
using HouseBills.Domain;

namespace HouseBills.Wpf.ViewModels.Bills;

public sealed partial class BillEditorViewModel : EditorViewModel
{
    public BillEditorViewModel(BillListItem? bill, DateOnly defaultDueDate, IReadOnlyList<PayeeDto> payees, IReadOnlyList<CategoryDto> categories)
    {
        Id = bill?.Id;
        RowVersion = bill?.RowVersion;
        Payees = payees;
        Categories = categories;
        Description = bill?.Description ?? string.Empty;
        PayeeId = bill?.PayeeId;
        CategoryId = bill?.CategoryId;
        Amount = bill?.Amount;
        DueDate = bill?.DueDate ?? defaultDueDate;
        Notes = bill?.Notes;
        ResetValidation();
    }

    public int? Id { get; }

    public byte[]? RowVersion { get; }

    public IReadOnlyList<PayeeDto> Payees { get; }

    public IReadOnlyList<CategoryDto> Categories { get; }

    public override string Title => Id is null ? "New bill" : "Edit bill";

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required(ErrorMessage = "Description is required.")]
    [MaxLength(Bill.DescriptionMaxLength)]
    public partial string Description { get; set; }

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
    [NotifyDataErrorInfo]
    [Required(ErrorMessage = "Due date is required.")]
    public partial DateOnly? DueDate { get; set; }

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [MaxLength(Bill.NotesMaxLength)]
    public partial string? Notes { get; set; }

    /// <summary>Builds the request; call only after <see cref="EditorViewModel.Validate"/> succeeded.</summary>
    public SaveBillRequest ToRequest()
    {
        return new SaveBillRequest(Id, Description, PayeeId!.Value, CategoryId!.Value, Amount!.Value, DueDate!.Value, Notes, RowVersion);
    }
}