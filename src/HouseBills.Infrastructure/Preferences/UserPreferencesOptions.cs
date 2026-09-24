namespace HouseBills.Infrastructure.Preferences;

/// <summary>Where <see cref="JsonUserPreferencesStore"/> keeps the preferences file.</summary>
public sealed class UserPreferencesOptions
{
    /// <summary>Preferences file path; environment variables such as <c>%LOCALAPPDATA%</c> are expanded.</summary>
    public string FilePath { get; set; } = @"%LOCALAPPDATA%\HouseBills\preferences.json";
}