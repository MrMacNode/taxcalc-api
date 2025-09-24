using Microsoft.Data.SqlClient;

namespace TaxcalcApi.Infrastructure.Database.Repositories;

/// <summary>
/// This factory can be used to abstract the acquisition of SQL connections.
/// - Our current implementation is a simple build from a connection string. 
///     - On a larger, more complex network where our app doesn't have direct, privileged access to the database as a factor VPCs, we could add more security to code and config.
/// - For more complex or demanding scenarios, we could implement connection pooling, logging, or other cross-cutting concerns here.
/// - This factory also makes it easier to mock or stub out database connections for testing.
/// </summary>
public interface ISqlConnectionFactory
{
    SqlConnection GetSqlConnection();
}

public class SqlConnectionFactory : ISqlConnectionFactory
{
    private readonly string _connectionString;

    public SqlConnectionFactory(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("Connection string must be provided.", nameof(connectionString));

        _connectionString = connectionString;
    }

    public SqlConnection GetSqlConnection()
    {
        return new SqlConnection(_connectionString);
    }
}
