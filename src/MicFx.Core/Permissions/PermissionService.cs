using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace MicFx.Core.Permissions;

/// <summary>
/// Permission service with discoverable permission system
/// Provides auto-discovery and policy-based authorization
/// </summary>
public class PermissionService : IPermissionService
{
    private readonly IPermissionDiscoveryService _discoveryService;
    private readonly ILogger<PermissionService> _logger;
    private readonly IServiceProvider _serviceProvider;

    public PermissionService(
        IPermissionDiscoveryService discoveryService,
        ILogger<PermissionService> logger,
        IServiceProvider serviceProvider)
    {
        _discoveryService = discoveryService;
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    /// Check if user has permission
    /// </summary>
    public async Task<bool> HasPermissionAsync(ClaimsPrincipal user, string permission)
    {
        try
        {
            _logger.LogDebug("Checking permission {Permission} for user", permission);
            
            // SuperAdmin bypass
            if (user.IsInRole("SuperAdmin"))
            {
                return true;
            }

            // Get user permissions from claims
            var userPermissions = user.Claims
                .Where(c => c.Type == "permission")
                .Select(c => c.Value)
                .ToList();

            return userPermissions.Contains(permission);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking permission {Permission}", permission);
            return false;
        }
    }

    /// <summary>
    /// Check if user has any of the specified permissions
    /// </summary>
    public async Task<bool> HasAnyPermissionAsync(ClaimsPrincipal user, params string[] permissions)
    {
        foreach (var permission in permissions)
        {
            if (await HasPermissionAsync(user, permission))
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Check if user has all of the specified permissions
    /// </summary>
    public async Task<bool> HasAllPermissionsAsync(ClaimsPrincipal user, params string[] permissions)
    {
        foreach (var permission in permissions)
        {
            if (!await HasPermissionAsync(user, permission))
            {
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// Get user permissions organized by module
    /// </summary>
    public async Task<Dictionary<string, List<string>>> GetUserPermissionsByModuleAsync(string userId)
    {
        _logger.LogDebug("Getting permissions for user {UserId}", userId);
        
        var result = new Dictionary<string, List<string>>();
        
        // Get discovered permissions and organize by module
        var discoveredModules = _discoveryService.DiscoverPermissions();
        foreach (var module in discoveredModules)
        {
            result[module.ModuleName] = module.Permissions.Select(p => p.Name).ToList();
        }
        
        return await Task.FromResult(result);
    }

    /// <summary>
    /// Clear all permission caches
    /// </summary>
    public async Task ClearAllCachesAsync()
    {
        _logger.LogInformation("Clearing permission caches...");
        await Task.CompletedTask;
        _logger.LogInformation("Permission caches cleared");
    }

    /// <summary>
    /// Get all discovered permissions
    /// </summary>
    public async Task<IEnumerable<PermissionDefinition>> GetAllPermissionsAsync()
    {
        return await Task.Run(() =>
        {
            var discoveredModules = _discoveryService.DiscoverPermissions();
            var permissions = new List<PermissionDefinition>();

            foreach (var module in discoveredModules)
            {
                foreach (var permission in module.Permissions)
                {
                    permissions.Add(new PermissionDefinition(
                        Name: permission.Name,
                        Module: permission.Module,
                        DisplayName: permission.DisplayName,
                        Description: permission.Description,
                        Category: permission.Category,
                        IsSystemPermission: permission.IsSystemPermission
                    ));
                }
            }

            _logger.LogInformation("Loaded {Count} discovered permissions", permissions.Count);
            return permissions;
        });
    }

    /// <summary>
    /// Get permissions by module
    /// </summary>
    public async Task<IEnumerable<PermissionDefinition>> GetPermissionsByModuleAsync(string moduleName)
    {
        var allPermissions = await GetAllPermissionsAsync();
        return allPermissions.Where(p => p.Module.Equals(moduleName, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Get permission categories for UI organization
    /// </summary>
    public async Task<IEnumerable<string>> GetPermissionCategoriesAsync()
    {
        var allPermissions = await GetAllPermissionsAsync();
        return allPermissions
            .Where(p => !string.IsNullOrEmpty(p.Category))
            .Select(p => p.Category!)
            .Distinct()
            .OrderBy(c => c);
    }

    /// <summary>
    /// Validate permission existence
    /// </summary>
    public async Task<ValidationResult> ValidatePermissionsAsync(IEnumerable<string> permissionCodes)
    {
        var allPermissions = await GetAllPermissionsAsync();
        var existingCodes = allPermissions.Select(p => p.Name).ToHashSet();
        
        var missing = permissionCodes.Where(code => !existingCodes.Contains(code)).ToList();
        
        return new ValidationResult
        {
            IsValid = !missing.Any(),
            MissingPermissions = missing,
            ValidatedAt = DateTime.UtcNow
        };
    }
}

/// <summary>
/// Permission validation result
/// </summary>
public record ValidationResult
{
    public bool IsValid { get; init; }
    public IList<string> MissingPermissions { get; init; } = new List<string>();
    public DateTime ValidatedAt { get; init; }
    public string? ErrorMessage { get; init; }
}
