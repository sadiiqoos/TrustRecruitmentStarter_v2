using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using TrustRecruitment.Business.Interfaces;
using TrustRecruitment.Business.Services;
using TrustRecruitment.Data.Repositories.Interfaces;
using TrustRecruitment.Data.Repositories.MongoDb;
using TrustRecruitment.Data.Settings;
using TrustRecruitment.Data.Storage.Interfaces;
using TrustRecruitment.Data.Storage.Local;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

// MongoDB settings
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

// Cookie authentication
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

// Repositories
builder.Services.AddScoped<IUserRepository, MongoUserRepository>();
builder.Services.AddScoped<IJobRepository, MongoJobRepository>();
builder.Services.AddScoped<IApplicationRepository, MongoApplicationRepository>();

builder.Services.AddSingleton<IFileStorageRepository>(_ =>
{
    var root = Path.Combine(builder.Environment.ContentRootPath, "App_Data", "cv-uploads");
    return new LocalFileStorageRepository(root);
});

// Services
builder.Services.AddScoped<IJobService, JobService>();
builder.Services.AddScoped<IApplicationService, ApplicationService>();
builder.Services.AddScoped<IAccountService, AccountService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapControllers();

// Seed admin-användare vid uppstart
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