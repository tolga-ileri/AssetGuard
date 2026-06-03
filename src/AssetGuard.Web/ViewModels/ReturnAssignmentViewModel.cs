using System.ComponentModel.DataAnnotations;

namespace AssetGuard.Web.ViewModels;

public class ReturnAssignmentViewModel
{
    public int Id { get; set; }

    [Required]
    [DataType(DataType.Date)]
    [Display(Name = "Return Date")]
    public DateTime ReturnDate { get; set; } = DateTime.Today;

    [Display(Name = "Return Note")]
    [StringLength(500)]
    public string? ReturnNote { get; set; }
}
