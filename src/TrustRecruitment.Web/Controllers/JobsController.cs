using Microsoft.AspNetCore.Mvc;
using TrustRecruitment.Business.Interfaces;

namespace TrustRecruitment.Web.Controllers;

public class JobsController : Controller
{
    private readonly IJobService _jobService;

    public JobsController(IJobService jobService)
    {
        _jobService = jobService;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var jobs = await _jobService.GetActiveAsync(cancellationToken);
        return View(jobs);
    }

    public async Task<IActionResult> Details(Guid id, CancellationToken cancellationToken)
    {
        var job = await _jobService.GetByIdAsync(id, cancellationToken);
        if (job is null)
        {
            return NotFound();
        }

        return View(job);
    }
}
