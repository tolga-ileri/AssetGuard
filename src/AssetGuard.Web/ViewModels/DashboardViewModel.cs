using AssetGuard.Core.DTOs;

namespace AssetGuard.Web.ViewModels;

public class DashboardViewModel
{
    public DashboardStats Stats { get; set; } = new();
    public DashboardAnalytics Analytics { get; set; } = new();
    public IReadOnlyList<ChartDataPoint> DevicesByType { get; set; } = [];
    public IReadOnlyList<ChartDataPoint> DevicesByStatus { get; set; } = [];
    public IReadOnlyList<WarrantyReportItem> ExpiringWarranties { get; set; } = [];
    public IReadOnlyList<AssetGuard.Core.Models.MaintenanceRecord> UpcomingMaintenance { get; set; } = [];
}
