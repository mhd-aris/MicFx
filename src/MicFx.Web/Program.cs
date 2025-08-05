using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.FileProviders;
using Microsoft.AspNetCore.Mvc.Razor.RuntimeCompilation;
using MicFx.Infrastructure.Swagger;
using MicFx.Infrastructure.Logging;
using MicFx.Infrastructure.Extensions;
using MicFx.Abstractions.Extensions;
using MicFx.Core.Extensions;
using MicFx.Web.Infrastructure;
using MicFx.Web.Admin.Extensions;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog logging
builder.Services.AddMicFxSerilog(builder.Configuration, builder.Environment);

// Use Serilog as primary logging provider
builder.Host.UseSerilog();

// Add Razor + Runtime Compilation
builder.Services.AddControllersWithViews()
    .AddRazorRuntimeCompilation();

// 📁 Configure module view resolution
var contentRoot = builder.Environment.ContentRootPath;
var moduleRoot = Path.Combine(contentRoot, "..", "Modules"); // Go up one level from src/MicFx.Web to src/Modules

if (Directory.Exists(moduleRoot))
{
    // Add each module directory as a separate file provider
    var moduleDirectories = Directory.GetDirectories(moduleRoot, "MicFx.Modules.*");
    
    builder.Services.Configure<MvcRazorRuntimeCompilationOptions>(options =>
    {
        // Add the parent modules directory
        options.FileProviders.Add(new PhysicalFileProvider(moduleRoot));
        
        // Add each individual module directory for better resolution
        foreach (var moduleDir in moduleDirectories)
        {
            options.FileProviders.Add(new PhysicalFileProvider(moduleDir));
        }
    });
}

builder.Services.Configure<RazorViewEngineOptions>(options =>
{
    options.ViewLocationExpanders.Add(new MicFxViewLocationExpander());
});


// Add MicFx Configuration Management
builder.Services.AddMicFxConfigurationManagement(builder.Configuration, options =>
{
    options.ValidateOnStartup = true;
    options.ThrowOnValidationFailure = builder.Environment.IsDevelopment();
});

// Register MicFx Abstractions (interfaces available to modules)
builder.Services.AddMicFxAbstractions();

// Register MicFx Infrastructure implementations
builder.Services.AddMicFxInfrastructure();

// Load modules with dependency management
builder.Services.AddMicFxModules();

// Configure Authorization Policies for Admin Area
builder.Services.AddAuthorization(options =>
{
    // Admin area access policy - matches the one in Auth module
    options.AddPolicy("AdminAreaAccess", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireRole("SuperAdmin", "Admin");
    });
    
    // Additional policies can be added here
    options.AddPolicy("SuperAdminOnly", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireRole("SuperAdmin");
    });
});

// 🔧 Add Admin Navigation System
builder.Services.AddAdminNavigation();

// 🛡️ Add MicFx Exception Handling
builder.Services.AddMicFxExceptionHandling();

// 🩺 Add Module Health Checks
builder.Services.AddMicFxModuleHealthChecks();

// 🔧 Add MicFx Swagger Infrastructure 
builder.Services.AddMicFxSwaggerInfrastructure();

var app = builder.Build();

// Use Serilog for request logging
app.UseMicFxSerilog();

// 🛡️ Use MicFx Exception Handling (after Serilog request logging)
app.UseMicFxExceptionHandling();

// Use modules with lifecycle management
await app.UseMicFxModulesAsync();

// 🗄️ Ensure all module databases are initialized (migrations + fallback)
await app.Services.EnsureModuleDatabasesAsync();

// 🌱 Run module data seeders for development/demo data
if (app.Environment.IsDevelopment() || app.Environment.IsStaging())
{
    await app.Services.RunModuleSeedersAsync();
}

// 🩺 Add health check endpoints
app.UseHealthChecks("/health");
app.UseHealthChecks("/health/modules", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = healthCheck => healthCheck.Name == "modules",
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        var response = new
        {
            Status = report.Status.ToString(),
            Duration = report.TotalDuration,
            Checks = report.Entries.ToDictionary(kvp => kvp.Key, kvp => new
            {
                Status = kvp.Value.Status.ToString(),
                Description = kvp.Value.Description,
                Data = kvp.Value.Data,
                Duration = kvp.Value.Duration
            })
        };
        await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(response));
    }
});

// Use MicFx Swagger Infrastructure (development only)
app.UseMicFxSwaggerInfrastructure(app.Environment);

// Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

// Configure MVC routing
app.UseStaticFiles();

// Configure Area routing for Admin with authorization
app.MapControllerRoute(
    name: "admin_area",
    pattern: "admin/{controller=Dashboard}/{action=Index}/{id?}")
    .RequireAuthorization("AdminAreaAccess");

// Configure other areas without authorization requirement
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapControllers();

app.Run();
