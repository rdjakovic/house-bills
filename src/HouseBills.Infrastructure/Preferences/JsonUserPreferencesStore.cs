using System.Text.Json;

using HouseBills.Application.Preferences;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HouseBills.Infrastructure.Preferences;

/// <summary>Stores preferences as a small JSON file in the user's local app data folder.</summary>
internal sealed class JsonUserPreferencesStore(IOptions<UserPreferencesOptions> options, ILogger<JsonUserPreferencesStore> logger)
    : IUserPreferencesStore
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    private string FilePath => Environment.ExpandEnvironmentVariables(options.Value.FilePath);

    public async Task<UserPreferences> LoadAsync(CancellationToken cancellationToken)
    {
        if (!File.Exists(FilePath))
        {
            return UserPreferences.Default;
        }

        try
        {
            await using var stream = File.OpenRead(FilePath);
            return await JsonSerializer.DeserializeAsync<UserPreferences>(stream, JsonOptions, cancellationToken) ?? UserPreferences.Default;
        }
        catch (Exception ex) when (ex is JsonException or IOException or UnauthorizedAccessException)
        {
            // A damaged preferences file must not stop the app from starting; defaults are fine.
            logger.LogWarning(ex, "Could not read the preferences file; using default preferences.");
            return UserPreferences.Default;
        }
    }

    public async Task SaveAsync(UserPreferences preferences, CancellationToken cancellationToken)
    {
        var path = FilePath;
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);

        // Write to a temporary file first so a crash mid-write can't leave a half-written preferences file.
        var temporaryPath = path + ".tmp";
        await using (var stream = File.Create(temporaryPath))
        {
            await JsonSerializer.SerializeAsync(stream, preferences, JsonOptions, cancellationToken);
        }

        File.Move(temporaryPath, path, overwrite: true);
    }
}