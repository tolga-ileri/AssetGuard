using AssetGuard.Core.Common;
using AssetGuard.Core.DTOs;

namespace AssetGuard.Web.ViewModels;

public class ReportFilterViewModel
{
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public string? Department { get; set; }
    public string? DeviceType { get; set; }
    public string? Status { get; set; }

    public ReportFilter ToFilter() => new()
    {
        DateFrom = DateFrom,
        DateTo = DateTo,
        Department = Department,
        DeviceType = DeviceType,
        Status = Status
    };
}

public class AuditLogFilterViewModel
{
    public string? SearchTerm { get; set; }
    public string? UserName { get; set; }
    public string? ActionType { get; set; }
    public string? EntityName { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class InventoryReportViewModel
{
    public ReportFilterViewModel Filter { get; set; } = new();
    public IReadOnlyList<InventoryReportRow> Rows { get; set; } = [];
    public IReadOnlyList<string> Departments { get; set; } = [];
}

public class AssignedDevicesReportViewModel
{
    public ReportFilterViewModel Filter { get; set; } = new();
    public IReadOnlyList<AssignedDeviceReportRow> Rows { get; set; } = [];
    public IReadOnlyList<string> Departments { get; set; } = [];
}

public class MaintenanceCostReportViewModel
{
    public ReportFilterViewModel Filter { get; set; } = new();
    public MaintenanceCostReportResult Result { get; set; } = new();
}

public class WarrantyExpirationReportViewModel
{
    public ReportFilterViewModel Filter { get; set; } = new();
    public IReadOnlyList<WarrantyExpirationReportRow> Rows { get; set; } = [];
}

public class DepartmentDeviceReportViewModel
{
    public ReportFilterViewModel Filter { get; set; } = new();
    public IReadOnlyList<DepartmentDeviceReportRow> Rows { get; set; } = [];
    public IReadOnlyList<string> Departments { get; set; } = [];
}

public class AuditLogIndexViewModel
{
    public AuditLogFilterViewModel Filter { get; set; } = new();
    public PagedResult<AssetGuard.Core.Models.AuditLog> Logs { get; set; } = new();
}
