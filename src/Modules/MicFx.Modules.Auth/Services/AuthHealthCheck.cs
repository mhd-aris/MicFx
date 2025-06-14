using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MicFx.Modules.Auth.Data;
using MicFx.Modules.Auth.Domain.Entities;

namespace MicFx.Modules.Auth.Services
{
    /// <summary>
    /// Health check untuk Auth module
    /// Mengecek database connectivity, Identity services, dan basic functionality
    /// </summary>
    public class AuthHealthCheck : IHealthCheck
    {
        private readonly AuthDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<Role> _roleManager;

        public AuthHealthCheck(
            AuthDbContext context,
            UserManager<User> userManager,
            RoleManager<Role> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var healthData = new Dictionary<string, object>();

                // 1. Check database connectivity
                var canConnect = await _context.Database.CanConnectAsync(cancellationToken);
                healthData["DatabaseConnectivity"] = canConnect;

                if (!canConnect)
                {
                    return HealthCheckResult.Unhealthy(
                        "Cannot connect to Auth database",
                        data: healthData);
                }

                // 2. Check basic table existence
                var userCount = await _userManager.Users.CountAsync(cancellationToken);
                var roleCount = await _roleManager.Roles.CountAsync(cancellationToken);
                
                healthData["UserCount"] = userCount;
                healthData["RoleCount"] = roleCount;

                // 3. Check if admin user exists
                var adminExists = await _userManager.FindByEmailAsync("admin@micfx.dev") != null;
                healthData["AdminUserExists"] = adminExists;

                // 4. Check if default roles exist
                var defaultRoles = new[] { "SuperAdmin", "Admin", "User" };
                var existingRoles = new List<string>();
                
                foreach (var role in defaultRoles)
                {
                    if (await _roleManager.RoleExistsAsync(role))
                    {
                        existingRoles.Add(role);
                    }
                }
                
                healthData["DefaultRoles"] = existingRoles;
                healthData["AllDefaultRolesExist"] = existingRoles.Count == defaultRoles.Length;

                // 5. Check active users
                var activeUserCount = await _userManager.Users
                    .CountAsync(u => u.IsActive, cancellationToken);
                healthData["ActiveUserCount"] = activeUserCount;

                // Determine health status
                if (roleCount == 0)
                {
                    return HealthCheckResult.Degraded(
                        "Auth module is functional but no roles are configured",
                        data: healthData);
                }

                if (!adminExists)
                {
                    return HealthCheckResult.Degraded(
                        "Auth module is functional but admin user is missing",
                        data: healthData);
                }

                return HealthCheckResult.Healthy(
                    "Auth module is fully operational",
                    data: healthData);
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy(
                    "Auth module health check failed",
                    ex,
                    new Dictionary<string, object>
                    {
                        ["Error"] = ex.Message,
                        ["StackTrace"] = ex.StackTrace ?? "No stack trace available"
                    });
            }
        }
    }
} 