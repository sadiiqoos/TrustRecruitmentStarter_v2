// JobsController.cs - komplett och korrekt
using Microsoft.AspNetCore.Mvc;
using TrustRecruitment.Business.DTOs;
using TrustRecruitment.Business.Interfaces;
using TrustRecruitment.Web.ViewModels;

namespace TrustRecruitment.Web.Controllers;

public class JobsController : Controller
{
    private readonly IJobService _jobService;
    private readonly IApplicationService _applicationService;

    public JobsController(IJobService jobService, IApplicationService applicationService)
    {
        _jobService = jobService;
        _applicationService = applicationService;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var jobs = await _jobService.GetActiveAsync(cancellationToken);
        return View(jobs);
    }

    public async Task<IActionResult> Details(Guid id, CancellationToken cancellationToken)
    {
        var job = await _jobService.GetByIdAsync(id, cancellationToken);
        if (job is null) return NotFound();
        return View(job);
    }

    [HttpGet]
    public async Task<IActionResult> Apply(Guid id, CancellationToken cancellationToken)
    {
        var job = await _jobService.GetByIdAsync(id, cancellationToken);
        if (job is null) return NotFound();

        return View(new ApplyViewModel
        {
            JobId = id,
            JobTitle = job.Title
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Apply(ApplyViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return View(model);

        await _applicationService.CreateAsync(new ApplicationDto
        {
            JobId = model.JobId,
            CandidateName = model.CandidateName,
            CandidateEmail = model.CandidateEmail,
            Country = model.Country,
            AppliedUtc = DateTime.UtcNow
        }, cancellationToken);

        return RedirectToAction(nameof(Confirmation));
    }

    public IActionResult Confirmation()
    {
        return View();
    }
}