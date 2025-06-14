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
        private readonly AdminModuleScanner _moduleScanner;

        public DiagnosticsController(
            AdminNavDiscoveryService navDiscoveryService,
            AdminModuleScanner moduleScanner)
        {
            _navDiscoveryService = navDiscoveryService;
            _moduleScanner = moduleScanner;
        }

        /// <summary>
        /// Main diagnostics page
        /// </summary>
        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "Admin Navigation Diagnostics";

            var model = new DiagnosticsViewModel
            {
                ScanResults = _moduleScanner.GetScanResults(),
                NavigationItems = await _navDiscoveryService.GetNavigationItemsAsync(HttpContext),
                NavigationByCategory = await _navDiscoveryService.GetNavigationItemsByCategoryAsync(HttpContext),
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
        /// Clear navigation cache
        /// </summary>
        [HttpPost("clear-cache")]
        public IActionResult ClearCache()
        {
            _navDiscoveryService.ClearUserCache(User);
            TempData["SuccessMessage"] = "Navigation cache cleared successfully!";
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Get navigation items as JSON for API testing
        /// </summary>
        [HttpGet("api/navigation")]
        public async Task<IActionResult> GetNavigationJson()
        {
            var navItems = await _navDiscoveryService.GetNavigationItemsAsync(HttpContext);
            return Json(navItems);
        }

        /// <summary>
        /// Get scan results as JSON
        /// </summary>
        [HttpGet("api/scan-results")]
        public IActionResult GetScanResultsJson()
        {
            var scanResults = _moduleScanner.GetScanResults();
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
                
                var navItems = await _navDiscoveryService.GetNavigationItemsAsync(testContext);
                
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