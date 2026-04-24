using Microsoft.AspNetCore.Identity;
using TrustRecruitment.Business.Interfaces;
using TrustRecruitment.Business.Models;
using TrustRecruitment.Data.Entities;
using TrustRecruitment.Data.Repositories.Interfaces;

namespace TrustRecruitment.Business.Services;

public class AccountService : IAccountService
{
    private readonly IUserRepository _userRepository;
    private readonly PasswordHasher<ApplicationUser> _passwordHasher = new();

    public AccountService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<AccountResult> SignupAsync(string fullName, string email, string password)
    {
        fullName = fullName?.Trim() ?? string.Empty;
        email = NormalizeEmail(email);

        if (string.IsNullOrWhiteSpace(fullName))
        {
            return Failed("Fullständigt namn måste anges.");
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            return Failed("E-post måste anges.");
        }

        if (string.IsNullOrWhiteSpace(password))
        {
            return Failed("Lösenord måste anges.");
        }

        if (password.Length < 8)
        {
            return Failed("Lösenordet måste vara minst 8 tecken långt.");
        }

        var existingUser = await _userRepository.GetByEmailAsync(email);

        if (existingUser != null)
        {
            return Failed("E-postadressen används redan.");
        }

        var user = new ApplicationUser
        {
            FullName = fullName,
            Email = email,
            Role = "User"
        };

        user.PasswordHash = _passwordHasher.HashPassword(user, password);

        await _userRepository.AddAsync(user);

        return Succeeded();
    }

    public async Task<AccountResult> LoginAsync(string email, string password, bool rememberMe = true)
    {
        email = NormalizeEmail(email);

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            return Failed("Fel e-post eller lösenord.");
        }

        var user = await _userRepository.GetByEmailAsync(email);

        if (user == null)
        {
            return Failed("Fel e-post eller lösenord.");
        }

        var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);

        if (result == PasswordVerificationResult.Failed)
        {
            return Failed("Fel e-post eller lösenord.");
        }

        return Succeeded();
    }

    public Task LogoutAsync()
    {
        return Task.CompletedTask;
    }

    private static string NormalizeEmail(string email)
    {
        return (email ?? string.Empty).Trim().ToLowerInvariant();
    }

    private static AccountResult Succeeded()
    {
        return new AccountResult
        {
            Succeeded = true
        };
    }

    private static AccountResult Failed(params string[] errors)
    {
        return new AccountResult
        {
            Succeeded = false,
            Errors = errors.ToList()
        };
    }
}