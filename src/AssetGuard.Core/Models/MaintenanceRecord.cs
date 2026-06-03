using System.ComponentModel.DataAnnotations;

namespace AssetGuard.Core.Models;

public class MaintenanceRecord
{
    public int Id { get; set; }

    [Required]
    [Display(Name = "Device")]
    public int DeviceId { get; set; }

    [Required]
    [DataType(DataType.Date)]
    [Display(Name = "Maintenance Date")]
    public DateTime MaintenanceDate { get; set; }

    [Required]
    [StringLength(100)]
    [Display(Name = "Maintenance Type")]
    public string MaintenanceType { get; set; } = string.Empty;

    [Required]
    public string Description { get; set; } = string.Empty;

    [Range(0, 999999.99)]
    [DataType(DataType.Currency)]
    public decimal Cost { get; set; }

    [Required]
    [StringLength(200)]
    [Display(Name = "Performed By")]
    public string PerformedBy { get; set; } = string.Empty;

    [DataType(DataType.Date)]
    [Display(Name = "Next Maintenance Date")]
    public DateTime? NextMaintenanceDate { get; set; }

    // Joined display fields
    public string? DeviceName { get; set; }
    public string? AssetTag { get; set; }
}
