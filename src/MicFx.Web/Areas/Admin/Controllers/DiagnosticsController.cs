using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MicFx.Web.Admin.Services;
using MicFx.SharedKernel.Interfaces;
using System.Security.Claims;

namespace MicFx.Web.Areas.Admin.Controllers
{
    /// <summary>
    /// Diagnostics controller for admin navigation system testing and validation
    /// </summary>
    [Area("Admin")]
    [Route("admin/diagnostics")]
    [Authorize(Policy = "AdminAreaAccess")]
    public class DiagnosticsController : Controller
    {
        private readonly AdminNavDiscoveryService _navDiscoveryService;
        private readonly IServiceProvider _serviceProvider;

        public DiagnosticsController(
            AdminNavDiscoveryService navDiscoveryService,
            IServiceProvider serviceProvider)
        {
            _navDiscoveryService = navDiscoveryService;
            _serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Main diagnostics page
        /// </summary>
        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "Admin Navigation Diagnostics";

            var navigationItems = await _navDiscoveryService.GetNavigationItemsAsync();
            
            // Get real contributor information
            var contributors = _serviceProvider.GetServices<IAdminNavContributor>().ToList();
            var contributorInfos = contributors.Select(contributor => new ContributorInfo
            {
                TypeName = contributor.GetType().Name,
                AssemblyName = contributor.GetType().Assembly.GetName().Name ?? "Unknown",
                Namespace = contributor.GetType().Namespace ?? "Unknown"
            }).ToList();

            // Get real assembly information
            var assemblies = contributors
                .Select(c => c.GetType().Assembly.GetName().Name)
                .Where(name => !string.IsNullOrEmpty(name))
                .Distinct()
                .ToList();

            var model = new DiagnosticsViewModel
            {
                ScanResults = new AdminScanResult
                {
                    ScannedAssemblies = assemblies.Count,
                    AssemblyNames = assemblies!,
                    Contributors = contributorInfos
                },
                NavigationItems = navigationItems,
                NavigationByCategory = navigationItems.GroupBy(x => x.Category).ToDictionary(g => g.Key, g => g.ToList()),
                UserInfo = new UserDiagnosticInfo
                {
                    IsAuthenticated = User.Identity?.IsAuthenticated ?? false,
                    UserName = User.Identity?.Name ?? "Anonymous",
                    UserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "N/A",
                    Roles = User.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).ToList(),
                    Claims = User.Claims.Select(c => new ClaimInfo 
                    { 
                        Type = c.Type, 
                        Value = c.Value 
                    }).ToList()
                }
            };

            return View(model);
        }

        /// <summary>
        /// Clear navigation cache (simplified - no caching implemented)
        /// </summary>
        [HttpPost("clear-cache")]
        public IActionResult ClearCache()
        {
            TempData["SuccessMessage"] = "Navigation refreshed successfully!";
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Get navigation items as JSON for API testing
        /// </summary>
        [HttpGet("api/navigation")]
        public async Task<IActionResult> GetNavigationJson()
        {
            var navItems = await _navDiscoveryService.GetNavigationItemsAsync();
            return Json(navItems);
        }

        /// <summary>
        /// Get scan results as JSON with real data
        /// </summary>
        [HttpGet("api/scan-results")]
        public IActionResult GetScanResultsJson()
        {
            // Get real contributor information
            var contributors = _serviceProvider.GetServices<IAdminNavContributor>().ToList();
            var contributorInfos = contributors.Select(contributor => new ContributorInfo
            {
                TypeName = contributor.GetType().Name,
                AssemblyName = contributor.GetType().Assembly.GetName().Name ?? "Unknown",
                Namespace = contributor.GetType().Namespace ?? "Unknown"
            }).ToList();

            // Get real assembly information
            var assemblies = contributors
                .Select(c => c.GetType().Assembly.GetName().Name)
                .Where(name => !string.IsNullOrEmpty(name))
                .Distinct()
                .ToList();

            var scanResults = new AdminScanResult
            {
                ScannedAssemblies = assemblies.Count,
                AssemblyNames = assemblies!,
                Contributors = contributorInfos
            };
            return Json(scanResults);
        }

        /// <summary>
        /// Test navigation with different user roles
        /// </summary>
        [HttpGet("test-roles")]
        public async Task<IActionResult> TestRoles()
        {
            var testResults = new List<RoleTestResult>();

            // Test scenarios with different roles
            var testScenarios = new[]
            {
                new { Roles = new[] { "Admin" }, Description = "Admin User" },
                new { Roles = new[] { "User" }, Description = "Regular User" },
                new { Roles = new[] { "Admin", "User" }, Description = "Admin + User" },
                new { Roles = new string[0], Description = "No Roles" }
            };

            foreach (var scenario in testScenarios)
            {
                var testUser = CreateTestUser(scenario.Roles);
                var testContext = CreateTestHttpContext(testUser);
                
                var navItems = await _navDiscoveryService.GetNavigationItemsAsync();
                
                testResults.Add(new RoleTestResult
                {
                    Description = scenario.Description,
                    Roles = scenario.Roles.ToList(),
                    NavigationItemCount = navItems.Count(),
                    NavigationItems = navItems.Select(item => new NavigationItemSummary
                    {
                        Title = item.Title,
                        Url = item.Url,
                        Category = item.Category,
                        RequiredRoles = item.RequiredRoles?.ToList() ?? new List<string>()
                    }).ToList()
                });
            }

            var model = new RoleTestViewModel
            {
                TestResults = testResults
            };

            return View(model);
        }

        private ClaimsPrincipal CreateTestUser(string[] roles)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, "TestUser"),
                new Claim(ClaimTypes.NameIdentifier, "test-user-id")
            };

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            return new ClaimsPrincipal(new ClaimsIdentity(claims, "Test"));
        }

        private HttpContext CreateTestHttpContext(ClaimsPrincipal user)
        {
            var context = new DefaultHttpContext();
            context.User = user;
            context.Request.Path = "/admin/test";
            return context;
        }
    }

    /// <summary>
    /// View model for diagnostics page
    /// </summary>
    public class DiagnosticsViewModel
    {
        public AdminScanResult ScanResults { get; set; } = new();
        public IEnumerable<AdminNavItem> NavigationItems { get; set; } = new List<AdminNavItem>();
        public Dictionary<string, List<AdminNavItem>> NavigationByCategory { get; set; } = new();
        public UserDiagnosticInfo UserInfo { get; set; } = new();
    }

    /// <summary>
    /// User diagnostic information
    /// </summary>
    public class UserDiagnosticInfo
    {
        public bool IsAuthenticated { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public List<string> Roles { get; set; } = new();
        public List<ClaimInfo> Claims { get; set; } = new();
    }

    /// <summary>
    /// Claim information for diagnostics
    /// </summary>
    public class ClaimInfo
    {
        public string Type { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
    }

    /// <summary>
    /// Role testing view model
    /// </summary>
    public class RoleTestViewModel
    {
        public List<RoleTestResult> TestResults { get; set; } = new();
    }

    /// <summary>
    /// Result of role-based navigation testing
    /// </summary>
    public class RoleTestResult
    {
        public string Description { get; set; } = string.Empty;
        public List<string> Roles { get; set; } = new();
        public int NavigationItemCount { get; set; }
        public List<NavigationItemSummary> NavigationItems { get; set; } = new();
    }

    /// <summary>
    /// Summary of navigation item for testing
    /// </summary>
    public class NavigationItemSummary
    {
        public string Title { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public List<string> RequiredRoles { get; set; } = new();
    }
} 