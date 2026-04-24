using System.ComponentModel.DataAnnotations;

namespace TrustRecruitment.Web.Models.Account;

public class LoginViewModel
{
    [Required(ErrorMessage = "E-post måste anges.")]
    [EmailAddress(ErrorMessage = "Ogiltig e-postadress.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Lösenord måste anges.")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    public bool RememberMe { get; set; }
}