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
    [Authorize(Policy = "AdminAreaAccess")]
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
            ViewData["Title"] = "Auth Dashboard";

            // Get statistics
            var totalUsers = await _userManager.Users.CountAsync();
            var activeUsers = await _userManager.Users.CountAsync(u => u.IsActive);
            var totalRoles = await _roleManager.Roles.CountAsync();
            var totalPermissions = await _context.Permissions.CountAsync();

            // Get recent users
            var recentUsers = await _userManager.Users
                .OrderByDescending(u => u.CreatedAt)
                .Take(5)
                .Select(u => new UserViewModel
                {
                    Id = u.Id,
                    Email = u.Email ?? "",
                    FirstName = u.FirstName ?? "",
                    LastName = u.LastName ?? "",
                    FullName = $"{u.FirstName ?? ""} {u.LastName ?? ""}".Trim(),
                    IsActive = u.IsActive,
                    CreatedAt = u.CreatedAt
                })
                .ToListAsync();

            // Get top roles by user count - menggunakan query yang bisa di-translate
            var rolesWithUserCount = await _context.Roles
                .Select(r => new
                {
                    Role = r,
                    UserCount = _context.UserRoles.Count(ur => ur.RoleId == r.Id)
                })
                .OrderByDescending(x => x.UserCount)
                .Take(5)
                .ToListAsync();

            var topRoles = rolesWithUserCount.Select(x => new RoleViewModel
            {
                Id = x.Role.Id,
                Name = x.Role.Name ?? "",
                Description = x.Role.Description ?? "",
                UserCount = x.UserCount,
                IsSystemRole = x.Role.IsSystemRole,
                Priority = x.Role.Priority
            }).ToList();

            var viewModel = new AdminDashboardViewModel
            {
                TotalUsers = totalUsers,
                ActiveUsers = activeUsers,
                TotalRoles = totalRoles,
                TotalPermissions = totalPermissions,
                RecentUsers = recentUsers,
                TopRoles = topRoles
            };

            return View("AuthDashboard", viewModel);
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