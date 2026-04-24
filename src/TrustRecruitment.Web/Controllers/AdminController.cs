using Microsoft.AspNetCore.Mvc;
using TrustRecruitment.Business.DTOs;
using TrustRecruitment.Business.Interfaces;
using TrustRecruitment.Web.ViewModels;

namespace TrustRecruitment.Web.Controllers;

public class AdminController : Controller
{
    private readonly IJobService _jobService;
    private readonly IApplicationService _applicationService;

    public AdminController(IJobService jobService, IApplicationService applicationService)
    {
        _jobService = jobService;
        _applicationService = applicationService;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var jobs = await _jobService.GetAllAsync(cancellationToken);
        var applications = await _applicationService.GetAllAsync(cancellationToken);

        return View(new AdminDashboardViewModel
        {
            Jobs = jobs,
            Applications = applications
        });
    }

    [HttpGet]
    public IActionResult CreateJob()
    {
        return View(new JobEditorViewModel { IsActive = true });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateJob(JobEditorViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

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
}
