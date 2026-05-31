using Microsoft.AspNetCore.Mvc;
using TrustRecruitment.Business.DTOs;
using TrustRecruitment.Business.Interfaces;

namespace TrustRecruitment.Web.Controllers.Api;

/// <summary>
/// REST API för jobbannonser. Kräver X-Api-Key header (hanteras av ApiKeyMiddleware).
/// </summary>
[ApiController]
[Route("api/jobs")]
[Produces("application/json")]
public class JobsApiController : ControllerBase
{
    private readonly IJobService _jobService;
    private readonly ILogger<JobsApiController> _logger;

    public JobsApiController(IJobService jobService, ILogger<JobsApiController> logger)
    {
        _jobService = jobService;
        _logger = logger;
    }

    /// <summary>Hämtar alla aktiva jobbannonser.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<JobDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<JobDto>>> Get(CancellationToken cancellationToken)
    {
        _logger.LogInformation("API: GetActiveJobs called");
        return Ok(await _jobService.GetActiveAsync(cancellationToken));
    }

    /// <summary>Hämtar en specifik jobbannons med angivet ID.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(JobDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<JobDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        _logger.LogInformation("API: GetJobById called. JobId={JobId}", id);
        var job = await _jobService.GetByIdAsync(id, cancellationToken);
        return job is null ? NotFound() : Ok(job);
    }

    /// <summary>Skapar en ny jobbannons.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(JobDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] JobDto dto, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        dto.Id = Guid.NewGuid();
        await _jobService.CreateAsync(dto, cancellationToken);

        _logger.LogInformation("API: Job created. JobId={JobId}", dto.Id);

        return CreatedAtAction(nameof(GetById), new { id = dto.Id }, dto);
    }
}
