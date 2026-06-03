using AssetGuard.Application.Services;
using AssetGuard.Core.Enums;
using AssetGuard.Web.Services;
using AssetGuard.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AssetGuard.Web.Controllers;

[Authorize]
public class ReportsController : Controller
{
    private readonly ReportService _reportService;

    public ReportsController(ReportService reportService) => _reportService = reportService;

    public IActionResult Index() => View();

    public async Task<IActionResult> Inventory(ReportFilterViewModel filter)
    {
        var rows = await _reportService.GetInventoryAsync(filter.ToFilter());
        return View(new InventoryReportViewModel
        {
            Filter = filter,
            Rows = rows,
            Departments = await _reportService.GetDepartmentsAsync()
        });
    }

    [HttpGet]
    public async Task<IActionResult> ExportInventory(ReportFilterViewModel filter)
    {
        var rows = await _reportService.GetInventoryAsync(filter.ToFilter());
        var bytes = ExcelReportExporter.ExportInventory(rows, filter.ToFilter());
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"AssetGuard_Inventory_{DateTime.Now:yyyyMMdd}.xlsx");
    }

    [HttpGet]
    public async Task<IActionResult> ExportInventoryPdf(ReportFilterViewModel filter)
    {
        var rows = await _reportService.GetInventoryAsync(filter.ToFilter());
        var bytes = PdfReportExporter.ExportInventory(rows, filter.ToFilter());
        return File(bytes, "application/pdf", $"AssetGuard_Inventory_{DateTime.Now:yyyyMMdd}.pdf");
    }

    public async Task<IActionResult> AssignedDevices(ReportFilterViewModel filter)
    {
        var rows = await _reportService.GetAssignedDevicesAsync(filter.ToFilter());
        return View(new AssignedDevicesReportViewModel
        {
            Filter = filter,
            Rows = rows,
            Departments = await _reportService.GetDepartmentsAsync()
        });
    }

    [HttpGet]
    public async Task<IActionResult> ExportAssignedDevices(ReportFilterViewModel filter)
    {
        var rows = await _reportService.GetAssignedDevicesAsync(filter.ToFilter());
        var bytes = ExcelReportExporter.ExportAssignedDevices(rows, filter.ToFilter());
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"AssetGuard_AssignedDevices_{DateTime.Now:yyyyMMdd}.xlsx");
    }

    [HttpGet]
    public async Task<IActionResult> ExportAssignedDevicesPdf(ReportFilterViewModel filter)
    {
        var rows = await _reportService.GetAssignedDevicesAsync(filter.ToFilter());
        var bytes = PdfReportExporter.ExportAssignedDevices(rows, filter.ToFilter());
        return File(bytes, "application/pdf", $"AssetGuard_AssignedDevices_{DateTime.Now:yyyyMMdd}.pdf");
    }

    public async Task<IActionResult> MaintenanceCost(ReportFilterViewModel filter)
    {
        var result = await _reportService.GetMaintenanceCostAsync(filter.ToFilter());
        return View(new MaintenanceCostReportViewModel { Filter = filter, Result = result });
    }

    [HttpGet]
    public async Task<IActionResult> ExportMaintenanceCost(ReportFilterViewModel filter)
    {
        var result = await _reportService.GetMaintenanceCostAsync(filter.ToFilter());
        var bytes = ExcelReportExporter.ExportMaintenanceCost(result, filter.ToFilter());
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"AssetGuard_MaintenanceCost_{DateTime.Now:yyyyMMdd}.xlsx");
    }

    [HttpGet]
    public async Task<IActionResult> ExportMaintenanceCostPdf(ReportFilterViewModel filter)
    {
        var result = await _reportService.GetMaintenanceCostAsync(filter.ToFilter());
        var bytes = PdfReportExporter.ExportMaintenanceCost(result, filter.ToFilter());
        return File(bytes, "application/pdf", $"AssetGuard_MaintenanceCost_{DateTime.Now:yyyyMMdd}.pdf");
    }

    public async Task<IActionResult> WarrantyExpiration(ReportFilterViewModel filter)
    {
        var rows = await _reportService.GetWarrantyExpirationAsync(filter.ToFilter());
        return View(new WarrantyExpirationReportViewModel { Filter = filter, Rows = rows });
    }

    [HttpGet]
    public async Task<IActionResult> ExportWarrantyExpiration(ReportFilterViewModel filter)
    {
        var rows = await _reportService.GetWarrantyExpirationAsync(filter.ToFilter());
        var bytes = ExcelReportExporter.ExportWarrantyExpiration(rows, filter.ToFilter());
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"AssetGuard_WarrantyExpiration_{DateTime.Now:yyyyMMdd}.xlsx");
    }

    [HttpGet]
    public async Task<IActionResult> ExportWarrantyExpirationPdf(ReportFilterViewModel filter)
    {
        var rows = await _reportService.GetWarrantyExpirationAsync(filter.ToFilter());
        var bytes = PdfReportExporter.ExportWarrantyExpiration(rows, filter.ToFilter());
        return File(bytes, "application/pdf", $"AssetGuard_WarrantyExpiration_{DateTime.Now:yyyyMMdd}.pdf");
    }

    public async Task<IActionResult> DepartmentDevices(ReportFilterViewModel filter)
    {
        var rows = await _reportService.GetDepartmentDevicesAsync(filter.ToFilter());
        return View(new DepartmentDeviceReportViewModel
        {
            Filter = filter,
            Rows = rows,
            Departments = await _reportService.GetDepartmentsAsync()
        });
    }

    [HttpGet]
    public async Task<IActionResult> ExportDepartmentDevices(ReportFilterViewModel filter)
    {
        var rows = await _reportService.GetDepartmentDevicesAsync(filter.ToFilter());
        var bytes = ExcelReportExporter.ExportDepartmentDevices(rows, filter.ToFilter());
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"AssetGuard_DepartmentDevices_{DateTime.Now:yyyyMMdd}.xlsx");
    }

    [HttpGet]
    public async Task<IActionResult> ExportDepartmentDevicesPdf(ReportFilterViewModel filter)
    {
        var rows = await _reportService.GetDepartmentDevicesAsync(filter.ToFilter());
        var bytes = PdfReportExporter.ExportDepartmentDevices(rows, filter.ToFilter());
        return File(bytes, "application/pdf", $"AssetGuard_DepartmentDevices_{DateTime.Now:yyyyMMdd}.pdf");
    }

    private void PopulateReportViewBag()
    {
        ViewBag.DeviceTypes = DeviceTypes.All;
        ViewBag.DeviceStatuses = DeviceStatus.All;
        ViewBag.WarrantyStatuses = new[] { "Active", "Expiring30", "Expired" };
    }
}
