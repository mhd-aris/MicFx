using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MicFx.Core.Permissions;

/// <summary>
/// Fluent builder for seeding permissions and role assignments
/// Provides a clean, readable API for permission setup
/// </summary>
public class PermissionSeedBuilder
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<PermissionSeedBuilder> _logger;
    private readonly List<PermissionDiscoveryResult> _permissionModules = new();
    private readonly List<RoleAssignmentBuilder> _roleAssignments = new();

    internal PermissionSeedBuilder(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _logger = serviceProvider.GetRequiredService<ILogger<PermissionSeedBuilder>>();
    }

    /// <summary>
    /// Add permissions from a permission module class
    /// </summary>
    /// <typeparam name="T">Permission module class with [PermissionModule] attribute</typeparam>
    /// <returns>Builder for method chaining</returns>
    public PermissionSeedBuilder FromModule<T>() where T : class
    {
        var discoveryService = _serviceProvider.GetRequiredService<IPermissionDiscoveryService>();
        var assembly = typeof(T).Assembly;
        var results = discoveryService.DiscoverPermissions(assembly);
        
        var moduleResults = results.Where(r => r.Permissions.Any(p => 
            typeof(T).GetFields().Any(f => f.GetRawConstantValue()?.ToString() == p.Name)))
            .ToList();

        _permissionModules.AddRange(moduleResults);
        
        _logger.LogDebug("Added {Count} permission modules from {TypeName}", moduleResults.Count, typeof(T).Name);
        
        return this;
    }

    /// <summary>
    /// Add manual permission definitions
    /// </summary>
    /// <param name="moduleName">Module name</param>
    /// <param name="permissions">List of permission definitions</param>
    /// <returns>Builder for method chaining</returns>
    public PermissionSeedBuilder FromDefinitions(string moduleName, params DiscoveredPermission[] permissions)
    {
        _permissionModules.Add(new PermissionDiscoveryResult
        {
            ModuleName = moduleName,
            Permissions = permissions.ToList()
        });

        _logger.LogDebug("Added {Count} manual permissions for module {ModuleName}", permissions.Length, moduleName);
        
        return this;
    }

    /// <summary>
    /// Assign permissions to a role
    /// </summary>
    /// <param name="roleName">Role name</param>
    /// <param name="configure">Configuration action for role permissions</param>
    /// <returns>Builder for method chaining</returns>
    public PermissionSeedBuilder AssignToRole(string roleName, Action<RoleAssignmentBuilder> configure)
    {
        var roleBuilder = new RoleAssignmentBuilder(roleName, _permissionModules);
        configure(roleBuilder);
        _roleAssignments.Add(roleBuilder);

        _logger.LogDebug("Configured role assignments for {RoleName}", roleName);
        
        return this;
    }

    /// <summary>
    /// Execute the seeding process
    /// </summary>
    /// <returns>Async task</returns>
    public async Task ExecuteAsync()
    {
        _logger.LogInformation("Starting permission seeding process...");

        // TODO: Implement actual seeding logic
        // This will integrate with the existing Auth module infrastructure

        await SeedPermissionsAsync();
        await SeedRoleAssignmentsAsync();

        _logger.LogInformation("Permission seeding completed successfully");
    }

    private async Task SeedPermissionsAsync()
    {
        // Implementation will integrate with existing AuthDbContext
        _logger.LogInformation("Seeding {Count} permission modules", _permissionModules.Count);
        
        foreach (var module in _permissionModules)
        {
            _logger.LogDebug("Seeding {Count} permissions for module {ModuleName}", 
                module.Permissions.Count, module.ModuleName);
        }
        
        await Task.CompletedTask; // Placeholder
    }

    private async Task SeedRoleAssignmentsAsync()
    {
        _logger.LogInformation("Seeding role assignments for {Count} roles", _roleAssignments.Count);
        
        foreach (var roleAssignment in _roleAssignments)
        {
            _logger.LogDebug("Assigning {Count} permissions to role {RoleName}", 
                roleAssignment.Permissions.Count, roleAssignment.RoleName);
        }
        
        await Task.CompletedTask; // Placeholder
    }
}

/// <summary>
/// Builder for configuring role permission assignments
/// </summary>
public class RoleAssignmentBuilder
{
    internal string RoleName { get; }
    internal List<string> Permissions { get; } = new();
    
    private readonly List<PermissionDiscoveryResult> _availableModules;

    internal RoleAssignmentBuilder(string roleName, List<PermissionDiscoveryResult> availableModules)
    {
        RoleName = roleName;
        _availableModules = availableModules;
    }

    /// <summary>
    /// Include a specific permission
    /// </summary>
    /// <param name="permission">Permission constant</param>
    /// <returns>Builder for method chaining</returns>
    public RoleAssignmentBuilder Include(string permission)
    {
        if (!Permissions.Contains(permission))
        {
            Permissions.Add(permission);
        }
        return this;
    }

    /// <summary>
    /// Include multiple specific permissions
    /// </summary>
    /// <param name="permissions">Array of permission constants</param>
    /// <returns>Builder for method chaining</returns>
    public RoleAssignmentBuilder Include(params string[] permissions)
    {
        foreach (var permission in permissions)
        {
            Include(permission);
        }
        return this;
    }

    /// <summary>
    /// Include all permissions from a specific module
    /// </summary>
    /// <param name="moduleName">Module name</param>
    /// <returns>Builder for method chaining</returns>
    public RoleAssignmentBuilder AllFromModule(string moduleName)
    {
        var module = _availableModules.FirstOrDefault(m => m.ModuleName == moduleName);
        if (module != null)
        {
            foreach (var permission in module.Permissions)
            {
                Include(permission.Name);
            }
        }
        return this;
    }

    /// <summary>
    /// Include all permissions from all modules
    /// </summary>
    /// <returns>Builder for method chaining</returns>
    public RoleAssignmentBuilder AllPermissions()
    {
        foreach (var module in _availableModules)
        {
            foreach (var permission in module.Permissions)
            {
                Include(permission.Name);
            }
        }
        return this;
    }

    /// <summary>
    /// Include permissions matching a pattern
    /// </summary>
    /// <param name="pattern">Pattern to match (supports wildcards)</param>
    /// <returns>Builder for method chaining</returns>
    public RoleAssignmentBuilder IncludePattern(string pattern)
    {
        foreach (var module in _availableModules)
        {
            foreach (var permission in module.Permissions)
            {
                if (IsPatternMatch(permission.Name, pattern))
                {
                    Include(permission.Name);
                }
            }
        }
        return this;
    }

    private static bool IsPatternMatch(string permission, string pattern)
    {
        if (pattern == "*") return true;
        if (pattern.EndsWith("*"))
        {
            var prefix = pattern[..^1];
            return permission.StartsWith(prefix);
        }
        return permission == pattern;
    }
}

/// <summary>
/// Extension methods for easy permission seeding
/// </summary>
public static class PermissionSeedExtensions
{
    /// <summary>
    /// Start permission seeding process
    /// </summary>
    /// <param name="serviceProvider">Service provider</param>
    /// <returns>Permission seed builder</returns>
    public static PermissionSeedBuilder SeedPermissions(this IServiceProvider serviceProvider)
    {
        return new PermissionSeedBuilder(serviceProvider);
    }
}
