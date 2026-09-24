using HouseBills.Application.Common;
using HouseBills.Application.Persistence;
using HouseBills.Application.Reports;
using HouseBills.Infrastructure.Persistence;
using HouseBills.Infrastructure.Persistence.Repositories;
using HouseBills.Infrastructure.Reports;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HouseBills.Infrastructure;

public static class DependencyInjection
{
    public const string ConnectionStringName = "HouseBills";

    /// <summary>Registers direct SQL Server persistence (EF Core + Dapper) for the Application interfaces.</summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = GetRequiredConnectionString(configuration);

        services.AddDbContextFactory<AppDbContext>(options => options.UseSqlServer(connectionString));
        services.AddSingleton<ISqlConnectionFactory>(new SqlConnectionFactory(connectionString));

        services.AddSingleton(TimeProvider.System);
        services.AddSingleton<IClock, SystemClock>();
        services.AddSingleton<ICategoryRepository, CategoryRepository>();
        services.AddSingleton<IPayeeRepository, PayeeRepository>();
        services.AddSingleton<IRecurringBillRepository, RecurringBillRepository>();
        services.AddSingleton<IBillRepository, BillRepository>();
        services.AddSingleton<IReportQueries, ReportQueries>();

        return services;
    }

    internal static string GetRequiredConnectionString(IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString(ConnectionStringName);
        return string.IsNullOrWhiteSpace(connectionString)
            ? throw new InvalidOperationException($"Connection string '{ConnectionStringName}' is not configured.")
            : connectionString;
    }
}