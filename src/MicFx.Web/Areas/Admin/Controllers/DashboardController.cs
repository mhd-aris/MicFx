using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MicFx.Web.Admin.Services;
using MicFx.SharedKernel.Interfaces;
using System.Reflection;

namespace MicFx.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Route("admin")]
[Authorize(Policy = "AdminAreaAccess")]
public class DashboardController : Controller
{
    private readonly ILogger<DashboardController> _logger;
    private readonly AdminNavDiscoveryService _navDiscoveryService;
    private readonly IServiceProvider _serviceProvider;

    public DashboardController(
        ILogger<DashboardController> logger,
        AdminNavDiscoveryService navDiscoveryService,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _navDiscoveryService = navDiscoveryService;
        _serviceProvider = serviceProvider;
    }

    [HttpGet]
    [Route("")]
    [Route("dashboard")]
    public async Task<IActionResult> Index()
    {
        _logger.LogInformation("Admin Dashboard accessed");
        
        // Get real navigation information
        var navigationItems = await _navDiscoveryService.GetNavigationItemsAsync();
        var navigationByCategory = navigationItems.GroupBy(x => x.Category).ToDictionary(g => g.Key, g => g.ToList());
        
        // Get real module information dynamically
        var contributors = _serviceProvider.GetServices<IAdminNavContributor>().ToList();
        var moduleAssemblies = contributors
            .Select(c => c.GetType().Assembly.GetName().Name)
            .Where(name => !string.IsNullOrEmpty(name) && name.Contains("Modules"))
            .Distinct()
            .ToList();

        var model = new DashboardViewModel
        {
            WelcomeMessage = "Welcome to MicFx Admin Panel",
            CurrentDateTime = DateTime.Now,
            SystemInfo = new SystemInfoViewModel
            {
                ApplicationName = "MicFx Framework",
                Version = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "1.0.0",
                Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"
            },
            ModuleInfo = new ModuleInfoViewModel
            {
                TotalModules = moduleAssemblies.Count,
                LoadedModules = moduleAssemblies!,
                NavigationContributors = contributors.Count,
                TotalNavigationItems = navigationItems.Count(),
                NavigationByCategory = navigationByCategory
            }
        };

        return View(model);
    }
}

public class DashboardViewModel
{
    public string WelcomeMessage { get; set; } = string.Empty;
    public DateTime CurrentDateTime { get; set; }
    public SystemInfoViewModel SystemInfo { get; set; } = new();
    public ModuleInfoViewModel ModuleInfo { get; set; } = new();
}

public class SystemInfoViewModel
{
    public string ApplicationName { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string Environment { get; set; } = string.Empty;
}

public class ModuleInfoViewModel
{
    public int TotalModules { get; set; }
    public List<string> LoadedModules { get; set; } = new();
    public int NavigationContributors { get; set; }
    public int TotalNavigationItems { get; set; }
    public Dictionary<string, List<AdminNavItem>> NavigationByCategory { get; set; } = new();
} 