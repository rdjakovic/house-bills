namespace HouseBills.Wpf.Hosting;

/// <summary>Settings for the daily log files, bound from the "FileLogging" configuration section.</summary>
internal sealed class FileLoggingOptions
{
    public const string SectionName = "FileLogging";
    public const int MaxRetainedDays = 3650;

    /// <summary>Folder for log files. Environment variables such as <c>%LOCALAPPDATA%</c> are expanded.</summary>
    public string Directory { get; set; } = @"%LOCALAPPDATA%\HouseBills\Logs";

    /// <summary>Log files are deleted once they are older than this many days.</summary>
    public int RetainedDays { get; set; } = 30;

    public string ResolveDirectory() => Environment.ExpandEnvironmentVariables(Directory);

    /// <summary>Returns a description of invalid settings, or <c>null</c> if they are valid.</summary>
    public string? Validate()
    {
        if (string.IsNullOrWhiteSpace(Directory))
        {
            return $"{SectionName}:Directory is required.";
        }

        return RetainedDays is < 1 or > MaxRetainedDays
            ? $"{SectionName}:RetainedDays must be between 1 and {MaxRetainedDays}."
            : null;
    }
}