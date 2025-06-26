using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MicFx.SharedKernel.Modularity;
using MicFx.Modules.Auth.Domain.Entities;
using MicFx.Modules.Auth.Data;

namespace MicFx.Modules.Auth.Data;

/// <summary>
/// Enhanced data seeder for Auth module
/// Initialize default users, roles, and permissions with wildcard support for development
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

    private static async Task SeedPermissionsAsync(AuthDbContext dbContext, ILogger logger)
    {
        var permissions = new[]
        {
            // Auth module - User management permissions
            new Permission { Name = "users.view", Module = "auth", DisplayName = "View Users", Description = "View user list and details", IsActive = true, CreatedBy = "AuthModuleSeeder" },
            new Permission { Name = "users.create", Module = "auth", DisplayName = "Create Users", Description = "Create new users", IsActive = true, CreatedBy = "AuthModuleSeeder" },
            new Permission { Name = "users.edit", Module = "auth", DisplayName = "Edit Users", Description = "Edit user details", IsActive = true, CreatedBy = "AuthModuleSeeder" },
            new Permission { Name = "users.delete", Module = "auth", DisplayName = "Delete Users", Description = "Delete users", IsActive = true, CreatedBy = "AuthModuleSeeder" },
            new Permission { Name = "users.activate", Module = "auth", DisplayName = "Activate Users", Description = "Activate/deactivate users", IsActive = true, CreatedBy = "AuthModuleSeeder" },
            new Permission { Name = "users.roles", Module = "auth", DisplayName = "Manage User Roles", Description = "Assign roles to users", IsActive = true, CreatedBy = "AuthModuleSeeder" },

            // Auth module - Role management permissions
            new Permission { Name = "roles.view", Module = "auth", DisplayName = "View Roles", Description = "View role list and details", IsActive = true, CreatedBy = "AuthModuleSeeder" },
            new Permission { Name = "roles.create", Module = "auth", DisplayName = "Create Roles", Description = "Create new roles", IsActive = true, CreatedBy = "AuthModuleSeeder" },
            new Permission { Name = "roles.edit", Module = "auth", DisplayName = "Edit Roles", Description = "Edit role details", IsActive = true, CreatedBy = "AuthModuleSeeder" },
            new Permission { Name = "roles.delete", Module = "auth", DisplayName = "Delete Roles", Description = "Delete roles", IsActive = true, CreatedBy = "AuthModuleSeeder" },
            new Permission { Name = "roles.permissions", Module = "auth", DisplayName = "Manage Role Permissions", Description = "Assign permissions to roles", IsActive = true, CreatedBy = "AuthModuleSeeder" },

            // Auth module - Permission management
            new Permission { Name = "permissions.view", Module = "auth", DisplayName = "View Permissions", Description = "View permission list", IsActive = true, CreatedBy = "AuthModuleSeeder" },
            new Permission { Name = "permissions.manage", Module = "auth", DisplayName = "Manage Permissions", Description = "Create and manage permissions", IsActive = true, CreatedBy = "AuthModuleSeeder" },

            // System administration
            new Permission { Name = "system.admin", Module = "auth", DisplayName = "System Administration", Description = "Full system administration access", IsActive = true, CreatedBy = "AuthModuleSeeder" },

            // Wildcard permissions for admin roles
            new Permission { Name = "users.*", Module = "auth", DisplayName = "All User Operations", Description = "Wildcard permission for all user operations", IsActive = true, CreatedBy = "AuthModuleSeeder" },
            new Permission { Name = "roles.*", Module = "auth", DisplayName = "All Role Operations", Description = "Wildcard permission for all role operations", IsActive = true, CreatedBy = "AuthModuleSeeder" },
            new Permission { Name = "auth.*", Module = "auth", DisplayName = "All Auth Operations", Description = "Wildcard permission for all auth module operations", IsActive = true, CreatedBy = "AuthModuleSeeder" },
            new Permission { Name = "*", Module = "system", DisplayName = "Global Admin Access", Description = "Global wildcard permission - full system access", IsActive = true, CreatedBy = "AuthModuleSeeder" }
        };

        foreach (var permission in permissions)
        {
            var existingPermission = await dbContext.Permissions
                .FirstOrDefaultAsync(p => p.Name == permission.Name && p.Module == permission.Module);

            if (existingPermission == null)
            {
                dbContext.Permissions.Add(permission);
                logger.LogInformation("✅ Created permission: {Module}.{Permission}", permission.Module, permission.Name);
            }
            else
            {
                logger.LogDebug("Permission {Module}.{Permission} already exists, skipping", permission.Module, permission.Name);
            }
        }

        await dbContext.SaveChangesAsync();
        logger.LogInformation("📋 Permission seeding completed");
    }

    private static async Task SeedRolePermissionsAsync(AuthDbContext dbContext, ILogger logger)
    {
        // Get roles and permissions for assignment
        var superAdminRole = await dbContext.Roles.FirstOrDefaultAsync(r => r.Name == "SuperAdmin");
        var adminRole = await dbContext.Roles.FirstOrDefaultAsync(r => r.Name == "Admin");
        var userRole = await dbContext.Roles.FirstOrDefaultAsync(r => r.Name == "User");

        var globalWildcard = await dbContext.Permissions.FirstOrDefaultAsync(p => p.Name == "*");
        var authWildcard = await dbContext.Permissions.FirstOrDefaultAsync(p => p.Name == "auth.*");
        var usersWildcard = await dbContext.Permissions.FirstOrDefaultAsync(p => p.Name == "users.*");
        var rolesWildcard = await dbContext.Permissions.FirstOrDefaultAsync(p => p.Name == "roles.*");

        // SuperAdmin role gets global wildcard
        if (superAdminRole != null && globalWildcard != null)
        {
            await AssignPermissionToRole(dbContext, superAdminRole, globalWildcard, logger);
        }

        // Admin role gets auth module wildcard
        if (adminRole != null && authWildcard != null)
        {
            await AssignPermissionToRole(dbContext, adminRole, authWildcard, logger);
        }

        // User role gets basic view permissions
        if (userRole != null)
        {
            var basicPermissions = await dbContext.Permissions
                .Where(p => p.Name == "users.view" || p.Name == "roles.view")
                .ToListAsync();

            foreach (var permission in basicPermissions)
            {
                await AssignPermissionToRole(dbContext, userRole, permission, logger);
            }
        }

        await dbContext.SaveChangesAsync();
        logger.LogInformation("🔗 Role permission assignments completed");
    }

    private static async Task AssignPermissionToRole(AuthDbContext dbContext, Role role, Permission permission, ILogger logger)
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
                GrantedBy = "AuthModuleSeeder",
                GrantedAt = DateTime.UtcNow
            };

            dbContext.RolePermissions.Add(rolePermission);
            logger.LogInformation("🔗 Assigned permission {Permission} to role {Role}", permission.Name, role.Name);
        }
        else
        {
            logger.LogDebug("Permission {Permission} already assigned to role {Role}, skipping", permission.Name, role.Name);
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