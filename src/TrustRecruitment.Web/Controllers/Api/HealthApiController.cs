using Microsoft.AspNetCore.Mvc;

namespace TrustRecruitment.Web.Controllers.Api;

[ApiController]
[Route("api/health")]
public class HealthApiController : ControllerBase
{
    [HttpGet]
    public IActionResult Get() => Ok(new
    {
        status = "Healthy",
        utcNow = DateTime.UtcNow
    });
}
