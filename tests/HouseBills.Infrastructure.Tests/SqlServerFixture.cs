using HouseBills.Application;
using HouseBills.Application.Common;
using HouseBills.Infrastructure.Persistence;

using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using NSubstitute;

using Testcontainers.MsSql;

namespace HouseBills.Infrastructure.Tests;

/// <summary>
/// Runs SQL Server in Docker, applies the real migrations, and wires Application + Infrastructure exactly as the app does
/// (except for a fixed clock). Shared by all tests in <see cref="SqlServerCollection"/>; tests use unique names/dates
/// so they don't interfere with each other.
/// </summary>
public sealed class SqlServerFixture : IAsyncLifetime
{
    public static readonly DateOnly Today = new(2026, 9, 24);

    private readonly MsSqlContainer _container = new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-latest").Build();

    public IServiceProvider Services { get; private set; } = null!;

    public async ValueTask InitializeAsync()
    {
        await _container.StartAsync();
        var connectionString = new SqlConnectionStringBuilder(_container.GetConnectionString())
        {
            InitialCatalog = "HouseBillsTests",
        }.ConnectionString;

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                [$"ConnectionStrings:{DependencyInjection.ConnectionStringName}"] = connectionString,
            })
            .Build();

        var clock = Substitute.For<IClock>();
        clock.Today.Returns(Today);

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddApplication();
        services.AddInfrastructure(configuration);
        services.Replace(ServiceDescriptor.Singleton(clock));
        Services = services.BuildServiceProvider();

        await using var db = await Services.GetRequiredService<IDbContextFactory<AppDbContext>>().CreateDbContextAsync();
        await db.Database.MigrateAsync();
    }

    public async ValueTask DisposeAsync()
    {
        await _container.DisposeAsync();
    }

    public T Get<T>()
        where T : notnull => Services.GetRequiredService<T>();
}