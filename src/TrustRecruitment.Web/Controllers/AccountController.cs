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
    private readonly ILogger<AccountController> _logger;

    public AccountController(
        IAccountService accountService,
        IUserRepository userRepository,
        ILogger<AccountController> logger)
    {
        _accountService = accountService;
        _userRepository = userRepository;
        _logger = logger;
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

        // Loggar INTE lösenordet — bara e-postadressen
        _logger.LogInformation("Login attempt. Email={Email}", normalizedEmail);

        var result = await _accountService.LoginAsync(normalizedEmail, model.Password);

        if (!result.Succeeded)
        {
            _logger.LogWarning("Login failed. Email={Email}, Errors={Errors}",
                normalizedEmail, string.Join(", ", result.Errors));

            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error);

            return View(model);
        }

        var user = await _userRepository.GetByEmailAsync(normalizedEmail);

        if (user == null)
        {
            _logger.LogError("User not found after successful password verification. Email={Email}", normalizedEmail);
            ModelState.AddModelError(string.Empty, "Kunde inte hitta användaren.");
            return View(model);
        }

        await SignInUserAsync(user.Id.ToString(), user.FullName, user.Email, user.Role, isPersistent: true);

        _logger.LogInformation("Login successful. UserId={UserId}, Role={Role}", user.Id, user.Role);

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

        _logger.LogInformation("Signup attempt. Email={Email}", normalizedEmail);

        var result = await _accountService.SignupAsync(model.FullName.Trim(), normalizedEmail, model.Password);

        if (!result.Succeeded)
        {
            _logger.LogWarning("Signup failed. Email={Email}, Errors={Errors}",
                normalizedEmail, string.Join(", ", result.Errors));

            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error);

            return View(model);
        }

        var user = await _userRepository.GetByEmailAsync(normalizedEmail);

        if (user == null)
        {
            _logger.LogError("Created user not found afterwards. Email={Email}", normalizedEmail);
            ModelState.AddModelError(string.Empty, "Kunde inte hitta den skapade användaren.");
            return View(model);
        }

        await SignInUserAsync(user.Id.ToString(), user.FullName, user.Email, user.Role, isPersistent: true);

        _logger.LogInformation("Signup successful. UserId={UserId}", user.Id);

        return RedirectToAction("Index", "Home");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        _logger.LogInformation("User logged out. UserId={UserId}", userId);

        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Home");
    }

    private async Task SignInUserAsync(string userId, string fullName, string email, string role, bool isPersistent)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Name, fullName),
            new Claim(ClaimTypes.Email, email),
            new Claim(ClaimTypes.Role, role)
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);
        var authProperties = new AuthenticationProperties { IsPersistent = isPersistent };

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, authProperties);
    }

    private static string NormalizeEmail(string email)
        => (email ?? string.Empty).Trim().ToLowerInvariant();
}
