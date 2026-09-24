using Dapper;

using HouseBills.Application.Common;
using HouseBills.Application.Persistence;
using HouseBills.Domain;
using HouseBills.Infrastructure.Persistence;

using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HouseBills.Infrastructure.Tests;

public sealed class LocalDbInitializerTests : IAsyncDisposable
{
    private const string LocalDbMaster = @"Server=(localdb)\MSSQLLocalDB;Database=master;Integrated Security=True;Connect Timeout=60";

    private readonly string _databaseName = $"HouseBillsTest_{Guid.NewGuid():N}";
    private readonly List<ServiceProvider> _providers = [];

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Theory]
    [InlineData(@"(localdb)\MSSQLLocalDB", true)]
    [InlineData(@"(LocalDB)\Custom", true)]
    [InlineData(@"localhost,1433", false)]
    [InlineData(@"SERVERPC\SQLEXPRESS", false)]
    [InlineData(null, false)]
    public void IsLocalDb_DataSource_DetectsLocalDbOnly(string? dataSource, bool expected)
    {
        LocalDbInitializer.IsLocalDb(dataSource).ShouldBe(expected);
    }

    [Fact]
    public async Task InitializeAsync_NewLocalDbDatabase_CreatesAndMigratesIt()
    {
        await SkipUnlessLocalDbAsync();
        var services = BuildServices();

        await services.GetRequiredService<IDatabaseInitializer>().InitializeAsync(Ct);
        await services.GetRequiredService<IDatabaseInitializer>().InitializeAsync(Ct);

        var categories = await services.GetRequiredService<ICategoryRepository>().ListAsync(Ct);
        categories.Count.ShouldBe(7);
    }

    [Fact]
    public async Task InitializeAsync_DatabaseDetachedButFilesRemain_ReattachesAndKeepsData()
    {
        await SkipUnlessLocalDbAsync();
        var services = BuildServices();
        var initializer = services.GetRequiredService<IDatabaseInitializer>();
        await initializer.InitializeAsync(Ct);
        await services.GetRequiredService<IPayeeRepository>().AddAsync(new Payee("Kept payee", null, null), Ct);
        await DetachAsync();

        await initializer.InitializeAsync(Ct);

        var payees = await services.GetRequiredService<IPayeeRepository>().ListAsync(Ct);
        payees.ShouldHaveSingleItem().Name.ShouldBe("Kept payee");
    }

    public async ValueTask DisposeAsync()
    {
        foreach (var provider in _providers)
        {
            await provider.DisposeAsync();
        }

        SqlConnection.ClearAllPools();
        if (!await IsLocalDbAvailableAsync())
        {
            return;
        }

        await using var master = new SqlConnection(LocalDbMaster);
        await master.ExecuteAsync(
            """
            IF DB_ID(@Name) IS NOT NULL
            BEGIN
                DECLARE @sql nvarchar(max) = N'ALTER DATABASE ' + QUOTENAME(@Name) + N' SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE ' + QUOTENAME(@Name) + N';';
                EXEC sys.sp_executesql @sql;
            END
            """,
            new { Name = _databaseName });

        // A failed test can leave the database detached; remove its (test-only) files so they don't accumulate.
        var dataPath = await master.ExecuteScalarAsync<string>("SELECT CAST(SERVERPROPERTY('InstanceDefaultDataPath') AS nvarchar(4000))");
        foreach (var file in new[] { $"{_databaseName}.mdf", $"{_databaseName}_log.ldf" }.Select(f => Path.Combine(dataPath!, f)))
        {
            File.Delete(file);
        }
    }

    private static async Task SkipUnlessLocalDbAsync()
    {
        Assert.SkipUnless(await IsLocalDbAvailableAsync(), "SQL Server LocalDB is not installed on this machine.");
    }

    private static async Task<bool> IsLocalDbAvailableAsync()
    {
        if (!OperatingSystem.IsWindows())
        {
            return false;
        }

        try
        {
            await using var connection = new SqlConnection(LocalDbMaster);
            await connection.OpenAsync();
            return true;
        }
        catch (SqlException)
        {
            return false;
        }
    }

    private ServiceProvider BuildServices()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                [$"ConnectionStrings:{DependencyInjection.ConnectionStringName}"] =
                    $@"Server=(localdb)\MSSQLLocalDB;Database={_databaseName};Integrated Security=True;Connect Timeout=60",
            })
            .Build();

        var services = new ServiceCollection().AddLogging().AddInfrastructure(configuration).BuildServiceProvider();
        _providers.Add(services);
        return services;
    }

    private async Task DetachAsync()
    {
        SqlConnection.ClearAllPools();
        await using var master = new SqlConnection(LocalDbMaster);
        await master.ExecuteAsync(
            """
            DECLARE @sql nvarchar(max) = N'ALTER DATABASE ' + QUOTENAME(@Name) + N' SET SINGLE_USER WITH ROLLBACK IMMEDIATE;';
            EXEC sys.sp_executesql @sql;
            EXEC sys.sp_detach_db @dbname = @Name;
            """,
            new { Name = _databaseName });
        (await master.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM sys.databases WHERE name = @Name", new { Name = _databaseName })).ShouldBe(0);
    }
}