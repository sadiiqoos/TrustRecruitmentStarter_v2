// ViewModels/ChangePasswordViewModel.cs
using System.ComponentModel.DataAnnotations;

namespace TrustRecruitment.Web.ViewModels;

public class ChangePasswordViewModel
{
    public Guid UserId { get; set; }
    public string UserEmail { get; set; } = string.Empty;

    [Required]
    [MinLength(8, ErrorMessage = "Lösenordet måste vara minst 8 tecken.")]
    [Display(Name = "Nytt lösenord")]
    public string NewPassword { get; set; } = string.Empty;

    [Required]
    [Compare(nameof(NewPassword), ErrorMessage = "Lösenorden matchar inte.")]
    [Display(Name = "Bekräfta lösenord")]
    public string ConfirmPassword { get; set; } = string.Empty;
}