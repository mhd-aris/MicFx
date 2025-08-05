using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MicFx.SharedKernel.Modularity;
using MicFx.Modules.Auth.Domain.Entities;
using MicFx.Core.Permissions;

namespace MicFx.Modules.Auth.Data;

/// <summary>
/// Data seeder for Auth module
/// Initializes default users, roles, and permissions for system bootstrap
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
            // Database creation is handled centrally by DatabaseExtensions
            // This prevents hanging and conflicts with migration state
            
            // Seed permissions first (required for role assignments)
            await SeedPermissionsAsync(dbContext, logger);

            // Seed default roles
            await SeedDefaultRolesAsync(roleManager, logger);

            // Assign permissions to roles
            await SeedRolePermissionsAsync(dbContext, logger);

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

    // Default system roles with clear hierarchy
    private static readonly List<RoleSeedData> DefaultRoles = new()
    {
        new("SuperAdmin", "Full system access with all permissions", 1),
        new("Admin", "Administrative access for system management", 10),
        new("User", "Standard user access with basic permissions", 100)
    };

    // Core auth permissions following module.action pattern
    private static readonly List<PermissionSeedData> DefaultPermissions = new()
    {
        // User management permissions
        new("users.view", "auth", "View Users"),
        new("users.create", "auth", "Create Users"),
        new("users.edit", "auth", "Edit Users"),
        new("users.delete", "auth", "Delete Users"),
        new("users.manage_roles", "auth", "Manage User Roles"),
        
        // Role management permissions
        new("roles.view", "auth", "View Roles"),
        new("roles.create", "auth", "Create Roles"),
        new("roles.edit", "auth", "Edit Roles"),
        new("roles.delete", "auth", "Delete Roles"),
        new("roles.manage_permissions", "auth", "Manage Role Permissions"),
        
        // Permission management
        new("permissions.view", "auth", "View Permissions"),
        new("permissions.manage", "auth", "Manage Permissions"),
        
        // System administration
        new("system.admin", "auth", "System Administration"),
        
        // Wildcard permissions for convenience
        new("users.*", "auth", "All User Operations"),
        new("roles.*", "auth", "All Role Operations"),
        new("auth.*", "auth", "All Auth Operations"),
        new("*", "system", "Global Admin Access")
    };

    // Role-permission assignments with clear mapping
    private static readonly List<RolePermissionMapping> DefaultRoleAssignments = new()
    {
        new("SuperAdmin", "*"),
        new("Admin", "auth.*"),
        new("User", "users.view"),
        new("User", "roles.view")
    };

    private static async Task SeedDefaultRolesAsync(RoleManager<Role> roleManager, ILogger logger)
    {
        foreach (var roleData in DefaultRoles)
        {
            if (!await roleManager.RoleExistsAsync(roleData.Name))
            {
                var role = new Role
                {
                    Name = roleData.Name,
                    Description = roleData.Description,
                    Priority = roleData.Priority,
                    IsSystemRole = true,
                    CreatedBy = "System"
                };

                var result = await roleManager.CreateAsync(role);
                if (result.Succeeded)
                {
                    logger.LogInformation("✅ Created role: {RoleName}", roleData.Name);
                }
                else
                {
                    logger.LogWarning("⚠️ Failed to create role {RoleName}: {Errors}", 
                        roleData.Name, string.Join(", ", result.Errors.Select(e => e.Description)));
                }
            }
            else
            {
                logger.LogDebug("Role {RoleName} already exists, skipping", roleData.Name);
            }
        }
    }

    private static async Task SeedPermissionsAsync(AuthDbContext dbContext, ILogger logger)
    {
        // Create temporary service provider for permission operations
        var serviceProvider = CreateTempServiceProvider(dbContext, logger);
        
        var permissions = DefaultPermissions.Select(p => new PermissionDefinition(
            p.Name, 
            p.Module, 
            p.DisplayName, 
            p.DisplayName
        )).ToList();

        // TODO: Replace with new permission seeding using PermissionSeedBuilder
        // await serviceProvider.SeedPermissions()
        //     .FromModule<AuthPermissions>()
        //     .AssignToRole("SuperAdmin", role => role.Include(AuthPermissionGroups.FullAccess))
        //     .ExecuteAsync();
        
        logger.LogInformation("Permission seeding will be implemented with new discoverable system");
        await Task.CompletedTask;
    }
    
    private static IServiceProvider CreateTempServiceProvider(AuthDbContext dbContext, ILogger logger)
    {
        var services = new ServiceCollection();
        services.AddSingleton(dbContext);
        services.AddSingleton(logger);
        return services.BuildServiceProvider();
    }

    private static async Task SeedRolePermissionsAsync(AuthDbContext dbContext, ILogger logger)
    {
        foreach (var mapping in DefaultRoleAssignments)
        {
            var role = await dbContext.Roles.FirstOrDefaultAsync(r => r.Name == mapping.RoleName);
            var permission = await dbContext.Permissions.FirstOrDefaultAsync(p => p.Name == mapping.PermissionName);

            if (role != null && permission != null)
            {
                await AssignPermissionToRoleAsync(dbContext, role, permission, logger);
            }
        }

        await dbContext.SaveChangesAsync();
        logger.LogInformation("🔗 Role permission assignments completed");
    }

    private static async Task AssignPermissionToRoleAsync(AuthDbContext dbContext, Role role, Permission permission, ILogger logger)
    {
        var existingAssignment = await dbContext.RolePermissions
            .FirstOrDefaultAsync(rp => rp.RoleId == role.Id && rp.PermissionId == permission.Id);

        if (existingAssignment == null)
        {
            var rolePermission = new RolePermission
            {
                RoleId = role.Id,
                PermissionId = permission.Id,
                IsActive = true,
                GrantedBy = "System",
                GrantedAt = DateTime.UtcNow
            };

            dbContext.RolePermissions.Add(rolePermission);
            logger.LogInformation("🔗 Assigned permission {Permission} to role {Role}", permission.Name, role.Name);
        }
        else
        {
            logger.LogDebug("Permission {Permission} already assigned to role {Role}, skipping", 
                permission.Name, role.Name);
        }
    }

    private static async Task SeedDefaultAdminUserAsync(UserManager<User> userManager, ILogger logger)
    {
        const string DefaultAdminEmail = "admin@micfx.dev";
        const string DefaultAdminPassword = "Admin123!";

        var existingAdmin = await userManager.FindByEmailAsync(DefaultAdminEmail);
        if (existingAdmin == null)
        {
            var adminUser = new User
            {
                UserName = DefaultAdminEmail,
                Email = DefaultAdminEmail,
                FirstName = "System",
                LastName = "Administrator",
                IsActive = true,
                EmailConfirmed = true,
                CreatedBy = "System"
            };

            var result = await userManager.CreateAsync(adminUser, DefaultAdminPassword);
            if (result.Succeeded)
            {
                // Assign SuperAdmin role
                await userManager.AddToRoleAsync(adminUser, "SuperAdmin");
                logger.LogInformation("✅ Created default admin user: {Email}", DefaultAdminEmail);
                logger.LogWarning("🔐 Default admin password: {Password} (Change in production!)", DefaultAdminPassword);
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

    // Seed data models
    private record RoleSeedData(string Name, string Description, int Priority);
    private record PermissionSeedData(string Name, string Module, string DisplayName);
    private record RolePermissionMapping(string RoleName, string PermissionName);
}