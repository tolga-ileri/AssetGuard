using System.ComponentModel.DataAnnotations;
using AssetGuard.Core.Models;

namespace AssetGuard.Web.ViewModels;

public class CreateUserViewModel
{
    [Required]
    [StringLength(100)]
    [Display(Name = "Username")]
    public string UserName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(200)]
    [Display(Name = "Full Name")]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string Role { get; set; } = AssetGuard.Core.Enums.UserRoles.Viewer;

    [Required]
    [StringLength(100, MinimumLength = 6)]
    [DataType(DataType.Password)]
    [Display(Name = "Password")]
    public string Password { get; set; } = string.Empty;

    [Required]
    [Compare(nameof(Password))]
    [DataType(DataType.Password)]
    [Display(Name = "Confirm Password")]
    public string ConfirmPassword { get; set; } = string.Empty;
}

public class UserIndexViewModel
{
    public IReadOnlyList<User> Users { get; set; } = [];
}
