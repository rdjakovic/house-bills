namespace HouseBills.Application.RecurringBills;

/// <summary>Settings for bill generation, bound from the "Billing" configuration section.</summary>
public sealed class BillingOptions
{
    public const string SectionName = "Billing";
    public const int MaxLookaheadDays = 366;

    /// <summary>Generate bills from recurring templates up to this many days after today.</summary>
    public int GenerationLookaheadDays { get; set; } = 31;
}