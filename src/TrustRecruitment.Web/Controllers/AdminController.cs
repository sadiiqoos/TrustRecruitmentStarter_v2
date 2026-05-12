// AdminController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TrustRecruitment.Business.DTOs;
using TrustRecruitment.Business.Interfaces;
using TrustRecruitment.Data.Repositories.Interfaces;
using TrustRecruitment.Web.ViewModels;

namespace TrustRecruitment.Web.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly IJobService _jobService;
    private readonly IApplicationService _applicationService;
    private readonly IUserRepository _userRepository;
    private readonly IAccountService _accountService;

    public AdminController(
        IJobService jobService,
        IApplicationService applicationService,
        IUserRepository userRepository,
        IAccountService accountService)
    {
        _jobService = jobService;
        _applicationService = applicationService;
        _userRepository = userRepository;
        _accountService = accountService;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var jobs = await _jobService.GetAllAsync(cancellationToken);
        var applications = await _applicationService.GetAllAsync(cancellationToken);
        var users = await _userRepository.GetAllAsync(cancellationToken);

        return View(new AdminDashboardViewModel
        {
            Jobs = jobs,
            Applications = applications,
            Users = users.Select(u => new UserDto
            {
                Id = u.Id,
                FullName = u.FullName,
                Email = u.Email,
                Role = u.Role
            }).ToList()
        });
    }

    // ── JOBB ──────────────────────────────────────────────────────

    [HttpGet]
    public IActionResult CreateJob()
    {
        return View(new JobEditorViewModel { IsActive = true });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateJob(JobEditorViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return View(model);

        await _jobService.CreateAsync(new JobDto
        {
            Title = model.Title,
            Department = model.Department,
            Location = model.Location,
            Country = model.Country,
            EmploymentType = model.EmploymentType,
            Description = model.Description,
            IsActive = model.IsActive
        }, cancellationToken);

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> EditJob(Guid id, CancellationToken cancellationToken)
    {
        var job = await _jobService.GetByIdAsync(id, cancellationToken);
        if (job is null) return NotFound();

        return View(new JobEditorViewModel
        {
            Id = id,
            Title = job.Title,
            Department = job.Department,
            Location = job.Location,
            Country = job.Country,
            EmploymentType = job.EmploymentType,
            Description = job.Description,
            IsActive = job.IsActive
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditJob(Guid id, JobEditorViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return View(model);

        await _jobService.UpdateAsync(id, new JobDto
        {
            Title = model.Title,
            Department = model.Department,
            Location = model.Location,
            Country = model.Country,
            EmploymentType = model.EmploymentType,
            Description = model.Description,
            IsActive = model.IsActive
        }, cancellationToken);

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteJob(Guid id, CancellationToken cancellationToken)
    {
        await _jobService.DeleteAsync(id, cancellationToken);
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> ViewApplications(Guid jobId, CancellationToken cancellationToken)
    {
        var applications = await _applicationService.GetByJobIdAsync(jobId, cancellationToken);
        return View(applications);
    }

    // ── ANVÄNDARE ─────────────────────────────────────────────────

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteUser(Guid id, CancellationToken cancellationToken)
    {
        var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (currentUserId == id.ToString())
        {
            TempData["Error"] = "Du kan inte ta bort ditt eget konto.";
            return RedirectToAction(nameof(Index));
        }

        await _userRepository.DeleteAsync(id, cancellationToken);
        TempData["Success"] = "Användaren har tagits bort.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> ChangePassword(Guid id, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(id, cancellationToken);
        if (user is null) return NotFound();

        return View(new ChangePasswordViewModel
        {
            UserId = id,
            UserEmail = user.Email
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return View(model);

        var result = await _accountService.ChangePasswordAsync(model.UserId, model.NewPassword);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error);
            return View(model);
        }

        TempData["Success"] = "Lösenordet har ändrats.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangeRole(Guid id, string role, CancellationToken cancellationToken)
    {
        var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (currentUserId == id.ToString())
        {
            TempData["Error"] = "Du kan inte ändra din egen roll.";
            return RedirectToAction(nameof(Index));
        }

        var user = await _userRepository.GetByIdAsync(id, cancellationToken);
        if (user is null) return NotFound();

        user.Role = role;
        await _userRepository.UpdateAsync(user, cancellationToken);

        TempData["Success"] = $"Rollen för {user.Email} har ändrats till {role}.";
        return RedirectToAction(nameof(Index));
    }
}