using System.Globalization;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Serilog;
using Serilog.Extensions.Logging;

namespace HouseBills.Wpf.Hosting;

internal static class FileLoggingExtensions
{
    // Serilog inserts the date before the extension: housebills-20260924.log
    private const string FileNamePattern = "housebills-.log";
    private const string OutputTemplate = "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}";

    /// <summary>
    /// Adds one log file per day alongside the default providers, deleting files older than
    /// <see cref="FileLoggingOptions.RetainedDays"/>. Levels are still controlled by the "Logging:LogLevel" section.
    /// </summary>
    /// <exception cref="InvalidOperationException">The "FileLogging" section is invalid.</exception>
    public static ILoggingBuilder AddDailyFileLogging(this ILoggingBuilder logging, IConfiguration configuration)
    {
        var options = configuration.GetSection(FileLoggingOptions.SectionName).Get<FileLoggingOptions>() ?? new FileLoggingOptions();
        if (options.Validate() is { } error)
        {
            throw new InvalidOperationException(error);
        }

        var logger = new LoggerConfiguration()
            .MinimumLevel.Verbose()
            .WriteTo.File(
                System.IO.Path.Combine(options.ResolveDirectory(), FileNamePattern),
                formatProvider: CultureInfo.InvariantCulture,
                outputTemplate: OutputTemplate,
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: null,
                retainedFileTimeLimit: TimeSpan.FromDays(options.RetainedDays),
                shared: true)
            .CreateLogger();

        // dispose: true flushes and closes the file when the host is disposed on exit.
        logging.AddSerilog(logger, dispose: true);

        // AddSerilog registers a provider-specific "allow everything" filter (Serilog normally filters itself), which
        // would bypass Logging:LogLevel:Default. Remove it so the file honours the same levels as the other providers.
        logging.Services.Configure<LoggerFilterOptions>(filters =>
        {
            foreach (var rule in filters.Rules.Where(r => r.ProviderName == typeof(SerilogLoggerProvider).FullName).ToList())
            {
                filters.Rules.Remove(rule);
            }
        });
        return logging;
    }
}