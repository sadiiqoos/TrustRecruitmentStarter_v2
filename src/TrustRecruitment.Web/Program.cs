using Azure.Identity;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using MongoDB.Driver;
using TrustRecruitment.Business.Interfaces;
using TrustRecruitment.Business.Services;
using TrustRecruitment.Data.Repositories.Interfaces;
using TrustRecruitment.Data.Repositories.MongoDb;
using TrustRecruitment.Data.Settings;
using TrustRecruitment.Data.Storage.Blob;
using TrustRecruitment.Data.Storage.Interfaces;
using TrustRecruitment.Data.Storage.Local;
using TrustRecruitment.Web.HealthChecks;
using TrustRecruitment.Web.Middleware;
using HealthChecks.UI.Client;

var builder = WebApplication.CreateBuilder(args);

// ── MVC + API ─────────────────────────────────────────────────────
builder.Services.AddControllersWithViews();
builder.Services.AddEndpointsApiExplorer();

// ── Swagger / OpenAPI ─────────────────────────────────────────────
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "TrustRecruitment API",
        Version = "v1",
        Description = "REST API för TrustRecruitment – exponerar jobb och ansökningar."
    });

    c.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Name = "X-Api-Key",
        Type = SecuritySchemeType.ApiKey,
        Description = "Ange din API-nyckel. Format: <din nyckel>"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "ApiKey"
                }
            },
            Array.Empty<string>()
        }
    });
});

// ── Application Insights ──────────────────────────────────────────
builder.Services.AddApplicationInsightsTelemetry();

// ── MongoDB ───────────────────────────────────────────────────────
builder.Services.Configure<MongoDbSettings>(
    builder.Configuration.GetSection("MongoDbSettings"));

builder.Services.AddSingleton<IMongoClient>(_ =>
{
    var connectionString = builder.Configuration.GetConnectionString("MongoDb");
    return new MongoClient(connectionString);
});

builder.Services.AddSingleton(sp =>
{
    var settings = sp.GetRequiredService<IOptions<MongoDbSettings>>().Value;
    var client = sp.GetRequiredService<IMongoClient>();
    return client.GetDatabase(settings.DatabaseName);
});

// ── Cookie-autentisering ──────────────────────────────────────────
builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/Login";
        options.Cookie.Name = "TrustRecruitment.Auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.Strict;
        options.SlidingExpiration = true;
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
    });

builder.Services.AddAuthorization();

// ── Repositories ──────────────────────────────────────────────────
builder.Services.AddScoped<IUserRepository, MongoUserRepository>();
builder.Services.AddScoped<IJobRepository, MongoJobRepository>();
builder.Services.AddScoped<IApplicationRepository, MongoApplicationRepository>();

// ── Fillagring: Blob i produktion, lokalt i utveckling ───────────
var blobAccountName = builder.Configuration["BlobStorage:AccountName"];

if (!string.IsNullOrEmpty(blobAccountName))
{
    builder.Services.AddSingleton(_ =>
        new BlobServiceClient(
            new Uri($"https://{blobAccountName}.blob.core.windows.net"),
            new DefaultAzureCredential()));

    builder.Services.AddSingleton<IFileStorageRepository, BlobFileStorageRepository>();
}
else
{
    builder.Services.AddSingleton<IFileStorageRepository>(_ =>
    {
        var root = Path.Combine(builder.Environment.ContentRootPath, "App_Data", "cv-uploads");
        return new LocalFileStorageRepository(root);
    });
}

// ── Services ──────────────────────────────────────────────────────
builder.Services.AddScoped<IJobService, JobService>();
builder.Services.AddScoped<IApplicationService, ApplicationService>();
builder.Services.AddScoped<IAccountService, AccountService>();

// ── Health Checks — egna implementationer, inga externa paket ────
// MongoDbHealthCheck använder IMongoDatabase som redan är registrerad ovan
builder.Services.AddSingleton<MongoDbHealthCheck>();

var healthBuilder = builder.Services
    .AddHealthChecks()
    .AddCheck<MongoDbHealthCheck>(
        "mongodb",
        tags: new[] { "db", "ready" });

if (!string.IsNullOrEmpty(blobAccountName))
{
    builder.Services.AddSingleton<BlobStorageHealthCheck>();
    healthBuilder.AddCheck<BlobStorageHealthCheck>(
        "blob-storage",
        tags: new[] { "storage", "ready" });
}

// ── App pipeline ──────────────────────────────────────────────────
var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "TrustRecruitment API v1");
    c.RoutePrefix = "swagger";
});

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// API-nyckel middleware — körs för /api-routes (utom /api/health och /healthz)
app.UseMiddleware<ApiKeyMiddleware>();

// Djup health probe — rapporterar status för MongoDB och Blob Storage
app.MapHealthChecks("/healthz", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapControllers();

// ── Seed admin-användare vid uppstart ─────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var accountService = scope.ServiceProvider.GetRequiredService<IAccountService>();
    var adminEmail = builder.Configuration["AdminSeed:Email"] ?? "admin@trustrecruitment.com";
    var adminPassword = builder.Configuration["AdminSeed:Password"] ?? "Admin123!";

    try
    {
        await accountService.CreateAdminIfNotExistsAsync(adminEmail, adminPassword);
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Failed to seed admin user");
    }
}

app.Run();
