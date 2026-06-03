namespace AssetGuard.Core.DTOs;

public class ReportFilter
{
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public string? Department { get; set; }
    public string? DeviceType { get; set; }
    public string? Status { get; set; }
}

public class InventoryReportRow
{
    public string AssetTag { get; set; } = string.Empty;
    public string SerialNumber { get; set; } = string.Empty;
    public string DeviceName { get; set; } = string.Empty;
    public string DeviceType { get; set; } = string.Empty;
    public string Brand { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? Location { get; set; }
    public DateTime PurchaseDate { get; set; }
    public DateTime? WarrantyEndDate { get; set; }
}

public class AssignedDeviceReportRow
{
    public string AssetTag { get; set; } = string.Empty;
    public string DeviceName { get; set; } = string.Empty;
    public string DeviceType { get; set; } = string.Empty;
    public string EmployeeName { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public DateTime AssignedDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? AssignmentNote { get; set; }
}

public class MaintenanceCostReportRow
{
    public DateTime MaintenanceDate { get; set; }
    public string AssetTag { get; set; } = string.Empty;
    public string DeviceName { get; set; } = string.Empty;
    public string DeviceType { get; set; } = string.Empty;
    public string MaintenanceType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Cost { get; set; }
    public string PerformedBy { get; set; } = string.Empty;
}

public class MaintenanceCostReportResult
{
    public IReadOnlyList<MaintenanceCostReportRow> Rows { get; set; } = [];
    public decimal TotalCost { get; set; }
    public int RecordCount { get; set; }
}

public class WarrantyExpirationReportRow
{
    public string AssetTag { get; set; } = string.Empty;
    public string DeviceName { get; set; } = string.Empty;
    public string DeviceType { get; set; } = string.Empty;
    public string Provider { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int DaysRemaining { get; set; }
    public string WarrantyStatus { get; set; } = string.Empty;
}

public class DepartmentDeviceReportRow
{
    public string Department { get; set; } = string.Empty;
    public string EmployeeName { get; set; } = string.Empty;
    public string AssetTag { get; set; } = string.Empty;
    public string DeviceName { get; set; } = string.Empty;
    public string DeviceType { get; set; } = string.Empty;
    public DateTime AssignedDate { get; set; }
    public string DeviceStatus { get; set; } = string.Empty;
}
