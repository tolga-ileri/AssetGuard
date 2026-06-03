using AssetGuard.Application.Services;
using AssetGuard.Core.Models;
using AssetGuard.Web.Helpers;
using AssetGuard.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AssetGuard.Web.Controllers;

[Authorize]
public class EmployeesController : Controller
{
    private readonly EmployeeService _employeeService;
    private readonly TimelineService _timelineService;
    private readonly AuditLogService _auditLogService;

    public EmployeesController(EmployeeService employeeService, TimelineService timelineService, AuditLogService auditLogService)
    {
        _employeeService = employeeService;
        _timelineService = timelineService;
        _auditLogService = auditLogService;
    }

    public async Task<IActionResult> Index(ListFilterViewModel filter)
    {
        ViewBag.Filter = filter;
        ViewBag.Departments = await _employeeService.GetDepartmentsAsync();
        var result = await _employeeService.GetPagedAsync(FilterMapper.ToFilter(filter));
        return View(result);
    }

    public async Task<IActionResult> Details(int id)
    {
        var employee = await _employeeService.GetByIdAsync(id);
        if (employee == null) return NotFound();

        var assignments = await _employeeService.GetAssignmentHistoryAsync(id);
        var auditLogs = await _auditLogService.GetForEmployeeAsync(id);

        return View(new EmployeeDetailViewModel
        {
            Employee = employee,
            AssignmentHistory = assignments,
            Timeline = _timelineService.BuildEmployeeTimeline(employee, assignments, auditLogs)
        });
    }

    public IActionResult Create() => View(new Employee { IsActive = true });

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Employee employee)
    {
        if (!ModelState.IsValid) return View(employee);
        await _employeeService.CreateAsync(employee);
        TempData["Success"] = "Employee created successfully.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var employee = await _employeeService.GetByIdAsync(id);
        if (employee == null) return NotFound();
        return View("Create", employee);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Employee employee)
    {
        if (id != employee.Id) return NotFound();
        if (!ModelState.IsValid) return View("Create", employee);
        await _employeeService.UpdateAsync(employee);
        TempData["Success"] = "Employee updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Deactivate(int id)
    {
        var employee = await _employeeService.GetByIdAsync(id);
        if (employee == null) return NotFound();
        if (!employee.IsActive) return RedirectToAction(nameof(Details), new { id });
        return View(employee);
    }

    [HttpPost, ActionName("Deactivate")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeactivateConfirmed(int id)
    {
        await _employeeService.DeactivateAsync(id);
        TempData["Success"] = "Employee deactivated successfully.";
        return RedirectToAction(nameof(Index));
    }
}
