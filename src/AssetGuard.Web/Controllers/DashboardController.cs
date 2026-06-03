using AssetGuard.Application.Services;
using AssetGuard.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AssetGuard.Web.Controllers;

[Authorize]
public class DashboardController : Controller
{
    private readonly DashboardService _dashboardService;

    public DashboardController(DashboardService dashboardService) => _dashboardService = dashboardService;

    public async Task<IActionResult> Index()
    {
        var stats = await _dashboardService.GetStatsAsync();
        var charts = await _dashboardService.GetChartDataAsync();
        var analytics = await _dashboardService.GetAnalyticsAsync();
        var expiring = await _dashboardService.GetExpiringWarrantiesAsync();
        var upcoming = await _dashboardService.GetUpcomingMaintenanceAsync();

        return View(new DashboardViewModel
        {
            Stats = stats,
            Analytics = analytics,
            DevicesByType = charts.ByType,
            DevicesByStatus = charts.ByStatus,
            ExpiringWarranties = expiring,
            UpcomingMaintenance = upcoming
        });
    }
}
