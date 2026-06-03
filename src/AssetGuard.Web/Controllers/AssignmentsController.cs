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
public class AssignmentsController : Controller
{
    private readonly AssignmentService _assignmentService;
    private readonly DeviceService _deviceService;
    private readonly EmployeeService _employeeService;

    public AssignmentsController(
        AssignmentService assignmentService,
        DeviceService deviceService,
        EmployeeService employeeService)
    {
        _assignmentService = assignmentService;
        _deviceService = deviceService;
        _employeeService = employeeService;
    }

    public async Task<IActionResult> Index(ListFilterViewModel filter)
    {
        ViewBag.Filter = filter;
        ViewBag.Statuses = AssignmentStatus.All;
        var result = await _assignmentService.GetPagedAsync(FilterMapper.ToFilter(filter));
        return View(result);
    }

    public async Task<IActionResult> History(ListFilterViewModel filter)
    {
        filter.Status = null;
        ViewBag.Filter = filter;
        ViewBag.IsHistory = true;
        var result = await _assignmentService.GetPagedAsync(FilterMapper.ToFilter(filter));
        return View("Index", result);
    }

    public async Task<IActionResult> Details(int id)
    {
        var assignment = await _assignmentService.GetByIdAsync(id);
        if (assignment == null) return NotFound();
        return View(assignment);
    }

    public async Task<IActionResult> Create()
    {
        await PopulateDropdownsAsync();
        return View(new Assignment { AssignedDate = DateTime.Today, Status = AssignmentStatus.Active });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Assignment assignment)
    {
        if (!ModelState.IsValid)
        {
            await PopulateDropdownsAsync();
            return View(assignment);
        }

        var result = await _assignmentService.CreateAsync(assignment);
        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Error!);
            await PopulateDropdownsAsync();
            return View(assignment);
        }

        TempData["Success"] = "Asset assigned successfully.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Return(int id)
    {
        var assignment = await _assignmentService.GetByIdAsync(id);
        if (assignment == null) return NotFound();
        if (assignment.Status == AssignmentStatus.Returned) return RedirectToAction(nameof(Index));

        return View(new ReturnAssignmentViewModel { Id = id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Return(ReturnAssignmentViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var result = await _assignmentService.ReturnAsync(model.Id, model.ReturnDate, model.ReturnNote);
        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Error!);
            return View(model);
        }

        TempData["Success"] = "Asset returned successfully.";
        return RedirectToAction(nameof(Index));
    }

    private async Task PopulateDropdownsAsync()
    {
        var devices = await _deviceService.GetAvailableAsync();
        var employees = await _employeeService.GetActiveAsync();

        ViewBag.Devices = new SelectList(devices, "Id", "DeviceName");
        ViewBag.Employees = new SelectList(employees, "Id", "FullName");
    }
}
