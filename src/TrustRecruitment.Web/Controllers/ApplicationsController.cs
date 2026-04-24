using Microsoft.AspNetCore.Mvc;
using TrustRecruitment.Business.DTOs;
using TrustRecruitment.Business.Interfaces;
using TrustRecruitment.Web.ViewModels;

namespace TrustRecruitment.Web.Controllers;

public class ApplicationsController : Controller
{
    private readonly IApplicationService _applicationService;
    private readonly IJobService _jobService;

    public ApplicationsController(IApplicationService applicationService, IJobService jobService)
    {
        _applicationService = applicationService;
        _jobService = jobService;
    }

    [HttpGet]
    public async Task<IActionResult> Apply(Guid jobId, CancellationToken cancellationToken)
    {
        var job = await _jobService.GetByIdAsync(jobId, cancellationToken);
        if (job is null)
        {
            return NotFound();
        }

        return View(new ApplyViewModel
        {
            JobId = job.Id,
            JobTitle = job.Title
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Apply(ApplyViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        if (model.CvFile is null || model.CvFile.Length == 0)
        {
            ModelState.AddModelError(nameof(model.CvFile), "Please upload a CV file.");
            return View(model);
        }

        try
        {
            await using var stream = model.CvFile.OpenReadStream();
            await _applicationService.CreateAsync(new CreateApplicationDto
            {
                JobId = model.JobId,
                CandidateUserId = model.CandidateEmail.ToLowerInvariant(),
                CandidateName = model.CandidateName,
                CandidateEmail = model.CandidateEmail,
                Country = model.Country,
                CvFileName = model.CvFile.FileName,
                CvContent = stream
            }, cancellationToken);

            TempData["Message"] = "Application submitted successfully.";
            return RedirectToAction(nameof(MyApplications), new { email = model.CandidateEmail });
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }
    }

    [HttpGet]
    public async Task<IActionResult> MyApplications(string email, CancellationToken cancellationToken)
    {
        var items = await _applicationService.GetByCandidateAsync(email.ToLowerInvariant(), cancellationToken);
        ViewBag.Email = email;
        return View(items);
    }
}
