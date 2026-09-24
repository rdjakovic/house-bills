using System.Globalization;

using HouseBills.Wpf.Hosting;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace HouseBills.Wpf.Tests;

public sealed class FileLoggingTests : IDisposable
{
    private readonly string _directory = Path.Combine(Path.GetTempPath(), "HouseBillsLogTests", Guid.NewGuid().ToString("N"));

    public void Dispose()
    {
        if (Directory.Exists(_directory))
        {
            Directory.Delete(_directory, recursive: true);
        }
    }

    [Theory]
    [InlineData(0)]
    [InlineData(FileLoggingOptions.MaxRetainedDays + 1)]
    public void Validate_RetainedDaysOutOfRange_ReturnsError(int days)
    {
        new FileLoggingOptions { RetainedDays = days }.Validate().ShouldNotBeNull();
    }

    [Fact]
    public void Validate_Defaults_IsValid()
    {
        new FileLoggingOptions().Validate().ShouldBeNull();
    }

    [Fact]
    public void ResolveDirectory_EnvironmentVariable_IsExpanded()
    {
        var options = new FileLoggingOptions { Directory = @"%LOCALAPPDATA%\HouseBills\Logs" };

        options.ResolveDirectory().ShouldBe(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "HouseBills", "Logs"));
    }

    [Fact]
    public void AddDailyFileLogging_InvalidSettings_Throws()
    {
        Should.Throw<InvalidOperationException>(() => BuildServices(retainedDays: 0));
    }

    [Fact]
    public void AddDailyFileLogging_Log_WritesTodaysFileAndDeletesFilesPastRetention()
    {
        Directory.CreateDirectory(_directory);
        var today = DateTime.Now.Date;
        var expired = CreateLogFile(today.AddDays(-10));
        var retained = CreateLogFile(today.AddDays(-3));

        using (var services = BuildServices(retainedDays: 5))
        {
            services.GetRequiredService<ILogger<FileLoggingTests>>().LogInformation("Hello {Name}", "log file");
        }

        var todaysFile = Path.Combine(_directory, FileName(today));
        File.ReadAllText(todaysFile).ShouldContain("[INF] HouseBills.Wpf.Tests.FileLoggingTests: Hello log file");
        File.Exists(expired).ShouldBeFalse();
        File.Exists(retained).ShouldBeTrue();
    }

    [Fact]
    public void AddDailyFileLogging_LogLevelConfiguration_IsHonouredByTheFile()
    {
        using (var services = BuildServices(retainedDays: 5))
        {
            var factory = services.GetRequiredService<ILoggerFactory>();
            var app = factory.CreateLogger("HouseBills.Sample");
            var noisy = factory.CreateLogger("Noisy.Component");
            app.LogDebug("app debug");
            app.LogInformation("app information");
            noisy.LogInformation("noisy information");
            noisy.LogWarning("noisy warning");
        }

        var content = File.ReadAllText(Path.Combine(_directory, FileName(DateTime.Now.Date)));
        content.ShouldContain("app information");
        content.ShouldContain("noisy warning");
        content.ShouldNotContain("app debug");
        content.ShouldNotContain("noisy information");
    }

    private ServiceProvider BuildServices(int retainedDays)
    {
        // Mirrors the host: Logging:LogLevel filters apply to all providers.
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                [$"{FileLoggingOptions.SectionName}:Directory"] = _directory,
                [$"{FileLoggingOptions.SectionName}:RetainedDays"] = retainedDays.ToString(CultureInfo.InvariantCulture),
                ["Logging:LogLevel:Default"] = "Information",
                ["Logging:LogLevel:Noisy"] = "Warning",
            })
            .Build();

        return new ServiceCollection()
            .AddLogging(logging => logging
                .AddConfiguration(configuration.GetSection("Logging"))
                .AddDailyFileLogging(configuration))
            .BuildServiceProvider();
    }

    private string CreateLogFile(DateTime date)
    {
        var path = Path.Combine(_directory, FileName(date));
        File.WriteAllText(path, "old entry");
        return path;
    }

    private static string FileName(DateTime date) => $"housebills-{date:yyyyMMdd}.log";
}