using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MicFx.Modules.Auth.Domain.Entities;
using MicFx.Modules.Auth.Services;
using MicFx.Modules.Auth.Domain.DTOs;
using MicFx.Modules.Auth.Data;

namespace MicFx.Modules.Auth.Areas.Admin.Controllers
{
    /// <summary>
    /// Admin dashboard controller for auth system overview
    /// Only accessible by Admin and SuperAdmin
    /// </summary>
    [Area("Admin")]
    [Route("admin/auth")]
    [Authorize(Policy = AuthorizationPolicyService.AdminAreaPolicy)]
    public class DashboardController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<Role> _roleManager;
        private readonly AuthDbContext _context;
        private readonly ILogger<DashboardController> _logger;

        public DashboardController(
            UserManager<User> userManager,
            RoleManager<Role> roleManager,
            AuthDbContext context,
            ILogger<DashboardController> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
            _logger = logger;
        }

        // GET: /admin/auth
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "Auth Management Dashboard";

            // Get statistics
            var totalUsers = await _userManager.Users.CountAsync();
            var activeUsers = await _userManager.Users.CountAsync(u => u.IsActive);
            var totalRoles = await _roleManager.Roles.CountAsync();
            var totalPermissions = await _context.Permissions.CountAsync(p => p.IsActive);

            // Get recent users (last 10)
            var recentUsers = await _userManager.Users
                .OrderByDescending(u => u.CreatedAt)
                .Take(10)
                .ToListAsync();

            var recentUserViewModels = new List<UserViewModel>();
            foreach (var user in recentUsers)
            {
                var roles = await _userManager.GetRolesAsync(user);
                recentUserViewModels.Add(new UserViewModel
                {
                    Id = user.Id,
                    Email = user.Email ?? "",
                    FirstName = user.FirstName ?? "",
                    LastName = user.LastName ?? "",
                    FullName = $"{user.FirstName ?? ""} {user.LastName ?? ""}".Trim(),
                    IsActive = user.IsActive,
                    CreatedAt = user.CreatedAt,
                    Roles = roles.ToList()
                });
            }

            // Get top roles by user count
            var topRoles = new List<RoleViewModel>();
            var topRolesList = await _roleManager.Roles.OrderBy(r => r.Priority).Take(5).ToListAsync();
            foreach (var role in topRolesList)
            {
                var userCount = await _userManager.GetUsersInRoleAsync(role.Name ?? "");
                topRoles.Add(new RoleViewModel
                {
                    Id = role.Id,
                    Name = role.Name ?? "",
                    Description = role.Description ?? "",
                    IsSystemRole = role.IsSystemRole,
                    Priority = role.Priority,
                    UserCount = userCount.Count
                });
            }

            var viewModel = new AdminDashboardViewModel
            {
                TotalUsers = totalUsers,
                ActiveUsers = activeUsers,
                TotalRoles = totalRoles,
                TotalPermissions = totalPermissions,
                RecentUsers = recentUserViewModels,
                TopRoles = topRoles
            };

            // Redirect to role management since we don't have a specific dashboard view
            return RedirectToAction("Index", "RoleManagement");
        }

        // GET: /admin/auth/quick-stats
        [HttpGet("quick-stats")]
        public async Task<IActionResult> QuickStats()
        {
            var stats = new
            {
                TotalUsers = await _userManager.Users.CountAsync(),
                ActiveUsers = await _userManager.Users.CountAsync(u => u.IsActive),
                InactiveUsers = await _userManager.Users.CountAsync(u => !u.IsActive),
                TotalRoles = await _roleManager.Roles.CountAsync(),
                SystemRoles = await _roleManager.Roles.CountAsync(r => r.IsSystemRole),
                CustomRoles = await _roleManager.Roles.CountAsync(r => !r.IsSystemRole),
                TotalPermissions = await _context.Permissions.CountAsync(),
                ActivePermissions = await _context.Permissions.CountAsync(p => p.IsActive),
                RecentLogins = await _userManager.Users
                    .Where(u => u.LastLoginAt.HasValue && u.LastLoginAt > DateTime.UtcNow.AddDays(-7))
                    .CountAsync()
            };

            return Json(stats);
        }

        // GET: /admin/auth/user-activity
        [HttpGet("user-activity")]
        public async Task<IActionResult> UserActivity(int days = 30)
        {
            var startDate = DateTime.UtcNow.AddDays(-days);
            
            var activity = await _userManager.Users
                .Where(u => u.CreatedAt >= startDate || (u.LastLoginAt.HasValue && u.LastLoginAt >= startDate))
                .GroupBy(u => u.CreatedAt.Date)
                .Select(g => new
                {
                    Date = g.Key,
                    NewUsers = g.Count(),
                    ActiveUsers = g.Count(u => u.LastLoginAt.HasValue && u.LastLoginAt >= startDate)
                })
                .OrderBy(x => x.Date)
                .ToListAsync();

            return Json(activity);
        }

        // GET: /admin/auth/role-distribution
        [HttpGet("role-distribution")]
        public async Task<IActionResult> RoleDistribution()
        {
            var roles = await _roleManager.Roles.ToListAsync();
            var distribution = new List<object>();

            foreach (var role in roles)
            {
                var userCount = await _userManager.GetUsersInRoleAsync(role.Name ?? "");
                distribution.Add(new
                {
                    RoleName = role.Name,
                    UserCount = userCount.Count,
                    IsSystemRole = role.IsSystemRole,
                    Priority = role.Priority
                });
            }

            return Json(distribution.OrderByDescending(x => ((dynamic)x).UserCount));
        }
    }
} 