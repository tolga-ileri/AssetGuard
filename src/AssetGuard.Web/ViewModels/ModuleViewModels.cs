using AssetGuard.Core.Common;
using AssetGuard.Core.DTOs;
using AssetGuard.Core.Models;

namespace AssetGuard.Web.ViewModels;

public class DeviceDetailViewModel
{
    public Device Device { get; set; } = new();
    public IReadOnlyList<Assignment> AssignmentHistory { get; set; } = [];
    public IReadOnlyList<MaintenanceRecord> MaintenanceHistory { get; set; } = [];
    public IReadOnlyList<Warranty> Warranties { get; set; } = [];
    public IReadOnlyList<ActivityTimelineItem> Timeline { get; set; } = [];
    public IReadOnlyList<AuditLog> AuditLogs { get; set; } = [];
    public bool HasActiveAssignment { get; set; }
}

public class EmployeeDetailViewModel
{
    public Employee Employee { get; set; } = new();
    public IReadOnlyList<Assignment> AssignmentHistory { get; set; } = [];
    public IReadOnlyList<ActivityTimelineItem> Timeline { get; set; } = [];
}

public class WarrantyIndexViewModel
{
    public PagedResult<Warranty> Warranties { get; set; } = new();
    public WarrantySummaryCounts Summary { get; set; } = new();
    public ListFilterViewModel Filter { get; set; } = new();
}

public class MaintenanceIndexViewModel
{
    public PagedResult<MaintenanceRecord> Records { get; set; } = new();
    public IReadOnlyList<MaintenanceRecord> UpcomingMaintenance { get; set; } = [];
    public ListFilterViewModel Filter { get; set; } = new();
}
