using MicFx.SharedKernel.Interfaces;

namespace MicFx.Modules.HelloWorld.Areas.Admin
{
    /// <summary>
    /// Navigation contributor for HelloWorld module admin pages
    /// </summary>
    public class HelloWorldAdminNavContributor : IAdminNavContributor
    {
        public IEnumerable<AdminNavItem> GetNavItems()
        {
            return new[]
            {
                new AdminNavItem
                {
                    Title = "Hello World",
                    Url = "/admin/hello-world",
                    Icon = "hello",
                    Order = 10,
                    Category = "Modules",
                    RequiredRoles = null, // Temporarily disabled for demo
                    IsActive = true
                },
                new AdminNavItem
                {
                    Title = "Hello Settings",
                    Url = "/admin/hello-world/settings",
                    Icon = "settings",
                    Order = 20,
                    Category = "Modules",
                    RequiredRoles = null, // Temporarily disabled for demo
                    IsActive = true
                }
            };
        }
    }
} 