using AssetGuard.Application.Services;
using AssetGuard.Core.Models;
using AssetGuard.Web.Helpers;
using AssetGuard.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AssetGuard.Web.Controllers;

[Authorize]
public class WarrantiesController : Controller
{
    private readonly WarrantyService _warrantyService;
    private readonly DeviceService _deviceService;

    public WarrantiesController(WarrantyService warrantyService, DeviceService deviceService)
    {
        _warrantyService = warrantyService;
        _deviceService = deviceService;
    }

    public async Task<IActionResult> Index(ListFilterViewModel filter)
    {
        var summary = await _warrantyService.GetSummaryCountsAsync();
        var warranties = await _warrantyService.GetPagedAsync(FilterMapper.ToFilter(filter));

        return View(new WarrantyIndexViewModel
        {
            Warranties = warranties,
            Summary = summary,
            Filter = filter
        });
    }

    public async Task<IActionResult> Details(int id)
    {
        var warranty = await _warrantyService.GetByIdAsync(id);
        if (warranty == null) return NotFound();
        return View(warranty);
    }

    public async Task<IActionResult> Create()
    {
        await PopulateDevicesAsync();
        return View(new Warranty { IsActive = true, StartDate = DateTime.Today, EndDate = DateTime.Today.AddYears(1) });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Warranty warranty)
    {
        if (!ModelState.IsValid)
        {
            await PopulateDevicesAsync();
            return View(warranty);
        }

        if (warranty.EndDate < warranty.StartDate)
        {
            ModelState.AddModelError(nameof(Warranty.EndDate), "End date must be on or after start date.");
            await PopulateDevicesAsync();
            return View(warranty);
        }

        await _warrantyService.CreateAsync(warranty);
        TempData["Success"] = "Warranty created successfully.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var warranty = await _warrantyService.GetByIdAsync(id);
        if (warranty == null) return NotFound();
        await PopulateDevicesAsync();
        return View("Create", warranty);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Warranty warranty)
    {
        if (id != warranty.Id) return NotFound();
        if (!ModelState.IsValid)
        {
            await PopulateDevicesAsync();
            return View("Create", warranty);
        }

        if (warranty.EndDate < warranty.StartDate)
        {
            ModelState.AddModelError(nameof(Warranty.EndDate), "End date must be on or after start date.");
            await PopulateDevicesAsync();
            return View("Create", warranty);
        }

        await _warrantyService.UpdateAsync(warranty);
        TempData["Success"] = "Warranty updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int id)
    {
        var warranty = await _warrantyService.GetByIdAsync(id);
        if (warranty == null) return NotFound();
        return View(warranty);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        await _warrantyService.DeleteAsync(id);
        TempData["Success"] = "Warranty deleted successfully.";
        return RedirectToAction(nameof(Index));
    }

    private async Task PopulateDevicesAsync()
    {
        var filter = new AssetGuard.Core.Common.SearchFilter { Page = 1, PageSize = 1000 };
        var devices = await _deviceService.GetPagedAsync(filter);
        ViewBag.Devices = new SelectList(devices.Items, "Id", "DeviceName");
    }
}
