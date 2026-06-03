using AssetGuard.Core.Common;
using AssetGuard.Core.DTOs;
using AssetGuard.Core.Models;

namespace AssetGuard.Core.Interfaces;

public interface IDeviceRepository
{
    Task<PagedResult<Device>> GetPagedAsync(SearchFilter filter);
    Task<Device?> GetByIdAsync(int id);
    Task<int> CreateAsync(Device device);
    Task UpdateAsync(Device device);
    Task DeleteAsync(int id);
    Task<bool> AssetTagExistsAsync(string assetTag, int? excludeId = null);
    Task<bool> HasActiveAssignmentAsync(int deviceId);
    Task<IReadOnlyList<Device>> GetAvailableAsync();
    Task<IReadOnlyList<ChartDataPoint>> GetCountByTypeAsync();
    Task<IReadOnlyList<ChartDataPoint>> GetCountByStatusAsync();
}

public interface IEmployeeRepository
{
    Task<PagedResult<Employee>> GetPagedAsync(SearchFilter filter);
    Task<Employee?> GetByIdAsync(int id);
    Task<int> CreateAsync(Employee employee);
    Task UpdateAsync(Employee employee);
    Task DeactivateAsync(int id);
    Task<IReadOnlyList<Employee>> GetActiveAsync();
    Task<int> GetActiveCountAsync();
    Task<IReadOnlyList<string>> GetDepartmentsAsync();
}

public interface IAssignmentRepository
{
    Task<PagedResult<Assignment>> GetPagedAsync(SearchFilter filter);
    Task<Assignment?> GetByIdAsync(int id);
    Task<int> CreateAsync(Assignment assignment);
    Task UpdateAsync(Assignment assignment);
    Task ReturnAsync(int id, DateTime returnDate, string? returnNote);
    Task<int> GetActiveCountAsync();
    Task<IReadOnlyList<Assignment>> GetByDeviceIdAsync(int deviceId);
    Task<IReadOnlyList<Assignment>> GetByEmployeeIdAsync(int employeeId);
    Task<IReadOnlyList<ChartDataPoint>> GetCountByDepartmentAsync();
    Task<IReadOnlyList<MonthlyTrendPoint>> GetMonthlyAssignmentTrendsAsync(int months = 6);
}

public interface IMaintenanceRepository
{
    Task<PagedResult<MaintenanceRecord>> GetPagedAsync(SearchFilter filter);
    Task<MaintenanceRecord?> GetByIdAsync(int id);
    Task<int> CreateAsync(MaintenanceRecord record);
    Task UpdateAsync(MaintenanceRecord record);
    Task DeleteAsync(int id);
    Task<decimal> GetTotalCostAsync();
    Task<int> GetUpcomingCountAsync(int daysAhead = 30);
    Task<IReadOnlyList<MaintenanceRecord>> GetUpcomingAsync(int daysAhead = 30);
    Task<IReadOnlyList<MaintenanceRecord>> GetByDeviceIdAsync(int deviceId);
    Task<IReadOnlyList<MonthlyCostPoint>> GetMonthlyCostsAsync(int months = 6);
}

public interface IWarrantyRepository
{
    Task<PagedResult<Warranty>> GetPagedAsync(SearchFilter filter);
    Task<Warranty?> GetByIdAsync(int id);
    Task<int> CreateAsync(Warranty warranty);
    Task UpdateAsync(Warranty warranty);
    Task DeleteAsync(int id);
    Task<int> GetExpiringCountAsync(int daysAhead = 30);
    Task<int> GetExpiredCountAsync();
    Task<WarrantySummaryCounts> GetSummaryCountsAsync();
    Task<IReadOnlyList<WarrantyReportItem>> GetExpiringAsync(int daysAhead = 90);
    Task<IReadOnlyList<Warranty>> GetByDeviceIdAsync(int deviceId);
    Task<IReadOnlyList<MonthlyTrendPoint>> GetExpirationTrendsAsync(int months = 6);
}

public interface IAuditLogRepository
{
    Task<PagedResult<AuditLog>> GetPagedAsync(SearchFilter filter);
    Task<IReadOnlyList<AuditLog>> GetAllForExportAsync(SearchFilter filter);
    Task<IReadOnlyList<AuditLog>> GetForDeviceAsync(int deviceId, int limit = 50);
    Task<IReadOnlyList<AuditLog>> GetForEmployeeAsync(int employeeId, int limit = 50);
    Task<long> CreateAsync(AuditLog log);
}

public interface IUserRepository
{
    Task<User?> GetByUserNameAsync(string userName);
    Task<User?> GetByIdAsync(int id);
    Task<IReadOnlyList<User>> GetAllAsync();
    Task<int> CreateAsync(User user);
    Task DeactivateAsync(int id);
    Task<bool> UserNameExistsAsync(string userName, int? excludeId = null);
    Task UpdateLastLoginAsync(int userId);
}
