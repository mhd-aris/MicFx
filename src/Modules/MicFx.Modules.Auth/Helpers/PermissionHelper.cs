using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using MicFx.Modules.Auth.Data;
using MicFx.Modules.Auth.Domain.Entities;

namespace MicFx.Modules.Auth.Helpers;

/// <summary>
/// Helper utilities for developers to implement permissions easily
/// Provides commonly used methods with consistent patterns
/// </summary>
public static class PermissionHelper
{
    /// <summary>
    /// Seed permissions for module with consistent pattern
    /// Usage: await PermissionHelper.SeedPermissionsAsync(serviceProvider, YourModulePermissions.AllPermissions);
    /// </summary>
    public static async Task SeedPermissionsAsync(IServiceProvider serviceProvider, List<PermissionDefinition> permissions)
    {
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger>();

        foreach (var permissionDef in permissions)
        {
            var existingPermission = await dbContext.Permissions
                .FirstOrDefaultAsync(p => p.Name == permissionDef.Name && p.Module == permissionDef.Module);

            if (existingPermission == null)
            {
                var permission = new Permission
                {
                    Name = permissionDef.Name,
                    Module = permissionDef.Module,
                    DisplayName = permissionDef.DisplayName,
                    Description = permissionDef.Description,
                    Category = permissionDef.Category ?? ExtractCategoryFromName(permissionDef.Name),
                    IsSystemPermission = permissionDef.IsSystemPermission,
                    IsActive = true,
                    CreatedBy = "System"
                };

                dbContext.Permissions.Add(permission);
                logger.LogInformation("✅ Created permission: {Module}.{Permission}", 
                    permissionDef.Module, permissionDef.Name);
            }
            else
            {
                // Update existing permission properties if needed
                var updated = false;
                
                if (existingPermission.DisplayName != permissionDef.DisplayName)
                {
                    existingPermission.DisplayName = permissionDef.DisplayName;
                    updated = true;
                }
                
                if (existingPermission.Description != permissionDef.Description)
                {
                    existingPermission.Description = permissionDef.Description;
                    updated = true;
                }

                if (updated)
                {
                    existingPermission.UpdatedAt = DateTime.UtcNow;
                    existingPermission.UpdatedBy = "System";
                    logger.LogInformation("🔄 Updated permission: {Module}.{Permission}", 
                        permissionDef.Module, permissionDef.Name);
                }
                else
                {
                    logger.LogDebug("Permission {Module}.{Permission} already exists and up to date", 
                        permissionDef.Module, permissionDef.Name);
                }
            }
        }

        await dbContext.SaveChangesAsync();
        logger.LogInformation("📋 Completed seeding {Count} permissions", permissions.Count);
    }

    /// <summary>
    /// Seed role-permission assignments with validation
    /// Usage: await PermissionHelper.SeedRolePermissionsAsync(serviceProvider, assignments);
    /// </summary>
    public static async Task SeedRolePermissionsAsync(IServiceProvider serviceProvider, List<RolePermissionAssignment> assignments)
    {
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger>();

        foreach (var assignment in assignments)
        {
            var role = await dbContext.Roles.FirstOrDefaultAsync(r => r.Name == assignment.RoleName);
            if (role == null)
            {
                logger.LogWarning("⚠️ Role '{RoleName}' not found for permission assignment", assignment.RoleName);
                continue;
            }

            var permission = await dbContext.Permissions.FirstOrDefaultAsync(p => p.Name == assignment.PermissionName);
            if (permission == null)
            {
                logger.LogWarning("⚠️ Permission '{PermissionName}' not found for role assignment", assignment.PermissionName);
                continue;
            }

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
                    GrantedAt = DateTime.UtcNow,
                    Reason = assignment.Reason ?? $"Default assignment for {assignment.RoleName} role"
                };

                dbContext.RolePermissions.Add(rolePermission);
                logger.LogInformation("🔗 Assigned permission '{PermissionName}' to role '{RoleName}'", 
                    assignment.PermissionName, assignment.RoleName);
            }
            else if (!existingAssignment.IsActive)
            {
                // Reactivate if previously deactivated
                existingAssignment.IsActive = true;
                existingAssignment.GrantedAt = DateTime.UtcNow;
                existingAssignment.GrantedBy = "System";
                logger.LogInformation("🔄 Reactivated permission '{PermissionName}' for role '{RoleName}'", 
                    assignment.PermissionName, assignment.RoleName);
            }
            else
            {
                logger.LogDebug("Assignment '{PermissionName}' to '{RoleName}' already exists and active", 
                    assignment.PermissionName, assignment.RoleName);
            }
        }

        await dbContext.SaveChangesAsync();
        logger.LogInformation("🔗 Completed seeding {Count} role-permission assignments", assignments.Count);
    }

    /// <summary>
    /// Create full permission name with module prefix
    /// Usage: var fullName = PermissionHelper.CreateFullPermissionName("items.view", "inventory");
    /// Result: "inventory.items.view"
    /// </summary>
    public static string CreateFullPermissionName(string permission, string moduleName)
    {
        if (string.IsNullOrEmpty(permission) || string.IsNullOrEmpty(moduleName))
        {
            return permission ?? string.Empty;
        }

        // If permission already has module prefix, return as-is
        if (permission.StartsWith($"{moduleName}."))
        {
            return permission;
        }

        return $"{moduleName.ToLowerInvariant()}.{permission}";
    }

    /// <summary>
    /// Extract category from permission name
    /// Usage: var category = PermissionHelper.ExtractCategoryFromName("users.view"); // Returns: "Users"
    /// </summary>
    public static string ExtractCategoryFromName(string permissionName)
    {
        if (string.IsNullOrEmpty(permissionName))
            return "General";

        var parts = permissionName.Split('.');
        if (parts.Length >= 2)
        {
            // Take the first part as entity name and capitalize
            var entityName = parts[0];
            return char.ToUpperInvariant(entityName[0]) + entityName[1..].ToLowerInvariant();
        }

        return "General";
    }

    /// <summary>
    /// Validate permission name format
    /// Usage: var isValid = PermissionHelper.IsValidPermissionName("users.view");
    /// </summary>
    public static bool IsValidPermissionName(string permissionName)
    {
        if (string.IsNullOrWhiteSpace(permissionName))
            return false;

        // Must contain at least one dot
        if (!permissionName.Contains('.'))
            return false;

        // Must not start or end with dot
        if (permissionName.StartsWith('.') || permissionName.EndsWith('.'))
            return false;

        // Must not contain consecutive dots
        if (permissionName.Contains(".."))
            return false;

        // Must contain only alphanumeric, dots, and underscores
        return System.Text.RegularExpressions.Regex.IsMatch(permissionName, @"^[a-zA-Z0-9._]+$");
    }

    /// <summary>
    /// Generate standard CRUD permissions for entity
    /// Usage: var permissions = PermissionHelper.GenerateCrudPermissions("items", "inventory", "Items");
    /// </summary>
    public static List<PermissionDefinition> GenerateCrudPermissions(
        string entityName, 
        string moduleName, 
        string? displayEntityName = null)
    {
        displayEntityName ??= char.ToUpperInvariant(entityName[0]) + entityName[1..].ToLowerInvariant();
        
        return new List<PermissionDefinition>
        {
            new($"{entityName}.view", moduleName, $"View {displayEntityName}", $"Can view {displayEntityName.ToLowerInvariant()}"),
            new($"{entityName}.create", moduleName, $"Create {displayEntityName}", $"Can create new {displayEntityName.ToLowerInvariant()}"),
            new($"{entityName}.edit", moduleName, $"Edit {displayEntityName}", $"Can edit existing {displayEntityName.ToLowerInvariant()}"),
            new($"{entityName}.delete", moduleName, $"Delete {displayEntityName}", $"Can delete {displayEntityName.ToLowerInvariant()}")
        };
    }

    /// <summary>
    /// Generate standard role assignments for CRUD permissions
    /// Usage: var assignments = PermissionHelper.GenerateStandardRoleAssignments(permissions);
    /// </summary>
    public static List<RolePermissionAssignment> GenerateStandardRoleAssignments(List<PermissionDefinition> permissions)
    {
        var assignments = new List<RolePermissionAssignment>();

        foreach (var permission in permissions)
        {
            // SuperAdmin gets all permissions
            assignments.Add(new RolePermissionAssignment("SuperAdmin", permission.Name, "Full system access"));

            // Admin gets all except delete (conservative approach)
            if (!permission.Name.EndsWith(".delete"))
            {
                assignments.Add(new RolePermissionAssignment("Admin", permission.Name, "Administrative access"));
            }

            // Users only get view permissions by default
            if (permission.Name.EndsWith(".view"))
            {
                assignments.Add(new RolePermissionAssignment("User", permission.Name, "Standard user access"));
            }
        }

        return assignments;
    }
}

/// <summary>
/// Data structure for permission definition
/// Helps developers define permissions in a type-safe manner
/// </summary>
public record PermissionDefinition(
    string Name,
    string Module,
    string DisplayName,
    string Description,
    string? Category = null,
    bool IsSystemPermission = false
);

/// <summary>
/// Data structure for role-permission assignment
/// Helps developers define role assignments easily
/// </summary>
public record RolePermissionAssignment(
    string RoleName,
    string PermissionName,
    string? Reason = null
);
