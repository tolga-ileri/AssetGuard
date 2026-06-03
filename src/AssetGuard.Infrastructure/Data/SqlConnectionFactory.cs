using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using AssetGuard.Core.Interfaces;

namespace AssetGuard.Infrastructure.Data;

public class SqlConnectionFactory : IDbConnectionFactory
{
    private static readonly HashSet<string> AllowedWindowsAuthServers = new(StringComparer.OrdinalIgnoreCase)
    {
        @"(localdb)\MSSQLLocalDB",
        @"localhost\SQLEXPRESS"
    };

    private readonly string _connectionString;

    public SqlConnectionFactory(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "Connection string 'DefaultConnection' not found. " +
                "Add ConnectionStrings:DefaultConnection to appsettings.json or appsettings.Development.json.");

        var builder = new SqlConnectionStringBuilder(_connectionString);

        if (builder.IntegratedSecurity && !IsAllowedWindowsAuthServer(builder.DataSource))
        {
            throw new InvalidOperationException(
                $"Windows authentication (Trusted_Connection) is only allowed for LocalDB or SQL Express. " +
                $"Server '{builder.DataSource}' must be (localdb)\\MSSQLLocalDB or localhost\\SQLEXPRESS. " +
                "For other servers, use SQL authentication: User Id=sa;Password=... in appsettings.json.");
        }
    }

    public IDbConnection CreateConnection()
    {
        try
        {
            return new SqlConnection(_connectionString);
        }
        catch (ArgumentException ex)
        {
            throw new InvalidOperationException(
                "Invalid SQL connection string. Verify ConnectionStrings:DefaultConnection format.", ex);
        }
    }

    internal static bool IsAllowedWindowsAuthServer(string dataSource) =>
        AllowedWindowsAuthServers.Contains(dataSource.Trim());
}
