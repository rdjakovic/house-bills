using System.ComponentModel.DataAnnotations;

using CommunityToolkit.Mvvm.ComponentModel;

using HouseBills.Application.Payees;
using HouseBills.Domain;

namespace HouseBills.Wpf.ViewModels.Payees;

public sealed partial class PayeeEditorViewModel : EditorViewModel
{
    public PayeeEditorViewModel(PayeeDto? payee)
    {
        Id = payee?.Id;
        RowVersion = payee?.RowVersion;
        Name = payee?.Name ?? string.Empty;
        AccountReference = payee?.AccountReference;
        Notes = payee?.Notes;
        ResetValidation();
    }

    public int? Id { get; }

    public byte[]? RowVersion { get; }

    public override string Title => Id is null ? "New payee" : "Edit payee";

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required(ErrorMessage = "Name is required.")]
    [MaxLength(Payee.NameMaxLength)]
    public partial string Name { get; set; }

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [MaxLength(Payee.AccountReferenceMaxLength)]
    public partial string? AccountReference { get; set; }

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [MaxLength(Payee.NotesMaxLength)]
    public partial string? Notes { get; set; }

    public SavePayeeRequest ToRequest() => new(Id, Name, AccountReference, Notes, RowVersion);
}