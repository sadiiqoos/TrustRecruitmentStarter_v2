using Microsoft.AspNetCore.Mvc;

namespace TrustRecruitment.Web.Controllers.Api;

/// <summary>
/// Enkel ping-endpoint. Den djupa health proben finns på /healthz och kontrollerar
/// MongoDB och Blob Storage med AspNetCore.HealthChecks.
/// </summary>
[ApiController]
[Route("api/health")]
public class HealthApiController : ControllerBase
{
    /// <summary>Returnerar applikationens version och aktuell tid (ytlig kontroll).</summary>
    [HttpGet]
    public IActionResult Get() => Ok(new
    {
        status = "Healthy",
        utcNow = DateTime.UtcNow,
        note = "For deep dependency checks, see /healthz"
    });
}
