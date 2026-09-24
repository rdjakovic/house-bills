using Dapper;

using HouseBills.Application.Common;

using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HouseBills.Infrastructure.Persistence;

/// <summary>
/// Creates and upgrades the database at startup, but only for SQL Server LocalDB: a LocalDB database is private to one
/// Windows user, so there is no shared schema to protect. Any other server is left to deployment scripts (AGENTS.md §6).
/// </summary>
internal sealed class LocalDbInitializer(IDbContextFactory<AppDbContext> contextFactory, ILogger<LocalDbInitializer> logger)
    : IDatabaseInitializer
{
    private const string DatabaseExistsSql = "SELECT COUNT(*) FROM sys.databases WHERE name = @Name;";
    private const string DefaultDataPathSql = "SELECT CAST(SERVERPROPERTY('InstanceDefaultDataPath') AS nvarchar(4000));";

    // Identifiers and file names can't be parameters in CREATE DATABASE; they are quoted server-side.
    private const string AttachSql = """
        DECLARE @sql nvarchar(max) =
            N'CREATE DATABASE ' + QUOTENAME(@Name)
            + N' ON (FILENAME = N''' + REPLACE(@Mdf, N'''', N'''''') + N''')'
            + CASE WHEN @Ldf IS NULL THEN N'' ELSE N', (FILENAME = N''' + REPLACE(@Ldf, N'''', N'''''') + N''')' END
            + N' FOR ATTACH;';
        EXEC sys.sp_executesql @sql;
        """;

    /// <summary>
    /// Waits before each retry. LocalDB sometimes fails to start its instance ("error 50 … SQL Server process failed
    /// to start"), e.g. right after another process using it was killed; a later attempt then succeeds.
    /// </summary>
    internal IReadOnlyList<TimeSpan> RetryDelays { get; init; } = [TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(5)];

    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
        await using var db = await contextFactory.CreateDbContextAsync(cancellationToken);
        var connection = new SqlConnectionStringBuilder(db.Database.GetConnectionString());
        if (!IsLocalDb(connection.DataSource))
        {
            logger.LogInformation("Database server is not LocalDB; the schema is managed by deployment scripts.");
            return;
        }

        // Every step below is idempotent (check, re-attach, migrate), so retrying a failed attempt is safe.
        for (var attempt = 0; ; attempt++)
        {
            try
            {
                await PrepareAsync(db, connection, cancellationToken);
                return;
            }
            catch (SqlException ex) when (attempt < RetryDelays.Count)
            {
                logger.LogWarning(ex, "Could not prepare the LocalDB database (attempt {Attempt}); retrying in {Delay}.", attempt + 1, RetryDelays[attempt]);
                await Task.Delay(RetryDelays[attempt], cancellationToken);
            }
        }
    }

    internal static bool IsLocalDb(string? dataSource)
    {
        return dataSource?.TrimStart().StartsWith(@"(localdb)\", StringComparison.OrdinalIgnoreCase) == true;
    }

    private async Task PrepareAsync(AppDbContext db, SqlConnectionStringBuilder connection, CancellationToken cancellationToken)
    {
        await ReattachIfDetachedAsync(connection, cancellationToken);

        var pending = (await db.Database.GetPendingMigrationsAsync(cancellationToken)).ToList();
        if (pending.Count == 0)
        {
            return;
        }

        logger.LogInformation("Applying {MigrationCount} migration(s) to the LocalDB database: {Migrations}", pending.Count, pending);
        await db.Database.MigrateAsync(cancellationToken);
    }

    /// <summary>
    /// LocalDB can lose track of a database while its files remain in the instance's data folder. Re-attaching keeps the
    /// user's data; otherwise migrating would fail because CREATE DATABASE finds the files already present.
    /// </summary>
    private async Task ReattachIfDetachedAsync(SqlConnectionStringBuilder target, CancellationToken cancellationToken)
    {
        var databaseName = target.InitialCatalog;
        if (string.IsNullOrEmpty(databaseName))
        {
            return;
        }

        var masterConnectionString = new SqlConnectionStringBuilder(target.ConnectionString) { InitialCatalog = "master" }.ConnectionString;
        await using var master = new SqlConnection(masterConnectionString);
        await master.OpenAsync(cancellationToken);

        var exists = await master.ExecuteScalarAsync<int>(
            new CommandDefinition(DatabaseExistsSql, new { Name = databaseName }, cancellationToken: cancellationToken));
        if (exists > 0)
        {
            return;
        }

        var dataPath = await master.ExecuteScalarAsync<string?>(new CommandDefinition(DefaultDataPathSql, cancellationToken: cancellationToken));
        if (string.IsNullOrEmpty(dataPath))
        {
            return;
        }

        var mdf = Path.Combine(dataPath, databaseName + ".mdf");
        if (!File.Exists(mdf))
        {
            return;
        }

        var ldf = Path.Combine(dataPath, databaseName + "_log.ldf");
        logger.LogWarning("LocalDB database {Database} was not registered but its data file exists; re-attaching it.", databaseName);
        await master.ExecuteAsync(new CommandDefinition(
            AttachSql,
            new { Name = databaseName, Mdf = mdf, Ldf = File.Exists(ldf) ? ldf : null },
            cancellationToken: cancellationToken));
    }
}