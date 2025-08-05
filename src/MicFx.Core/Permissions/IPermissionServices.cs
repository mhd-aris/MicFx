using System.Security.Claims;

namespace MicFx.Core.Permissions;

/// <summary>
/// Simple permission definition for manual registration
/// Used when not using attribute-based discovery
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
/// Discovery result from scanning permission modules
/// Contains all information needed to register permissions
/// </summary>
public class PermissionDiscoveryResult
{
    /// <summary>
    /// Module name
    /// </summary>
    public string ModuleName { get; set; } = string.Empty;

    /// <summary>
    /// All discovered permissions in this module
    /// </summary>
    public List<DiscoveredPermission> Permissions { get; set; } = new();
}

/// <summary>
/// Individual permission discovered from attribute scanning
/// </summary>
public class DiscoveredPermission
{
    /// <summary>
    /// Permission name/code (e.g., "users.view")
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Human-readable display name
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Detailed description
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Module this permission belongs to
    /// </summary>
    public string Module { get; set; } = string.Empty;

    /// <summary>
    /// Category within module (optional)
    /// </summary>
    public string? Category { get; set; }

    /// <summary>
    /// Whether this is a system permission
    /// </summary>
    public bool IsSystemPermission { get; set; }

    /// <summary>
    /// Full permission name with module prefix
    /// </summary>
    public string FullName => $"{Module}.{Name}";
}

/// <summary>
/// Service interface for discovering permissions from assemblies
/// </summary>
public interface IPermissionDiscoveryService
{
    /// <summary>
    /// Scan all loaded assemblies for permission modules
    /// </summary>
    /// <returns>List of discovered permission modules</returns>
    List<PermissionDiscoveryResult> DiscoverPermissions();

    /// <summary>
    /// Scan specific assembly for permission modules
    /// </summary>
    /// <param name="assembly">Assembly to scan</param>
    /// <returns>List of discovered permission modules</returns>
    List<PermissionDiscoveryResult> DiscoverPermissions(System.Reflection.Assembly assembly);
}

/// <summary>
/// Permission service with policy-based authorization
/// </summary>
public interface IPermissionService
{
    /// <summary>
    /// Check if user has specific permission
    /// </summary>
    /// <param name="user">User claims principal</param>
    /// <param name="permission">Permission name</param>
    /// <returns>True if user has permission</returns>
    Task<bool> HasPermissionAsync(ClaimsPrincipal user, string permission);

    /// <summary>
    /// Check if user has any of the specified permissions
    /// </summary>
    /// <param name="user">User claims principal</param>
    /// <param name="permissions">Array of permission names</param>
    /// <returns>True if user has any of the permissions</returns>
    Task<bool> HasAnyPermissionAsync(ClaimsPrincipal user, params string[] permissions);

    /// <summary>
    /// Check if user has all of the specified permissions
    /// </summary>
    /// <param name="user">User claims principal</param>
    /// <param name="permissions">Array of permission names</param>
    /// <returns>True if user has all permissions</returns>
    Task<bool> HasAllPermissionsAsync(ClaimsPrincipal user, params string[] permissions);

    /// <summary>
    /// Get all permissions for user with module grouping
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <returns>Dictionary grouped by module</returns>
    Task<Dictionary<string, List<string>>> GetUserPermissionsByModuleAsync(string userId);

    /// <summary>
    /// Clear all permission caches
    /// </summary>
    Task ClearAllCachesAsync();
}
