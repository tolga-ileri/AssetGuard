using AssetGuard.Core.Common;
using AssetGuard.Core.DTOs;
using AssetGuard.Core.Models;
using AssetGuard.Core.Enums;
using AssetGuard.Core.Interfaces;

namespace AssetGuard.Application.Services;

public class DashboardService
{
    private readonly IDeviceRepository _deviceRepository;
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IAssignmentRepository _assignmentRepository;
    private readonly IWarrantyRepository _warrantyRepository;
    private readonly IMaintenanceRepository _maintenanceRepository;

    public DashboardService(
        IDeviceRepository deviceRepository,
        IEmployeeRepository employeeRepository,
        IAssignmentRepository assignmentRepository,
        IWarrantyRepository warrantyRepository,
        IMaintenanceRepository maintenanceRepository)
    {
        _deviceRepository = deviceRepository;
        _employeeRepository = employeeRepository;
        _assignmentRepository = assignmentRepository;
        _warrantyRepository = warrantyRepository;
        _maintenanceRepository = maintenanceRepository;
    }

    public async Task<DashboardStats> GetStatsAsync()
    {
        var statusCounts = await _deviceRepository.GetCountByStatusAsync();
        return new DashboardStats
        {
            TotalDevices = statusCounts.Sum(x => x.Value),
            AvailableDevices = statusCounts.FirstOrDefault(x => x.Label == DeviceStatus.Available)?.Value ?? 0,
            AssignedDevices = statusCounts.FirstOrDefault(x => x.Label == DeviceStatus.Assigned)?.Value ?? 0,
            InMaintenanceDevices = statusCounts.FirstOrDefault(x => x.Label == DeviceStatus.InMaintenance)?.Value ?? 0,
            TotalEmployees = await _employeeRepository.GetActiveCountAsync(),
            ActiveAssignments = await _assignmentRepository.GetActiveCountAsync(),
            ExpiringWarranties = await _warrantyRepository.GetExpiringCountAsync(30),
            ExpiredWarranties = await _warrantyRepository.GetExpiredCountAsync(),
            UpcomingMaintenance = await _maintenanceRepository.GetUpcomingCountAsync(30),
            TotalMaintenanceCost = await _maintenanceRepository.GetTotalCostAsync()
        };
    }

    public async Task<(IReadOnlyList<ChartDataPoint> ByType, IReadOnlyList<ChartDataPoint> ByStatus)> GetChartDataAsync()
    {
        var byType = await _deviceRepository.GetCountByTypeAsync();
        var byStatus = await _deviceRepository.GetCountByStatusAsync();
        return (byType, byStatus);
    }

    public Task<IReadOnlyList<WarrantyReportItem>> GetExpiringWarrantiesAsync() =>
        _warrantyRepository.GetExpiringAsync(30);

    public Task<IReadOnlyList<MaintenanceRecord>> GetUpcomingMaintenanceAsync() =>
        _maintenanceRepository.GetUpcomingAsync(30);

    public async Task<DashboardAnalytics> GetAnalyticsAsync()
    {
        return new DashboardAnalytics
        {
            MonthlyMaintenanceCosts = await _maintenanceRepository.GetMonthlyCostsAsync(6),
            DevicesByDepartment = await _assignmentRepository.GetCountByDepartmentAsync(),
            AssignmentTrends = await _assignmentRepository.GetMonthlyAssignmentTrendsAsync(6),
            WarrantyExpirationTrends = await _warrantyRepository.GetExpirationTrendsAsync(6)
        };
    }
}

public class DeviceService
{
    private readonly IDeviceRepository _deviceRepository;
    private readonly IAssignmentRepository _assignmentRepository;
    private readonly IMaintenanceRepository _maintenanceRepository;
    private readonly IWarrantyRepository _warrantyRepository;
    private readonly IAuditService _auditService;

    public DeviceService(
        IDeviceRepository deviceRepository,
        IAssignmentRepository assignmentRepository,
        IMaintenanceRepository maintenanceRepository,
        IWarrantyRepository warrantyRepository,
        IAuditService auditService)
    {
        _deviceRepository = deviceRepository;
        _assignmentRepository = assignmentRepository;
        _maintenanceRepository = maintenanceRepository;
        _warrantyRepository = warrantyRepository;
        _auditService = auditService;
    }

    public Task<PagedResult<Device>> GetPagedAsync(SearchFilter filter) => _deviceRepository.GetPagedAsync(filter);
    public Task<Device?> GetByIdAsync(int id) => _deviceRepository.GetByIdAsync(id);
    public Task<IReadOnlyList<Device>> GetAvailableAsync() => _deviceRepository.GetAvailableAsync();

    public async Task<(bool Success, string? Error, int Id)> CreateAsync(Device device)
    {
        if (await _deviceRepository.AssetTagExistsAsync(device.AssetTag))
            return (false, "Asset tag already exists.", 0);

        var id = await _deviceRepository.CreateAsync(device);
        await _auditService.LogAsync(AuditActionTypes.Create, "Device", id, $"Created device {device.AssetTag}");
        return (true, null, id);
    }

    public async Task<(bool Success, string? Error)> UpdateAsync(Device device)
    {
        if (await _deviceRepository.AssetTagExistsAsync(device.AssetTag, device.Id))
            return (false, "Asset tag already exists.");

        await _deviceRepository.UpdateAsync(device);
        await _auditService.LogAsync(AuditActionTypes.Update, "Device", device.Id, $"Updated device {device.AssetTag}");
        return (true, null);
    }

    public async Task<(bool Success, string? Error)> DeleteAsync(int id)
    {
        if (await _deviceRepository.HasActiveAssignmentAsync(id))
            return (false, "Cannot delete a device with an active assignment. Return the device first.");

        var device = await _deviceRepository.GetByIdAsync(id);
        await _deviceRepository.DeleteAsync(id);
        if (device != null)
            await _auditService.LogAsync(AuditActionTypes.Delete, "Device", id, $"Deleted device {device.AssetTag}");
        return (true, null);
    }

    public Task<IReadOnlyList<Assignment>> GetAssignmentHistoryAsync(int deviceId) =>
        _assignmentRepository.GetByDeviceIdAsync(deviceId);

    public Task<IReadOnlyList<MaintenanceRecord>> GetMaintenanceHistoryAsync(int deviceId) =>
        _maintenanceRepository.GetByDeviceIdAsync(deviceId);

    public Task<IReadOnlyList<Warranty>> GetWarrantiesAsync(int deviceId) =>
        _warrantyRepository.GetByDeviceIdAsync(deviceId);
}

public class EmployeeService
{
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IAssignmentRepository _assignmentRepository;
    private readonly IAuditService _auditService;

    public EmployeeService(IEmployeeRepository employeeRepository, IAssignmentRepository assignmentRepository, IAuditService auditService)
    {
        _employeeRepository = employeeRepository;
        _assignmentRepository = assignmentRepository;
        _auditService = auditService;
    }

    public Task<PagedResult<Employee>> GetPagedAsync(SearchFilter filter) => _employeeRepository.GetPagedAsync(filter);
    public Task<Employee?> GetByIdAsync(int id) => _employeeRepository.GetByIdAsync(id);
    public Task<IReadOnlyList<Employee>> GetActiveAsync() => _employeeRepository.GetActiveAsync();

    public async Task<int> CreateAsync(Employee employee)
    {
        var id = await _employeeRepository.CreateAsync(employee);
        await _auditService.LogAsync(AuditActionTypes.Create, "Employee", id, $"Created employee {employee.FullName}");
        return id;
    }

    public async Task UpdateAsync(Employee employee)
    {
        await _employeeRepository.UpdateAsync(employee);
        await _auditService.LogAsync(AuditActionTypes.Update, "Employee", employee.Id, $"Updated employee {employee.FullName}");
    }

    public async Task DeactivateAsync(int id)
    {
        var employee = await _employeeRepository.GetByIdAsync(id);
        await _employeeRepository.DeactivateAsync(id);
        if (employee != null)
            await _auditService.LogAsync(AuditActionTypes.Deactivate, "Employee", id, $"Deactivated employee {employee.FullName}");
    }

    public Task<IReadOnlyList<string>> GetDepartmentsAsync() => _employeeRepository.GetDepartmentsAsync();

    public Task<IReadOnlyList<Assignment>> GetAssignmentHistoryAsync(int employeeId) =>
        _assignmentRepository.GetByEmployeeIdAsync(employeeId);
}

public class AssignmentService
{
    private readonly IAssignmentRepository _assignmentRepository;
    private readonly IDeviceRepository _deviceRepository;
    private readonly IAuditService _auditService;

    public AssignmentService(IAssignmentRepository assignmentRepository, IDeviceRepository deviceRepository, IAuditService auditService)
    {
        _assignmentRepository = assignmentRepository;
        _deviceRepository = deviceRepository;
        _auditService = auditService;
    }

    public Task<PagedResult<Assignment>> GetPagedAsync(SearchFilter filter) => _assignmentRepository.GetPagedAsync(filter);
    public Task<Assignment?> GetByIdAsync(int id) => _assignmentRepository.GetByIdAsync(id);
    public Task<IReadOnlyList<Assignment>> GetByDeviceIdAsync(int deviceId) => _assignmentRepository.GetByDeviceIdAsync(deviceId);
    public Task<IReadOnlyList<Assignment>> GetByEmployeeIdAsync(int employeeId) => _assignmentRepository.GetByEmployeeIdAsync(employeeId);

    public async Task<(bool Success, string? Error, int Id)> CreateAsync(Assignment assignment)
    {
        var device = await _deviceRepository.GetByIdAsync(assignment.DeviceId);
        if (device == null) return (false, "Device not found.", 0);
        if (device.Status != DeviceStatus.Available)
            return (false, "This device is already assigned or unavailable.", 0);
        if (await _deviceRepository.HasActiveAssignmentAsync(assignment.DeviceId))
            return (false, "This device already has an active assignment.", 0);

        assignment.Status = AssignmentStatus.Active;
        var id = await _assignmentRepository.CreateAsync(assignment);

        device.Status = DeviceStatus.Assigned;
        await _deviceRepository.UpdateAsync(device);
        await _auditService.LogAsync(AuditActionTypes.Assign, "Assignment", id, $"Assigned device {device.AssetTag} to employee ID {assignment.EmployeeId}");
        return (true, null, id);
    }

    public async Task<(bool Success, string? Error)> ReturnAsync(int id, DateTime returnDate, string? returnNote)
    {
        var assignment = await _assignmentRepository.GetByIdAsync(id);
        if (assignment == null) return (false, "Assignment not found.");
        if (assignment.Status == AssignmentStatus.Returned) return (false, "Assignment already returned.");

        await _assignmentRepository.ReturnAsync(id, returnDate, returnNote);

        var device = await _deviceRepository.GetByIdAsync(assignment.DeviceId);
        if (device != null)
        {
            device.Status = DeviceStatus.Available;
            await _deviceRepository.UpdateAsync(device);
        }

        await _auditService.LogAsync(AuditActionTypes.Return, "Assignment", id, $"Returned device {assignment.AssetTag}");
        return (true, null);
    }
}

public class MaintenanceService
{
    private readonly IMaintenanceRepository _maintenanceRepository;
    private readonly IDeviceRepository _deviceRepository;
    private readonly IAuditService _auditService;

    public MaintenanceService(IMaintenanceRepository maintenanceRepository, IDeviceRepository deviceRepository, IAuditService auditService)
    {
        _maintenanceRepository = maintenanceRepository;
        _deviceRepository = deviceRepository;
        _auditService = auditService;
    }

    public Task<PagedResult<MaintenanceRecord>> GetPagedAsync(SearchFilter filter) => _maintenanceRepository.GetPagedAsync(filter);
    public Task<MaintenanceRecord?> GetByIdAsync(int id) => _maintenanceRepository.GetByIdAsync(id);
    public Task<IReadOnlyList<MaintenanceRecord>> GetUpcomingAsync(int daysAhead = 30) => _maintenanceRepository.GetUpcomingAsync(daysAhead);
    public Task<IReadOnlyList<MaintenanceRecord>> GetByDeviceIdAsync(int deviceId) => _maintenanceRepository.GetByDeviceIdAsync(deviceId);
    public Task<decimal> GetTotalCostAsync() => _maintenanceRepository.GetTotalCostAsync();

    public async Task<int> CreateAsync(MaintenanceRecord record)
    {
        var id = await _maintenanceRepository.CreateAsync(record);

        if (record.MaintenanceType is "Corrective" or "Inspection")
        {
            var device = await _deviceRepository.GetByIdAsync(record.DeviceId);
            if (device != null && device.Status != DeviceStatus.Retired)
            {
                device.Status = DeviceStatus.InMaintenance;
                await _deviceRepository.UpdateAsync(device);
            }
        }

        await _auditService.LogAsync(AuditActionTypes.Create, "MaintenanceRecord", id, $"Logged maintenance for device ID {record.DeviceId}");
        return id;
    }

    public async Task UpdateAsync(MaintenanceRecord record)
    {
        await _maintenanceRepository.UpdateAsync(record);
        await _auditService.LogAsync(AuditActionTypes.Update, "MaintenanceRecord", record.Id, $"Updated maintenance record {record.Id}");
    }

    public async Task DeleteAsync(int id)
    {
        await _maintenanceRepository.DeleteAsync(id);
        await _auditService.LogAsync(AuditActionTypes.Delete, "MaintenanceRecord", id, $"Deleted maintenance record {id}");
    }
}

public class WarrantyService
{
    private readonly IWarrantyRepository _warrantyRepository;
    private readonly IAuditService _auditService;

    public WarrantyService(IWarrantyRepository warrantyRepository, IAuditService auditService)
    {
        _warrantyRepository = warrantyRepository;
        _auditService = auditService;
    }

    public Task<PagedResult<Warranty>> GetPagedAsync(SearchFilter filter) => _warrantyRepository.GetPagedAsync(filter);
    public Task<Warranty?> GetByIdAsync(int id) => _warrantyRepository.GetByIdAsync(id);
    public Task<WarrantySummaryCounts> GetSummaryCountsAsync() => _warrantyRepository.GetSummaryCountsAsync();
    public Task<IReadOnlyList<Warranty>> GetByDeviceIdAsync(int deviceId) => _warrantyRepository.GetByDeviceIdAsync(deviceId);

    public async Task<int> CreateAsync(Warranty warranty)
    {
        var id = await _warrantyRepository.CreateAsync(warranty);
        await _auditService.LogAsync(AuditActionTypes.Create, "Warranty", id, $"Created warranty for device ID {warranty.DeviceId}");
        return id;
    }

    public async Task UpdateAsync(Warranty warranty)
    {
        await _warrantyRepository.UpdateAsync(warranty);
        await _auditService.LogAsync(AuditActionTypes.Update, "Warranty", warranty.Id, $"Updated warranty {warranty.Id}");
    }

    public async Task DeleteAsync(int id)
    {
        await _warrantyRepository.DeleteAsync(id);
        await _auditService.LogAsync(AuditActionTypes.Delete, "Warranty", id, $"Deleted warranty {id}");
    }
}

public class ReportService
{
    private readonly IReportRepository _reportRepository;
    private readonly IDeviceRepository _deviceRepository;
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IAssignmentRepository _assignmentRepository;
    private readonly IMaintenanceRepository _maintenanceRepository;
    private readonly IWarrantyRepository _warrantyRepository;

    public ReportService(
        IReportRepository reportRepository,
        IDeviceRepository deviceRepository,
        IEmployeeRepository employeeRepository,
        IAssignmentRepository assignmentRepository,
        IMaintenanceRepository maintenanceRepository,
        IWarrantyRepository warrantyRepository)
    {
        _reportRepository = reportRepository;
        _deviceRepository = deviceRepository;
        _employeeRepository = employeeRepository;
        _assignmentRepository = assignmentRepository;
        _maintenanceRepository = maintenanceRepository;
        _warrantyRepository = warrantyRepository;
    }

    public Task<IReadOnlyList<InventoryReportRow>> GetInventoryAsync(ReportFilter filter) =>
        _reportRepository.GetInventoryReportAsync(filter);

    public Task<IReadOnlyList<AssignedDeviceReportRow>> GetAssignedDevicesAsync(ReportFilter filter) =>
        _reportRepository.GetAssignedDevicesReportAsync(filter);

    public Task<MaintenanceCostReportResult> GetMaintenanceCostAsync(ReportFilter filter) =>
        _reportRepository.GetMaintenanceCostReportAsync(filter);

    public Task<IReadOnlyList<WarrantyExpirationReportRow>> GetWarrantyExpirationAsync(ReportFilter filter) =>
        _reportRepository.GetWarrantyExpirationReportAsync(filter);

    public Task<IReadOnlyList<DepartmentDeviceReportRow>> GetDepartmentDevicesAsync(ReportFilter filter) =>
        _reportRepository.GetDepartmentDeviceReportAsync(filter);

    public async Task<ReportSummary> GetSummaryAsync()
    {
        var byType = await _deviceRepository.GetCountByTypeAsync();
        var byStatus = await _deviceRepository.GetCountByStatusAsync();
        var byDepartment = await _assignmentRepository.GetCountByDepartmentAsync();
        return new ReportSummary
        {
            TotalDevices = byStatus.Sum(x => x.Value),
            TotalEmployees = await _employeeRepository.GetActiveCountAsync(),
            ActiveAssignments = await _assignmentRepository.GetActiveCountAsync(),
            TotalMaintenanceCost = await _maintenanceRepository.GetTotalCostAsync(),
            DevicesByType = byType.ToList(),
            DevicesByStatus = byStatus.ToList(),
            DevicesByDepartment = byDepartment.ToList(),
            ExpiringWarranties = (await _warrantyRepository.GetExpiringAsync(90)).ToList()
        };
    }

    public Task<IReadOnlyList<string>> GetDepartmentsAsync() => _employeeRepository.GetDepartmentsAsync();
}

public class AuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IAuditService _auditService;

    public AuthService(IUserRepository userRepository, IPasswordHasher passwordHasher, IAuditService auditService)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _auditService = auditService;
    }

    public async Task<User?> ValidateUserAsync(string userName, string password)
    {
        var user = await _userRepository.GetByUserNameAsync(userName);
        if (user == null || !_passwordHasher.VerifyPassword(password, user.PasswordHash))
            return null;

        await _userRepository.UpdateLastLoginAsync(user.Id);
        await _auditService.LogAsync(AuditActionTypes.Login, "User", user.Id, $"User {userName} logged in");
        return user;
    }
}

public class AuditLogService
{
    private readonly IAuditLogRepository _auditLogRepository;

    public AuditLogService(IAuditLogRepository auditLogRepository) => _auditLogRepository = auditLogRepository;

    public Task<PagedResult<AuditLog>> GetPagedAsync(SearchFilter filter) => _auditLogRepository.GetPagedAsync(filter);

    public Task<IReadOnlyList<AuditLog>> GetAllForExportAsync(SearchFilter filter) =>
        _auditLogRepository.GetAllForExportAsync(filter);

    public Task<IReadOnlyList<AuditLog>> GetForDeviceAsync(int deviceId, int limit = 50) =>
        _auditLogRepository.GetForDeviceAsync(deviceId, limit);

    public Task<IReadOnlyList<AuditLog>> GetForEmployeeAsync(int employeeId, int limit = 50) =>
        _auditLogRepository.GetForEmployeeAsync(employeeId, limit);
}

public class UserService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IAuditService _auditService;

    public UserService(IUserRepository userRepository, IPasswordHasher passwordHasher, IAuditService auditService)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _auditService = auditService;
    }

    public Task<IReadOnlyList<User>> GetAllAsync() => _userRepository.GetAllAsync();

    public Task<User?> GetByIdAsync(int id) => _userRepository.GetByIdAsync(id);

    public async Task<(bool Success, string? Error, int Id)> CreateAsync(User user, string password)
    {
        if (await _userRepository.UserNameExistsAsync(user.UserName))
            return (false, "Username already exists.", 0);

        if (string.IsNullOrWhiteSpace(password) || password.Length < 6)
            return (false, "Password must be at least 6 characters.", 0);

        if (!UserRoles.All.Contains(user.Role))
            user.Role = UserRoles.Viewer;

        user.PasswordHash = _passwordHasher.HashPassword(password);
        user.IsActive = true;

        var id = await _userRepository.CreateAsync(user);
        await _auditService.LogAsync(AuditActionTypes.Create, "User", id, $"Created user {user.UserName} ({user.Role})");
        return (true, null, id);
    }

    public async Task<(bool Success, string? Error)> DeactivateAsync(int id, int currentUserId)
    {
        if (id == currentUserId)
            return (false, "You cannot deactivate your own account.");

        var user = await _userRepository.GetByIdAsync(id);
        if (user == null)
            return (false, "User not found.");

        if (!user.IsActive)
            return (false, "User is already inactive.");

        await _userRepository.DeactivateAsync(id);
        await _auditService.LogAsync(AuditActionTypes.Deactivate, "User", id, $"Deactivated user {user.UserName}");
        return (true, null);
    }
}
