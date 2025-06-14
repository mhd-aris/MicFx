using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MicFx.Mvc.Web.Admin.Services;

namespace MicFx.Mvc.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Route("admin")]
[Authorize(Policy = "AdminAreaAccess")]
public class DashboardController : Controller
{
    private readonly ILogger<DashboardController> _logger;
    private readonly AdminNavDiscoveryService _navDiscoveryService;
    private readonly AdminModuleScanner _moduleScanner;

    public DashboardController(
        ILogger<DashboardController> logger,
        AdminNavDiscoveryService navDiscoveryService,
        AdminModuleScanner moduleScanner)
    {
        _logger = logger;
        _navDiscoveryService = navDiscoveryService;
        _moduleScanner = moduleScanner;
    }

    [HttpGet]
    [Route("")]
    [Route("dashboard")]
    public async Task<IActionResult> Index()
    {
        _logger.LogInformation("Admin Dashboard accessed");
        
        // Get module information
        var scanResults = _moduleScanner.GetScanResults();
        var navigationItems = await _navDiscoveryService.GetNavigationItemsAsync(HttpContext);
        var navigationByCategory = await _navDiscoveryService.GetNavigationItemsByCategoryAsync(HttpContext);
        
        var model = new DashboardViewModel
        {
            WelcomeMessage = "Selamat datang di MicFx Admin Panel",
            CurrentDateTime = DateTime.Now,
            SystemInfo = new SystemInfoViewModel
            {
                ApplicationName = "MicFx Framework",
                Version = "1.0.0",
                Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"
            },
            ModuleInfo = new ModuleInfoViewModel
            {
                TotalModules = scanResults.ScannedAssemblies,
                LoadedModules = scanResults.AssemblyNames.ToList(),
                NavigationContributors = scanResults.Contributors.Count,
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
    public Dictionary<string, List<MicFx.SharedKernel.Interfaces.AdminNavItem>> NavigationByCategory { get; set; } = new();
} 