using AssetGuard.Application.Services;
using AssetGuard.Core.Enums;
using AssetGuard.Core.Models;
using AssetGuard.Web.Helpers;
using AssetGuard.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QRCoder;

namespace AssetGuard.Web.Controllers;

[Authorize]
public class DevicesController : Controller
{
    private readonly DeviceService _deviceService;
    private readonly TimelineService _timelineService;
    private readonly AuditLogService _auditLogService;

    public DevicesController(DeviceService deviceService, TimelineService timelineService, AuditLogService auditLogService)
    {
        _deviceService = deviceService;
        _timelineService = timelineService;
        _auditLogService = auditLogService;
    }

    public async Task<IActionResult> Index(ListFilterViewModel filter)
    {
        ViewBag.DeviceTypes = DeviceTypes.All;
        ViewBag.Statuses = DeviceStatus.All;
        ViewBag.Filter = filter;
        var result = await _deviceService.GetPagedAsync(FilterMapper.ToFilter(filter));
        return View(result);
    }

    public async Task<IActionResult> Details(int id)
    {
        var device = await _deviceService.GetByIdAsync(id);
        if (device == null) return NotFound();

        var assignments = await _deviceService.GetAssignmentHistoryAsync(id);
        var maintenance = await _deviceService.GetMaintenanceHistoryAsync(id);
        var warranties = await _deviceService.GetWarrantiesAsync(id);
        var auditLogs = await _auditLogService.GetForDeviceAsync(id);

        var model = new DeviceDetailViewModel
        {
            Device = device,
            AssignmentHistory = assignments,
            MaintenanceHistory = maintenance,
            Warranties = warranties,
            AuditLogs = auditLogs,
            HasActiveAssignment = assignments.Any(a => a.Status == AssignmentStatus.Active),
            Timeline = _timelineService.BuildDeviceTimeline(device, assignments, maintenance, warranties, auditLogs)
        };
        return View(model);
    }

    [HttpGet]
    public IActionResult QrCode(int id)
    {
        var url = Url.Action("Details", "Devices", new { id }, Request.Scheme)!;
        using var generator = new QRCodeGenerator();
        var data = generator.CreateQrCode(url, QRCodeGenerator.ECCLevel.Q);
        var png = new PngByteQRCode(data);
        return File(png.GetGraphic(8), "image/png");
    }

    public IActionResult Create() => View(new Device { Status = DeviceStatus.Available, PurchaseDate = DateTime.Today });

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Device device)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.DeviceTypes = DeviceTypes.All;
            ViewBag.Statuses = DeviceStatus.All;
            return View(device);
        }

        var result = await _deviceService.CreateAsync(device);
        if (!result.Success)
        {
            ModelState.AddModelError(nameof(Device.AssetTag), result.Error!);
            ViewBag.DeviceTypes = DeviceTypes.All;
            ViewBag.Statuses = DeviceStatus.All;
            return View(device);
        }

        TempData["Success"] = "Device created successfully.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var device = await _deviceService.GetByIdAsync(id);
        if (device == null) return NotFound();
        ViewBag.DeviceTypes = DeviceTypes.All;
        ViewBag.Statuses = DeviceStatus.All;
        return View("Create", device);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Device device)
    {
        if (id != device.Id) return NotFound();
        if (!ModelState.IsValid)
        {
            ViewBag.DeviceTypes = DeviceTypes.All;
            ViewBag.Statuses = DeviceStatus.All;
            return View("Create", device);
        }

        var result = await _deviceService.UpdateAsync(device);
        if (!result.Success)
        {
            ModelState.AddModelError(nameof(Device.AssetTag), result.Error!);
            ViewBag.DeviceTypes = DeviceTypes.All;
            ViewBag.Statuses = DeviceStatus.All;
            return View("Create", device);
        }

        TempData["Success"] = "Device updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int id)
    {
        var device = await _deviceService.GetByIdAsync(id);
        if (device == null) return NotFound();
        return View(device);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var result = await _deviceService.DeleteAsync(id);
        if (!result.Success)
        {
            TempData["Error"] = result.Error;
            return RedirectToAction(nameof(Details), new { id });
        }
        TempData["Success"] = "Device deleted successfully.";
        return RedirectToAction(nameof(Index));
    }
}
