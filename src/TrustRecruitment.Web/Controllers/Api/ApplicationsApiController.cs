using Microsoft.AspNetCore.Mvc;
using TrustRecruitment.Business.DTOs;
using TrustRecruitment.Business.Interfaces;

namespace TrustRecruitment.Web.Controllers.Api;

[ApiController]
[Route("api/applications")]
public class ApplicationsApiController : ControllerBase
{
    private readonly IApplicationService _applicationService;

    public ApplicationsApiController(IApplicationService applicationService)
    {
        _applicationService = applicationService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ApplicationDto>>> Get(CancellationToken cancellationToken)
        => Ok(await _applicationService.GetAllAsync(cancellationToken));
}
