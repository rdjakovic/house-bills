namespace HouseBills.Application.Preferences;

/// <summary>Per-user application preferences.</summary>
/// <param name="Language">Chosen UI language as a culture name (e.g. "en", "sr-Latn-RS"), or <c>null</c> for the default.</param>
public sealed record UserPreferences(string? Language)
{
    public static UserPreferences Default { get; } = new((string?)null);
}