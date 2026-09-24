using System.ComponentModel.DataAnnotations;

using CommunityToolkit.Mvvm.ComponentModel;

using HouseBills.Application.Bills;

namespace HouseBills.Wpf.ViewModels.Bills;

public sealed partial class PaymentEditorViewModel : EditorViewModel
{
    public PaymentEditorViewModel(BillListItem bill, DateOnly today)
    {
        Bill = bill;
        PaidOn = today;
        PaidAmount = bill.Amount;
        ResetValidation();
    }

    public BillListItem Bill { get; }

    public override string Title => "Record payment";

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required(ErrorMessage = "Payment date is required.")]
    public partial DateOnly? PaidOn { get; set; }

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required(ErrorMessage = "Amount is required.")]
    [Range(typeof(decimal), "0.01", "9999999999999999.99", ParseLimitsInInvariantCulture = true, ErrorMessage = "Amount must be greater than zero.")]
    public partial decimal? PaidAmount { get; set; }

    /// <summary>Builds the request; call only after <see cref="EditorViewModel.Validate"/> succeeded.</summary>
    public MarkBillPaidRequest ToRequest()
    {
        return new MarkBillPaidRequest(Bill.Id, PaidOn!.Value, PaidAmount!.Value, Bill.RowVersion);
    }
}