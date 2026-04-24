using Microsoft.AspNetCore.Mvc;
using System.Reflection;

namespace TrustRecruitment.Web.Controllers.Api;

[ApiController]
[Route("api/version")]
public class VersionApiController : ControllerBase
{
    [HttpGet]
    public IActionResult Get() => Ok(new
    {
        application = "TrustRecruitment",
        version = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "1.0.0",
        environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"
    });
}
