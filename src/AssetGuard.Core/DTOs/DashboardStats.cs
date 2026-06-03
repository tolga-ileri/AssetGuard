namespace AssetGuard.Core.DTOs;

public class DashboardStats
{
    public int TotalDevices { get; set; }
    public int AvailableDevices { get; set; }
    public int AssignedDevices { get; set; }
    public int InMaintenanceDevices { get; set; }
    public int TotalEmployees { get; set; }
    public int ActiveAssignments { get; set; }
    public int ExpiringWarranties { get; set; }
    public int ExpiredWarranties { get; set; }
    public int UpcomingMaintenance { get; set; }
    public decimal TotalMaintenanceCost { get; set; }
}

public class ChartDataPoint
{
    public string Label { get; set; } = string.Empty;
    public int Value { get; set; }
}

public class WarrantySummaryCounts
{
    public int Active { get; set; }
    public int Expired { get; set; }
    public int Expiring30 { get; set; }
}

public class ReportSummary
{
    public int TotalDevices { get; set; }
    public int TotalEmployees { get; set; }
    public int ActiveAssignments { get; set; }
    public decimal TotalMaintenanceCost { get; set; }
    public List<ChartDataPoint> DevicesByType { get; set; } = [];
    public List<ChartDataPoint> DevicesByStatus { get; set; } = [];
    public List<ChartDataPoint> DevicesByDepartment { get; set; } = [];
    public List<WarrantyReportItem> ExpiringWarranties { get; set; } = [];
}

public class WarrantyReportItem
{
    public string AssetTag { get; set; } = string.Empty;
    public string DeviceName { get; set; } = string.Empty;
    public string Provider { get; set; } = string.Empty;
    public DateTime EndDate { get; set; }
    public int DaysRemaining { get; set; }
}
