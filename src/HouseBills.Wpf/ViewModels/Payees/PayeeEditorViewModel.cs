using System.ComponentModel.DataAnnotations;

using CommunityToolkit.Mvvm.ComponentModel;

using HouseBills.Application.Payees;
using HouseBills.Domain;
using HouseBills.Presentation.Resources;

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

    public override string Title => Id is null ? Strings.Payees_EditorNewTitle : Strings.Payees_EditorEditTitle;

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required(ErrorMessageResourceType = typeof(Strings), ErrorMessageResourceName = nameof(Strings.Validation_NameRequired))]
    [MaxLength(Payee.NameMaxLength, ErrorMessageResourceType = typeof(Strings), ErrorMessageResourceName = nameof(Strings.Validation_MaxLength))]
    public partial string Name { get; set; }

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [MaxLength(Payee.AccountReferenceMaxLength, ErrorMessageResourceType = typeof(Strings), ErrorMessageResourceName = nameof(Strings.Validation_MaxLength))]
    public partial string? AccountReference { get; set; }

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [MaxLength(Payee.NotesMaxLength, ErrorMessageResourceType = typeof(Strings), ErrorMessageResourceName = nameof(Strings.Validation_MaxLength))]
    public partial string? Notes { get; set; }

    public SavePayeeRequest ToRequest() => new(Id, Name, AccountReference, Notes, RowVersion);
}