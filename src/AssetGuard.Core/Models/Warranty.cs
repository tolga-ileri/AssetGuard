using System.ComponentModel.DataAnnotations;

namespace AssetGuard.Core.Models;

public class Warranty
{
    public int Id { get; set; }

    [Required]
    [Display(Name = "Device")]
    public int DeviceId { get; set; }

    [Required]
    [StringLength(200)]
    public string Provider { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Date)]
    [Display(Name = "Start Date")]
    public DateTime StartDate { get; set; }

    [Required]
    [DataType(DataType.Date)]
    [Display(Name = "End Date")]
    public DateTime EndDate { get; set; }

    [Display(Name = "Coverage Details")]
    public string? CoverageDetails { get; set; }

    [Display(Name = "Active")]
    public bool IsActive { get; set; } = true;

    // Joined display fields
    public string? DeviceName { get; set; }
    public string? AssetTag { get; set; }
    public string? DeviceType { get; set; }
}
