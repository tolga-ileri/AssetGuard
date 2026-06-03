using AssetGuard.Application;
using AssetGuard.Core.Enums;
using AssetGuard.Infrastructure;
using AssetGuard.Web.Middleware;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Data.SqlClient;

if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")))
{
    Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development");
}

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor();
builder.Services.AddInfrastructure();
builder.Services.AddApplication();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole(UserRoles.Admin));
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

var app = builder.Build();

LogStartupConfiguration(app);
ValidateDatabaseConnection(app);

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
}

app.UseMiddleware<DatabaseExceptionMiddleware>();

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

static void LogStartupConfiguration(WebApplication app)
{
    var configuration = app.Services.GetRequiredService<IConfiguration>();
    var logger = app.Services.GetRequiredService<ILogger<Program>>();

    var environmentName = app.Environment.EnvironmentName;
    var connectionString = configuration.GetConnectionString("DefaultConnection");
    var devSettingsPath = Path.Combine(app.Environment.ContentRootPath, "appsettings.Development.json");

    logger.LogInformation("=== AssetGuard Startup Configuration ===");
    logger.LogInformation("ASPNETCORE_ENVIRONMENT: {Environment}", environmentName);
    logger.LogInformation("Content root: {ContentRoot}", app.Environment.ContentRootPath);
    logger.LogInformation("appsettings.Development.json present: {Present}", File.Exists(devSettingsPath));

    if (string.IsNullOrWhiteSpace(connectionString))
    {
        logger.LogWarning("DefaultConnection is not configured.");
    }
    else
    {
        logger.LogInformation("DefaultConnection: {ConnectionString}", MaskConnectionString(connectionString));
    }

    logger.LogInformation("========================================");
}

static void ValidateDatabaseConnection(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    var connectionString = configuration.GetConnectionString("DefaultConnection");
    if (string.IsNullOrWhiteSpace(connectionString))
    {
        throw new InvalidOperationException(
            "Connection string 'DefaultConnection' is missing. " +
            "Add it under ConnectionStrings in appsettings.Development.json.");
    }

    if (app.Environment.IsDevelopment()
        && !connectionString.Contains("(localdb)", StringComparison.OrdinalIgnoreCase))
    {
        throw new InvalidOperationException(
            "Development environment requires LocalDB. " +
            "ConnectionStrings:DefaultConnection must target (localdb)\\MSSQLLocalDB. " +
            $"Active environment: {app.Environment.EnvironmentName}. " +
            $"Resolved connection: {MaskConnectionString(connectionString)}. " +
            "Ensure ASPNETCORE_ENVIRONMENT=Development and appsettings.Development.json is loaded.");
    }

    var csb = new SqlConnectionStringBuilder(connectionString);

    if (!csb.IntegratedSecurity && connectionString.Contains("YOUR_PASSWORD_HERE", StringComparison.Ordinal))
    {
        throw new InvalidOperationException(
            "Replace YOUR_PASSWORD_HERE in ConnectionStrings:DefaultConnection " +
            "with your SQL Server sa password in appsettings.json.");
    }

    try
    {
        using var connection = new SqlConnection(connectionString);
        connection.Open();
        logger.LogInformation("Database connection verified successfully ({Server}, IntegratedSecurity={IntegratedSecurity}).",
            csb.DataSource, csb.IntegratedSecurity);
    }
    catch (SqlException ex)
    {
        logger.LogCritical(ex, "Cannot connect to SQL Server ({Server}).", csb.DataSource);

        var hint = csb.IntegratedSecurity
            ? "Ensure LocalDB is installed, start it with: sqllocaldb start MSSQLLocalDB. " +
              "Then run Database/Scripts/DatabaseSetup.sql to create AssetGuardDb."
            : "Ensure SQL Server is running, sa login is enabled, and AssetGuardDb exists.";

        throw new InvalidOperationException(
            $"Cannot connect to SQL Server ({csb.DataSource}). {hint}", ex);
    }
}

static string MaskConnectionString(string connectionString)
{
    var builder = new SqlConnectionStringBuilder(connectionString);
    if (!string.IsNullOrEmpty(builder.Password))
        builder.Password = "****";
    return builder.ConnectionString;
}
