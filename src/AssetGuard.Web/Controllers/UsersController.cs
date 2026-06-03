using AssetGuard.Application.Services;
using AssetGuard.Core.Enums;
using AssetGuard.Core.Models;
using AssetGuard.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AssetGuard.Web.Controllers;

[Authorize(Roles = UserRoles.Admin)]
public class UsersController : Controller
{
    private readonly UserService _userService;

    public UsersController(UserService userService) => _userService = userService;

    public async Task<IActionResult> Index()
    {
        var users = await _userService.GetAllAsync();
        return View(new UserIndexViewModel { Users = users });
    }

    public IActionResult Create() => View(new CreateUserViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateUserViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var user = new User
        {
            UserName = model.UserName.Trim(),
            Email = model.Email.Trim(),
            FullName = model.FullName.Trim(),
            Role = model.Role
        };

        var result = await _userService.CreateAsync(user, model.Password);
        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Error!);
            return View(model);
        }

        TempData["Success"] = $"User '{user.UserName}' created successfully.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Deactivate(int id)
    {
        var user = await _userService.GetByIdAsync(id);
        if (user == null) return NotFound();
        return View(user);
    }

    [HttpPost, ActionName("Deactivate")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeactivateConfirmed(int id)
    {
        var currentUserId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
        var result = await _userService.DeactivateAsync(id, currentUserId);
        if (!result.Success)
        {
            TempData["Error"] = result.Error;
            return RedirectToAction(nameof(Deactivate), new { id });
        }

        TempData["Success"] = "User deactivated successfully.";
        return RedirectToAction(nameof(Index));
    }
}
