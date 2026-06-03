using System.ComponentModel.DataAnnotations;

namespace AssetGuard.Core.Models;

public class Assignment
{
    public int Id { get; set; }

    [Required]
    [Display(Name = "Device")]
    public int DeviceId { get; set; }

    [Required]
    [Display(Name = "Employee")]
    public int EmployeeId { get; set; }

    [Required]
    [DataType(DataType.Date)]
    [Display(Name = "Assigned Date")]
    public DateTime AssignedDate { get; set; }

    [DataType(DataType.Date)]
    [Display(Name = "Return Date")]
    public DateTime? ReturnDate { get; set; }

    [Required]
    [StringLength(50)]
    public string Status { get; set; } = "Active";

    [StringLength(500)]
    [Display(Name = "Assignment Note")]
    public string? AssignmentNote { get; set; }

    [StringLength(500)]
    [Display(Name = "Return Note")]
    public string? ReturnNote { get; set; }

    // Joined display fields (not persisted)
    public string? DeviceName { get; set; }
    public string? AssetTag { get; set; }
    public string? EmployeeName { get; set; }
    public string? Department { get; set; }
}
