namespace HouseBills.Application.Common;

/// <summary>Abstracts the current date so business rules are testable.</summary>
public interface IClock
{
    /// <summary>Today's date in the user's local time zone.</summary>
    DateOnly Today { get; }
}