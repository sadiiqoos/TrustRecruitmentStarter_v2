// JobEditorViewModel.cs
using System.ComponentModel.DataAnnotations;

namespace TrustRecruitment.Web.ViewModels;

public class JobEditorViewModel
{
    public Guid Id { get; set; }

    [Required]
    public string Title { get; set; } = string.Empty;

    [Required]
    public string Department { get; set; } = string.Empty;

    [Required]
    public string Location { get; set; } = string.Empty;

    [Required]
    public string Country { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Employment type")]
    public string EmploymentType { get; set; } = string.Empty;

    [Required]
    public string Description { get; set; } = string.Empty;

    [Display(Name = "Active job")]
    public bool IsActive { get; set; }
}