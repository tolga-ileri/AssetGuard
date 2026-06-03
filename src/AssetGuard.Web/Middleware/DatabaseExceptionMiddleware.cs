using Microsoft.Data.SqlClient;

namespace AssetGuard.Web.Middleware;

public class DatabaseExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<DatabaseExceptionMiddleware> _logger;
    private readonly IHostEnvironment _environment;

    public DatabaseExceptionMiddleware(
        RequestDelegate next,
        ILogger<DatabaseExceptionMiddleware> logger,
        IHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex,
                "SQL Server connection failed. Check that SQL Server is running and " +
                "ConnectionStrings:DefaultConnection (User Id, Password, Server, Database) is correct.");

            context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
            context.Response.ContentType = "text/plain; charset=utf-8";

            var message = _environment.IsDevelopment()
                ? $"Database connection failed: {ex.Message}{Environment.NewLine}{Environment.NewLine}" +
                  "LocalDB: run sqllocaldb start MSSQLLocalDB, then Database\\Scripts\\DatabaseSetup.sql. " +
                  "Check ConnectionStrings:DefaultConnection in appsettings.Development.json."
                : "The application cannot connect to the database. Please contact the administrator.";

            await context.Response.WriteAsync(message);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("Connection string", StringComparison.OrdinalIgnoreCase)
            || ex.Message.Contains("Windows authentication", StringComparison.OrdinalIgnoreCase)
            || ex.Message.Contains("Cannot connect to SQL Server", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogError(ex, "Database configuration error.");

            context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
            context.Response.ContentType = "text/plain; charset=utf-8";
            await context.Response.WriteAsync(ex.Message);
        }
    }
}
