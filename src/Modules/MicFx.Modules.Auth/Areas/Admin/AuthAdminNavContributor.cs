using MicFx.SharedKernel.Interfaces;

namespace MicFx.Modules.Auth.Areas.Admin
{
    /// <summary>
    /// Navigation contributor for Auth module admin pages
    /// Provides navigation items for authentication and user management
    /// </summary>
    public class AuthAdminNavContributor : IAdminNavContributor
    {
        public IEnumerable<AdminNavItem> GetNavItems()
        {
            return new[]
            {
                new AdminNavItem
                {
                    Title = "Auth Dashboard",
                    Url = "/admin/auth",
                    Icon = "dashboard",
                    Order = 5,
                    Category = "Authentication",
                    RequiredRoles = new[] { "Admin", "SuperAdmin" },
                    IsActive = true
                },
                new AdminNavItem
                {
                    Title = "User Management",
                    Url = "/admin/auth/users",
                    Icon = "users",
                    Order = 10,
                    Category = "Authentication",
                    RequiredRoles = new[] { "Admin", "SuperAdmin" },
                    IsActive = true
                },
                new AdminNavItem
                {
                    Title = "Role Management",
                    Url = "/admin/auth/roles",
                    Icon = "roles",
                    Order = 20,
                    Category = "Authentication",
                    RequiredRoles = new[] { "Admin", "SuperAdmin" },
                    IsActive = true
                },
                new AdminNavItem
                {
                    Title = "Permission Management",
                    Url = "/admin/auth/permissions",
                    Icon = "permission",
                    Order = 25,
                    Category = "Authentication",
                    RequiredRoles = new[] { "Admin", "SuperAdmin" },
                    IsActive = true
                },
                new AdminNavItem
                {
                    Title = "Security Settings",
                    Url = "/admin/auth/settings",
                    Icon = "security",
                    Order = 30,
                    Category = "Authentication",
                    RequiredRoles = new[] { "SuperAdmin" },
                    IsActive = true
                }
            };
        }
    }
} 