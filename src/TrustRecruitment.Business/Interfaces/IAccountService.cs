using TrustRecruitment.Business.Models;

namespace TrustRecruitment.Business.Interfaces;

public interface IAccountService
{
    Task<AccountResult> SignupAsync(string fullName, string email, string password);
    Task<AccountResult> LoginAsync(string email, string password, bool rememberMe = true);
    Task LogoutAsync();
}