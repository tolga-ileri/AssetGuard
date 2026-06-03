using AssetGuard.Application.Services;
using AssetGuard.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace AssetGuard.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IAuditService, AuditService>();
        services.AddScoped<DashboardService>();
        services.AddScoped<DeviceService>();
        services.AddScoped<EmployeeService>();
        services.AddScoped<AssignmentService>();
        services.AddScoped<MaintenanceService>();
        services.AddScoped<WarrantyService>();
        services.AddScoped<ReportService>();
        services.AddScoped<AuthService>();
        services.AddScoped<AuditLogService>();
        services.AddScoped<UserService>();
        services.AddScoped<TimelineService>();
        return services;
    }
}
