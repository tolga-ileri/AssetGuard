using System.ComponentModel.DataAnnotations;

namespace AssetGuard.Core.Models;

public class Device
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Asset tag is required")]
    [StringLength(50)]
    [Display(Name = "Asset Tag")]
    public string AssetTag { get; set; } = string.Empty;

    [Required(ErrorMessage = "Serial number is required")]
    [StringLength(100)]
    [Display(Name = "Serial Number")]
    public string SerialNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "Device name is required")]
    [StringLength(200)]
    [Display(Name = "Device Name")]
    public string DeviceName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Device type is required")]
    [StringLength(50)]
    [Display(Name = "Device Type")]
    public string DeviceType { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string Brand { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string Model { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Date)]
    [Display(Name = "Purchase Date")]
    public DateTime PurchaseDate { get; set; }

    [DataType(DataType.Date)]
    [Display(Name = "Warranty End Date")]
    public DateTime? WarrantyEndDate { get; set; }

    [Required]
    [StringLength(50)]
    public string Status { get; set; } = "Available";

    [StringLength(200)]
    public string? Location { get; set; }

    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}
