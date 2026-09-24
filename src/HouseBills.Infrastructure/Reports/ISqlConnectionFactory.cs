using Microsoft.Data.SqlClient;

namespace HouseBills.Infrastructure.Reports;

/// <summary>Creates (unopened) connections for Dapper queries.</summary>
internal interface ISqlConnectionFactory
{
    SqlConnection Create();
}