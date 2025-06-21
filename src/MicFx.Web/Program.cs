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

// 🚀 Configure Serilog as early as possible for comprehensive logging
builder.Services.AddMicFxSerilog(builder.Configuration, builder.Environment, options =>
{
    // Environment-specific configuration
    if (builder.Environment.IsDevelopment())
    {
        options.MinimumLevel = Serilog.Events.LogEventLevel.Debug;
        options.EnableRequestLogging = true;
    }
    else
    {
        options.MinimumLevel = Serilog.Events.LogEventLevel.Information;
        options.EnableRequestLogging = false; // Disable request logging in production for performance
    }
});

// Use Serilog as primary logging provider
builder.Host.UseSerilog();

// 1️⃣ Add Razor + Runtime Compilation
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


// 🔧 Add MicFx Configuration Management (Simplified)
builder.Services.AddMicFxConfigurationManagement(builder.Configuration, options =>
{
    options.ValidateOnStartup = true;
    options.ThrowOnValidationFailure = builder.Environment.IsDevelopment();
});

// 📦 Register MicFx Abstractions (interfaces available to modules)
builder.Services.AddMicFxAbstractions();

// 🏗️ Register MicFx Infrastructure implementations
builder.Services.AddMicFxInfrastructure();

// 3️⃣ Load modules with enhanced dependency and lifecycle management
builder.Services.AddMicFxModulesWithDependencyManagement();

// 🔐 Configure Authorization Policies for Admin Area
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

// 🔧 Add MicFx Swagger Infrastructure (with smart auto-routing support)
builder.Services.AddMicFxSwaggerInfrastructure();

var app = builder.Build();

// 📝 Use Serilog request logging (before exception handling for complete request lifecycle logging)
// Only enable request logging in development for performance reasons
if (app.Environment.IsDevelopment())
{
    app.UseMicFxSerilog();
}

// 🛡️ Use MicFx Exception Handling (after Serilog request logging)
app.UseMicFxExceptionHandling();

// 4️⃣ Use modules with simplified lifecycle management
await app.UseMicFxModulesAsync();

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

// 📚 Use MicFx Swagger Infrastructure (development only)
app.UseMicFxSwaggerInfrastructure(app.Environment);

// 5️⃣ Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

// 6️⃣ Configure MVC routing
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
