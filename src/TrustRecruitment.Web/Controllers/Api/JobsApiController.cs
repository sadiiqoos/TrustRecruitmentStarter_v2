using Microsoft.AspNetCore.Mvc;
using TrustRecruitment.Business.DTOs;
using TrustRecruitment.Business.Interfaces;

namespace TrustRecruitment.Web.Controllers.Api;

[ApiController]
[Route("api/jobs")]
public class JobsApiController : ControllerBase
{
    private readonly IJobService _jobService;

    public JobsApiController(IJobService jobService)
    {
        _jobService = jobService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<JobDto>>> Get(CancellationToken cancellationToken)
        => Ok(await _jobService.GetActiveAsync(cancellationToken));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<JobDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var job = await _jobService.GetByIdAsync(id, cancellationToken);
        return job is null ? NotFound() : Ok(job);
    }

    [HttpPost]
    public async Task<IActionResult> Create(JobDto dto, CancellationToken cancellationToken)
    {
        await _jobService.CreateAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = dto.Id }, dto);
    }
}
