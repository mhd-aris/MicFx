using System;

namespace MicFx.Core.Permissions;

/// <summary>
/// Attribute to define permission metadata for constants
/// Used by auto-discovery system to register permissions
/// </summary>
[AttributeUsage(AttributeTargets.Field)]
public class PermissionAttribute : Attribute
{
    /// <summary>
    /// Human-readable display name for the permission
    /// </summary>
    public string DisplayName { get; }

    /// <summary>
    /// Detailed description of what this permission allows
    /// </summary>
    public string Description { get; }

    /// <summary>
    /// Category within the module (e.g., "Users", "Roles", "Settings")
    /// </summary>
    public string? Category { get; set; }

    /// <summary>
    /// Whether this is a system permission (cannot be deleted)
    /// </summary>
    public bool IsSystemPermission { get; set; } = false;

    /// <summary>
    /// Constructor for permission metadata
    /// </summary>
    /// <param name="displayName">Human-readable name</param>
    /// <param name="description">What this permission allows</param>
    public PermissionAttribute(string displayName, string description)
    {
        DisplayName = displayName ?? throw new ArgumentNullException(nameof(displayName));
        Description = description ?? throw new ArgumentNullException(nameof(description));
    }
}

/// <summary>
/// Attribute to mark a class as containing permissions for a specific module
/// Used by auto-discovery system to identify permission modules
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class PermissionModuleAttribute : Attribute
{
    /// <summary>
    /// Module name (e.g., "auth", "cms", "ecommerce")
    /// </summary>
    public string ModuleName { get; }

    /// <summary>
    /// Constructor for permission module
    /// </summary>
    /// <param name="moduleName">Module name</param>
    public PermissionModuleAttribute(string moduleName)
    {
        ModuleName = moduleName?.ToLowerInvariant() ?? throw new ArgumentNullException(nameof(moduleName));
    }
}
