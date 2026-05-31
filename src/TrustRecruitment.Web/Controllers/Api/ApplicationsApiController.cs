using Microsoft.AspNetCore.Mvc;
using TrustRecruitment.Business.DTOs;
using TrustRecruitment.Business.Interfaces;

namespace TrustRecruitment.Web.Controllers.Api;

/// <summary>
/// REST API för ansökningar. Kräver X-Api-Key header (hanteras av ApiKeyMiddleware).
/// </summary>
[ApiController]
[Route("api/applications")]
[Produces("application/json")]
public class ApplicationsApiController : ControllerBase
{
    private readonly IApplicationService _applicationService;
    private readonly ILogger<ApplicationsApiController> _logger;

    public ApplicationsApiController(IApplicationService applicationService, ILogger<ApplicationsApiController> logger)
    {
        _applicationService = applicationService;
        _logger = logger;
    }

    /// <summary>Hämtar alla ansökningar (admin-ändamål).</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ApplicationDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ApplicationDto>>> Get(CancellationToken cancellationToken)
    {
        _logger.LogInformation("API: GetAllApplications called");
        return Ok(await _applicationService.GetAllAsync(cancellationToken));
    }

    /// <summary>Hämtar ansökningar för ett specifikt jobb.</summary>
    [HttpGet("by-job/{jobId:guid}")]
    [ProducesResponseType(typeof(IReadOnlyList<ApplicationDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ApplicationDto>>> GetByJob(Guid jobId, CancellationToken cancellationToken)
    {
        _logger.LogInformation("API: GetApplicationsByJob called. JobId={JobId}", jobId);
        return Ok(await _applicationService.GetByJobIdAsync(jobId, cancellationToken));
    }
}
