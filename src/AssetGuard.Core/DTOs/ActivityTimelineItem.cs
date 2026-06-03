namespace AssetGuard.Core.DTOs;

public class ActivityTimelineItem
{
    public DateTime OccurredAt { get; set; }
    public string Category { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Icon { get; set; } = "bi-circle";
    public string ColorClass { get; set; } = "primary";
    public string? Actor { get; set; }
}

public class MonthlyCostPoint
{
    public string Month { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}

public class MonthlyTrendPoint
{
    public string Month { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class DashboardAnalytics
{
    public IReadOnlyList<MonthlyCostPoint> MonthlyMaintenanceCosts { get; set; } = [];
    public IReadOnlyList<ChartDataPoint> DevicesByDepartment { get; set; } = [];
    public IReadOnlyList<MonthlyTrendPoint> AssignmentTrends { get; set; } = [];
    public IReadOnlyList<MonthlyTrendPoint> WarrantyExpirationTrends { get; set; } = [];
}
