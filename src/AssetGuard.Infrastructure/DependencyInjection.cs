using Microsoft.Extensions.DependencyInjection;
using AssetGuard.Core.Interfaces;
using AssetGuard.Infrastructure.Repositories;

namespace AssetGuard.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<IDbConnectionFactory, Data.SqlConnectionFactory>();
        services.AddScoped<IDapperExecutor, Data.DapperExecutor>();
        services.AddScoped<IPasswordHasher, Security.BcryptPasswordHasher>();
        services.AddScoped<IDeviceRepository, DeviceRepository>();
        services.AddScoped<IEmployeeRepository, EmployeeRepository>();
        services.AddScoped<IAssignmentRepository, AssignmentRepository>();
        services.AddScoped<IMaintenanceRepository, MaintenanceRepository>();
        services.AddScoped<IWarrantyRepository, WarrantyRepository>();
        services.AddScoped<IAuditLogRepository, AuditLogRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IReportRepository, ReportRepository>();
        return services;
    }
}
