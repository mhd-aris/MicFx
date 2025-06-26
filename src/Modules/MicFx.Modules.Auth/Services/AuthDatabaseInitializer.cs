using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MicFx.Modules.Auth.Data;
using MicFx.Modules.Auth.Domain.Entities;
using MicFx.Modules.Auth.Domain.Configuration;

namespace MicFx.Modules.Auth.Services
{
    /// <summary>
    /// Database initializer untuk Auth module
    /// Membuat default roles dan admin user saat aplikasi start
    /// </summary>
    public class AuthDatabaseInitializer : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;

        public AuthDatabaseInitializer(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<Role>>();
            var config = scope.ServiceProvider.GetRequiredService<AuthConfig>();

            // Ensure database created
            await context.Database.EnsureCreatedAsync(cancellationToken);

            // Create default roles dari config
            foreach (var roleName in config.DefaultRoles)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new Role(roleName, $"Default {roleName} role", true));
                }
            }

            // Create default admin dari config
            var adminEmail = config.DefaultAdmin.Email;
            if (await userManager.FindByEmailAsync(adminEmail) == null)
            {
                var admin = new User
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FirstName = config.DefaultAdmin.FirstName,
                    LastName = config.DefaultAdmin.LastName,
                    IsActive = true,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(admin, config.DefaultAdmin.Password);
                if (result.Succeeded)
                {
                    // Assign SuperAdmin role to default admin
                    if (config.DefaultRoles.Contains("SuperAdmin"))
                    {
                        await userManager.AddToRoleAsync(admin, "SuperAdmin");
                    }
                    // Fallback to Admin if SuperAdmin doesn't exist
                    else if (config.DefaultRoles.Contains("Admin"))
                    {
                        await userManager.AddToRoleAsync(admin, "Admin");
                    }
                }
            }
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
} 