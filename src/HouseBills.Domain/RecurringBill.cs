namespace HouseBills.Domain;

/// <summary>
/// A template that produces <see cref="Bill"/> instances on a <see cref="RecurrenceSchedule"/>,
/// e.g. "Electricity, monthly, starting 2026-01-15".
/// </summary>
public sealed class RecurringBill : Entity
{
    public const int NameMaxLength = Bill.DescriptionMaxLength;
    public const int NotesMaxLength = Bill.NotesMaxLength;

    // Used by EF Core when materializing.
    private RecurringBill()
    {
        Name = string.Empty;
    }

    public RecurringBill(string name, int payeeId, int categoryId, decimal amount, RecurrenceSchedule schedule, string? notes)
    {
        Name = Guard.RequiredText(name, NameMaxLength, nameof(name));
        PayeeId = Guard.Id(payeeId, nameof(payeeId));
        CategoryId = Guard.Id(categoryId, nameof(categoryId));
        Amount = Guard.Money(amount, nameof(amount));
        Frequency = schedule.Frequency;
        StartDate = schedule.StartDate;
        EndDate = schedule.EndDate;
        Notes = Guard.OptionalText(notes, NotesMaxLength, nameof(notes));
        IsActive = true;
    }

    public string Name { get; private set; }

    public int PayeeId { get; private set; }

    public int CategoryId { get; private set; }

    /// <summary>Expected amount for each generated bill.</summary>
    public decimal Amount { get; private set; }

    public BillFrequency Frequency { get; private set; }

    public DateOnly StartDate { get; private set; }

    public DateOnly? EndDate { get; private set; }

    public string? Notes { get; private set; }

    public bool IsActive { get; private set; }

    /// <summary>
    /// Bills have been generated for every due date up to and including this date. Generation resumes after it,
    /// so a generated bill the user deleted is not re-created.
    /// </summary>
    public DateOnly? GeneratedThrough { get; private set; }

    public RecurrenceSchedule Schedule => new(Frequency, StartDate, EndDate);

    public void Update(string name, int payeeId, int categoryId, decimal amount, RecurrenceSchedule schedule, string? notes)
    {
        Name = Guard.RequiredText(name, NameMaxLength, nameof(name));
        PayeeId = Guard.Id(payeeId, nameof(payeeId));
        CategoryId = Guard.Id(categoryId, nameof(categoryId));
        Amount = Guard.Money(amount, nameof(amount));
        Frequency = schedule.Frequency;
        StartDate = schedule.StartDate;
        EndDate = schedule.EndDate;
        Notes = Guard.OptionalText(notes, NotesMaxLength, nameof(notes));
    }

    /// <summary>
    /// Pauses or resumes generation. Resuming skips due dates before <paramref name="today"/> so that
    /// bills for the paused period are not back-filled.
    /// </summary>
    public void SetActive(bool isActive, DateOnly today)
    {
        if (isActive && !IsActive)
        {
            var yesterday = today.AddDays(-1);
            if (GeneratedThrough is null || GeneratedThrough < yesterday)
            {
                GeneratedThrough = yesterday;
            }
        }

        IsActive = isActive;
    }

    /// <summary>
    /// Creates bills for every due date after <see cref="GeneratedThrough"/> up to <paramref name="upTo"/> (inclusive)
    /// and advances <see cref="GeneratedThrough"/>. Inactive templates generate nothing.
    /// </summary>
    public IReadOnlyList<Bill> GenerateBills(DateOnly upTo)
    {
        if (!IsActive || (GeneratedThrough is { } through && through >= upTo))
        {
            return [];
        }

        var from = GeneratedThrough?.AddDays(1) ?? StartDate;
        var bills = Schedule.GetDueDates(from, upTo).Select(dueDate => Bill.FromRecurring(this, dueDate)).ToList();
        GeneratedThrough = upTo;
        return bills;
    }
}