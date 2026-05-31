namespace TrustRecruitment.Web.Middleware;

/// <summary>
/// Middleware som skyddar /api-routes med en API-nyckel via X-Api-Key header.
/// Nyckeln hämtas från konfigurationen (miljövariabel ApiKey) och ska aldrig lagras i versionshanteringen.
/// </summary>
public class ApiKeyMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ApiKeyMiddleware> _logger;
    private const string ApiKeyHeaderName = "X-Api-Key";

    public ApiKeyMiddleware(RequestDelegate next, ILogger<ApiKeyMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, IConfiguration configuration)
    {
        // Skydda bara /api-routes, inte /swagger eller MVC-vyer
        if (!context.Request.Path.StartsWithSegments("/api"))
        {
            await _next(context);
            return;
        }

        // /api/health är publikt — readiness probe ska inte kräva nyckel
        if (context.Request.Path.StartsWithSegments("/api/health"))
        {
            await _next(context);
            return;
        }

        var expectedKey = configuration["ApiKey"];

        if (string.IsNullOrEmpty(expectedKey))
        {
            _logger.LogError("ApiKey is not configured. All API requests will be rejected.");
            context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
            await context.Response.WriteAsync("API key not configured on server.");
            return;
        }

        if (!context.Request.Headers.TryGetValue(ApiKeyHeaderName, out var providedKey)
            || providedKey != expectedKey)
        {
            _logger.LogWarning("Unauthorized API access attempt. Path={Path}, IP={IP}",
                context.Request.Path,
                context.Connection.RemoteIpAddress);

            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync("Unauthorized: provide a valid X-Api-Key header.");
            return;
        }

        await _next(context);
    }
}
