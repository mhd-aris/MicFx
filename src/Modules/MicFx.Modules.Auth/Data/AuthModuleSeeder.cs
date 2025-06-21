using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MicFx.SharedKernel.Modularity;
using MicFx.Modules.Auth.Domain.Entities;
using MicFx.Modules.Auth.Data;

namespace MicFx.Modules.Auth.Data;

/// <summary>
/// Data seeder untuk Auth module
/// Initialize default users, roles, dan permissions untuk development
/// </summary>
public class AuthModuleSeeder : IModuleSeeder
{
    public string ModuleName => "Auth";
    public int Priority => 1; // High priority - Auth module loads first

    public async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<AuthModuleSeeder>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<Role>>();
        var dbContext = scope.ServiceProvider.GetRequiredService<AuthDbContext>();

        logger.LogInformation("🌱 Starting Auth module data seeding...");

        try
        {
            // Ensure database is created
            await dbContext.Database.EnsureCreatedAsync();

            // Seed default roles
            await SeedDefaultRolesAsync(roleManager, logger);

            // Seed default admin user
            await SeedDefaultAdminUserAsync(userManager, logger);

            logger.LogInformation("✅ Auth module data seeding completed successfully");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "❌ Error during Auth module data seeding");
            throw;
        }
    }

    private static async Task SeedDefaultRolesAsync(RoleManager<Role> roleManager, ILogger logger)
    {
        var defaultRoles = new[]
        {
            new { Name = "SuperAdmin", Description = "Full system access", Priority = 1 },
            new { Name = "Admin", Description = "Administrative access", Priority = 10 },
            new { Name = "User", Description = "Standard user access", Priority = 100 }
        };

        foreach (var roleInfo in defaultRoles)
        {
            if (!await roleManager.RoleExistsAsync(roleInfo.Name))
            {
                var role = new Role
                {
                    Name = roleInfo.Name,
                    Description = roleInfo.Description,
                    Priority = roleInfo.Priority,
                    IsSystemRole = true,
                    CreatedBy = "AuthModuleSeeder"
                };

                var result = await roleManager.CreateAsync(role);
                if (result.Succeeded)
                {
                    logger.LogInformation("✅ Created role: {RoleName}", roleInfo.Name);
                }
                else
                {
                    logger.LogWarning("⚠️ Failed to create role {RoleName}: {Errors}", 
                        roleInfo.Name, string.Join(", ", result.Errors.Select(e => e.Description)));
                }
            }
            else
            {
                logger.LogDebug("Role {RoleName} already exists, skipping", roleInfo.Name);
            }
        }
    }

    private static async Task SeedDefaultAdminUserAsync(UserManager<User> userManager, ILogger logger)
    {
        const string adminEmail = "admin@micfx.dev";
        const string adminPassword = "Admin123!";

        var existingAdmin = await userManager.FindByEmailAsync(adminEmail);
        if (existingAdmin == null)
        {
            var adminUser = new User
            {
                UserName = adminEmail,
                Email = adminEmail,
                FirstName = "System",
                LastName = "Administrator",
                IsActive = true,
                EmailConfirmed = true,
                CreatedBy = "AuthModuleSeeder"
            };

            var result = await userManager.CreateAsync(adminUser, adminPassword);
            if (result.Succeeded)
            {
                // Assign SuperAdmin role
                await userManager.AddToRoleAsync(adminUser, "SuperAdmin");
                logger.LogInformation("✅ Created default admin user: {Email}", adminEmail);
                logger.LogWarning("🔐 Default admin password: {Password} (Change in production!)", adminPassword);
            }
            else
            {
                logger.LogWarning("⚠️ Failed to create admin user: {Errors}", 
                    string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }
        else
        {
            logger.LogDebug("Admin user already exists, skipping");
        }
    }
} 