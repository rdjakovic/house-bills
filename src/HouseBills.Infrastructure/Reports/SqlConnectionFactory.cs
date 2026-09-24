using Microsoft.Data.SqlClient;

namespace HouseBills.Infrastructure.Reports;

internal sealed class SqlConnectionFactory(string connectionString) : ISqlConnectionFactory
{
    public SqlConnection Create() => new(connectionString);
}