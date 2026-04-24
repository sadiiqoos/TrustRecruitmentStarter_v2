using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using TrustRecruitment.Business.Interfaces;
using TrustRecruitment.Data.Repositories.Interfaces;
using TrustRecruitment.Web.Models.Account;

namespace TrustRecruitment.Web.Controllers;

public class AccountController : Controller
{
    private readonly IAccountService _accountService;
    private readonly IUserRepository _userRepository;

    public AccountController(
        IAccountService accountService,
        IUserRepository userRepository)
    {
        _accountService = accountService;
        _userRepository = userRepository;
    }

    [HttpGet]
    public IActionResult Login()
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Home");

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var normalizedEmail = NormalizeEmail(model.Email);

        Console.WriteLine($"[Login] Attempt for: {normalizedEmail}");

        var result = await _accountService.LoginAsync(
            normalizedEmail,
            model.Password
        );

        Console.WriteLine($"[Login] AccountService result: {result.Succeeded}");

        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error);

            return View(model);
        }

        var user = await _userRepository.GetByEmailAsync(normalizedEmail);

        Console.WriteLine(user == null
            ? "[Login] User not found after successful password verification."
            : $"[Login] User found: {user.Email}");

        if (user == null)
        {
            ModelState.AddModelError(string.Empty, "Kunde inte hitta användaren.");
            return View(model);
        }

        Console.WriteLine("[Login] Creating auth cookie...");

        await SignInUserAsync(
            user.Id.ToString(),
            user.FullName,
            user.Email,
            user.Role,
            isPersistent: true);

        Console.WriteLine("[Login] Cookie created. Redirecting to Home/Index.");

        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult Signup()
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Home");

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Signup(SignupViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var normalizedEmail = NormalizeEmail(model.Email);

        Console.WriteLine($"[Signup] Attempt for: {normalizedEmail}");

        var result = await _accountService.SignupAsync(
            model.FullName.Trim(),
            normalizedEmail,
            model.Password);

        Console.WriteLine($"[Signup] AccountService result: {result.Succeeded}");

        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error);

            return View(model);
        }

        var user = await _userRepository.GetByEmailAsync(normalizedEmail);

        Console.WriteLine(user == null
            ? "[Signup] Created user not found afterwards."
            : $"[Signup] Created user found: {user.Email}");

        if (user == null)
        {
            ModelState.AddModelError(string.Empty, "Kunde inte hitta den skapade användaren.");
            return View(model);
        }

        Console.WriteLine("[Signup] Creating auth cookie...");

        await SignInUserAsync(
            user.Id.ToString(),
            user.FullName,
            user.Email,
            user.Role,
            isPersistent: true);

        Console.WriteLine("[Signup] Cookie created. Redirecting to Home/Index.");

        return RedirectToAction("Index", "Home");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        Console.WriteLine("[Logout] Signing out user.");
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Home");
    }

    private async Task SignInUserAsync(
        string userId,
        string fullName,
        string email,
        string role,
        bool isPersistent)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Name, fullName),
            new Claim(ClaimTypes.Email, email),
            new Claim(ClaimTypes.Role, role)
        };

        var identity = new ClaimsIdentity(
            claims,
            CookieAuthenticationDefaults.AuthenticationScheme);

        var principal = new ClaimsPrincipal(identity);

        var authProperties = new AuthenticationProperties
        {
            IsPersistent = isPersistent
        };

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            authProperties);
    }

    private static string NormalizeEmail(string email)
    {
        return (email ?? string.Empty).Trim().ToLowerInvariant();
    }
}