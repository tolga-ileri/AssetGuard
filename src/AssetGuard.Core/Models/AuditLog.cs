using System.ComponentModel.DataAnnotations;

namespace AssetGuard.Core.Models;

public class AuditLog
{
    public long Id { get; set; }

    [Required]
    [StringLength(100)]
    [Display(Name = "User")]
    public string UserName { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    [Display(Name = "Action")]
    public string ActionType { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    [Display(Name = "Entity")]
    public string EntityName { get; set; } = string.Empty;

    [Display(Name = "Entity ID")]
    public int? EntityId { get; set; }

    [Required]
    [StringLength(500)]
    public string Description { get; set; } = string.Empty;

    [Display(Name = "Timestamp")]
    public DateTime CreatedAt { get; set; }
}
