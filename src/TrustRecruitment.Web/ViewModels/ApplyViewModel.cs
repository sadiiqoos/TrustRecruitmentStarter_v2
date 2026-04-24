using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace TrustRecruitment.Web.ViewModels;

public class ApplyViewModel
{
    public Guid JobId { get; set; }

    public string JobTitle { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Full name")]
    public string CandidateName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string CandidateEmail { get; set; } = string.Empty;

    [Required]
    public string Country { get; set; } = string.Empty;

    [Display(Name = "CV file")]
    public IFormFile? CvFile { get; set; }
}
