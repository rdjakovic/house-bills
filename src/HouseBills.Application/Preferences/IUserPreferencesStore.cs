namespace HouseBills.Application.Preferences;

/// <summary>Loads and saves the current Windows user's <see cref="UserPreferences"/>.</summary>
public interface IUserPreferencesStore
{
    /// <summary>Returns the saved preferences, or <see cref="UserPreferences.Default"/> if none are saved or they can't be read.</summary>
    Task<UserPreferences> LoadAsync(CancellationToken cancellationToken);

    /// <summary>Saves the preferences, replacing any previously saved ones.</summary>
    Task SaveAsync(UserPreferences preferences, CancellationToken cancellationToken);
}