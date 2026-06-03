using AssetGuard.Application.Services;
using AssetGuard.Core.Models;
using AssetGuard.Core.Enums;
using AssetGuard.Web.Helpers;
using AssetGuard.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AssetGuard.Web.Controllers;

[Authorize]
public class MaintenanceController : Controller
{
    private readonly MaintenanceService _maintenanceService;
    private readonly DeviceService _deviceService;

    public MaintenanceController(MaintenanceService maintenanceService, DeviceService deviceService)
    {
        _maintenanceService = maintenanceService;
        _deviceService = deviceService;
    }

    public async Task<IActionResult> Index(ListFilterViewModel filter)
    {
        var records = await _maintenanceService.GetPagedAsync(FilterMapper.ToFilter(filter));
        var upcoming = await _maintenanceService.GetUpcomingAsync(30);

        return View(new MaintenanceIndexViewModel
        {
            Records = records,
            UpcomingMaintenance = upcoming,
            Filter = filter
        });
    }

    public async Task<IActionResult> Details(int id)
    {
        var record = await _maintenanceService.GetByIdAsync(id);
        if (record == null) return NotFound();
        return View(record);
    }

    public async Task<IActionResult> Create()
    {
        await PopulateDevicesAsync();
        ViewBag.MaintenanceTypes = MaintenanceTypes.All;
        return View(new MaintenanceRecord { MaintenanceDate = DateTime.Today });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(MaintenanceRecord record)
    {
        if (!ModelState.IsValid)
        {
            await PopulateDevicesAsync();
            ViewBag.MaintenanceTypes = MaintenanceTypes.All;
            return View(record);
        }

        await _maintenanceService.CreateAsync(record);
        TempData["Success"] = "Maintenance record created successfully.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var record = await _maintenanceService.GetByIdAsync(id);
        if (record == null) return NotFound();
        await PopulateDevicesAsync();
        ViewBag.MaintenanceTypes = MaintenanceTypes.All;
        return View("Create", record);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, MaintenanceRecord record)
    {
        if (id != record.Id) return NotFound();
        if (!ModelState.IsValid)
        {
            await PopulateDevicesAsync();
            ViewBag.MaintenanceTypes = MaintenanceTypes.All;
            return View("Create", record);
        }

        await _maintenanceService.UpdateAsync(record);
        TempData["Success"] = "Maintenance record updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int id)
    {
        var record = await _maintenanceService.GetByIdAsync(id);
        if (record == null) return NotFound();
        return View(record);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        await _maintenanceService.DeleteAsync(id);
        TempData["Success"] = "Maintenance record deleted successfully.";
        return RedirectToAction(nameof(Index));
    }

    private async Task PopulateDevicesAsync()
    {
        var filter = new AssetGuard.Core.Common.SearchFilter { Page = 1, PageSize = 1000 };
        var devices = await _deviceService.GetPagedAsync(filter);
        ViewBag.Devices = new SelectList(devices.Items, "Id", "DeviceName");
    }
}
