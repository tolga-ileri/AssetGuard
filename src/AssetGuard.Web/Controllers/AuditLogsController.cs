using AssetGuard.Application.Services;
using AssetGuard.Core.Enums;
using AssetGuard.Web.Helpers;
using AssetGuard.Web.Services;
using AssetGuard.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AssetGuard.Web.Controllers;

[Authorize]
public class AuditLogsController : Controller
{
    private readonly AuditLogService _auditLogService;

    public AuditLogsController(AuditLogService auditLogService) => _auditLogService = auditLogService;

    public async Task<IActionResult> Index(AuditLogFilterViewModel filter)
    {
        ViewBag.Filter = filter;
        ViewBag.ActionTypes = new[]
        {
            AuditActionTypes.Create, AuditActionTypes.Update, AuditActionTypes.Delete,
            AuditActionTypes.Assign, AuditActionTypes.Return, AuditActionTypes.Deactivate,
            AuditActionTypes.Login, AuditActionTypes.Logout
        };
        ViewBag.EntityNames = new[]
        {
            "Device", "Employee", "Assignment", "MaintenanceRecord", "Warranty", "User"
        };

        var logs = await _auditLogService.GetPagedAsync(FilterMapper.ToFilter(filter));
        return View(new AuditLogIndexViewModel { Filter = filter, Logs = logs });
    }

    [HttpGet]
    public async Task<IActionResult> Export(AuditLogFilterViewModel filter)
    {
        filter.Page = 1;
        filter.PageSize = 10000;
        var logs = await _auditLogService.GetAllForExportAsync(FilterMapper.ToFilter(filter));
        var bytes = ExcelReportExporter.ExportAuditLogs(logs);
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"AssetGuard_AuditLogs_{DateTime.Now:yyyyMMdd}.xlsx");
    }
}
