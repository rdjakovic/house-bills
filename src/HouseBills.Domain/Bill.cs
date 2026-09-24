namespace HouseBills.Domain;

/// <summary>A single bill that is due on a specific date, optionally generated from a <see cref="RecurringBill"/>.</summary>
public sealed class Bill : Entity
{
    public const int DescriptionMaxLength = 150;
    public const int NotesMaxLength = 500;

    /// <summary>An unpaid bill due within this many days (inclusive) counts as <see cref="BillStatus.DueSoon"/>.</summary>
    public const int DueSoonDays = 7;

    // Used by EF Core when materializing.
    private Bill()
    {
        Description = string.Empty;
    }

    public Bill(string description, int payeeId, int categoryId, decimal amount, DateOnly dueDate, string? notes)
    {
        Description = Guard.RequiredText(description, DescriptionMaxLength, nameof(description));
        PayeeId = Guard.Id(payeeId, nameof(payeeId));
        CategoryId = Guard.Id(categoryId, nameof(categoryId));
        Amount = Guard.Money(amount, nameof(amount));
        DueDate = dueDate;
        Notes = Guard.OptionalText(notes, NotesMaxLength, nameof(notes));
    }

    public string Description { get; private set; }

    public int PayeeId { get; private set; }

    public int CategoryId { get; private set; }

    public decimal Amount { get; private set; }

    public DateOnly DueDate { get; private set; }

    public string? Notes { get; private set; }

    public DateOnly? PaidOn { get; private set; }

    public decimal? PaidAmount { get; private set; }

    /// <summary>The template this bill was generated from, if any.</summary>
    public int? RecurringBillId { get; private set; }

    public bool IsPaid => PaidOn.HasValue;

    public static BillStatus DetermineStatus(DateOnly dueDate, DateOnly? paidOn, DateOnly today)
    {
        if (paidOn.HasValue)
        {
            return BillStatus.Paid;
        }

        if (dueDate < today)
        {
            return BillStatus.Overdue;
        }

        return dueDate <= today.AddDays(DueSoonDays) ? BillStatus.DueSoon : BillStatus.Upcoming;
    }

    internal static Bill FromRecurring(RecurringBill template, DateOnly dueDate)
    {
        return new Bill(template.Name, template.PayeeId, template.CategoryId, template.Amount, dueDate, template.Notes)
        {
            RecurringBillId = template.Id,
        };
    }

    public BillStatus GetStatus(DateOnly today) => DetermineStatus(DueDate, PaidOn, today);

    public void Update(string description, int payeeId, int categoryId, decimal amount, DateOnly dueDate, string? notes)
    {
        Description = Guard.RequiredText(description, DescriptionMaxLength, nameof(description));
        PayeeId = Guard.Id(payeeId, nameof(payeeId));
        CategoryId = Guard.Id(categoryId, nameof(categoryId));
        Amount = Guard.Money(amount, nameof(amount));
        DueDate = dueDate;
        Notes = Guard.OptionalText(notes, NotesMaxLength, nameof(notes));
    }

    public void MarkPaid(DateOnly paidOn, decimal paidAmount)
    {
        PaidAmount = Guard.Money(paidAmount, nameof(paidAmount));
        PaidOn = paidOn;
    }

    public void MarkUnpaid()
    {
        PaidOn = null;
        PaidAmount = null;
    }
}