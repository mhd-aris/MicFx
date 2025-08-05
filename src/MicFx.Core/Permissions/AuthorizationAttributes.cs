using Microsoft.AspNetCore.Authorization;
using System;

namespace MicFx.Core.Permissions;

/// <summary>
/// Clear, explicit permission authorization attribute
/// Replaces the old Permission attribute with better DX
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public class RequirePermissionAttribute : AuthorizeAttribute
{
    /// <summary>
    /// Required permission name
    /// </summary>
    public string Permission { get; }

    /// <summary>
    /// Constructor for single permission requirement
    /// </summary>
    /// <param name="permission">Permission constant (e.g., AuthPermissions.VIEW_USERS)</param>
    public RequirePermissionAttribute(string permission) : base()
    {
        Permission = permission ?? throw new ArgumentNullException(nameof(permission));
        Policy = $"Permission:{permission}";
    }
}

/// <summary>
/// Require any of the specified permissions (OR logic)
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public class RequireAnyPermissionAttribute : AuthorizeAttribute
{
    /// <summary>
    /// Array of permissions (user needs any one)
    /// </summary>
    public string[] Permissions { get; }

    /// <summary>
    /// Constructor for multiple permissions (ANY logic)
    /// </summary>
    /// <param name="permissions">Array of permission constants</param>
    public RequireAnyPermissionAttribute(params string[] permissions) : base()
    {
        if (permissions == null || permissions.Length == 0)
        {
            throw new ArgumentException("At least one permission is required", nameof(permissions));
        }

        Permissions = permissions;
        Policy = $"AnyPermission:{string.Join(",", permissions)}";
    }
}

/// <summary>
/// Require all of the specified permissions (AND logic)
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public class RequireAllPermissionsAttribute : AuthorizeAttribute
{
    /// <summary>
    /// Array of permissions (user needs all)
    /// </summary>
    public string[] Permissions { get; }

    /// <summary>
    /// Constructor for multiple permissions (ALL logic)
    /// </summary>
    /// <param name="permissions">Array of permission constants</param>
    public RequireAllPermissionsAttribute(params string[] permissions) : base()
    {
        if (permissions == null || permissions.Length == 0)
        {
            throw new ArgumentException("At least one permission is required", nameof(permissions));
        }

        Permissions = permissions;
        Policy = $"AllPermissions:{string.Join(",", permissions)}";
    }
}

/// <summary>
/// Simple role-based authorization (for basic scenarios)
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public class RequireRoleAttribute : AuthorizeAttribute
{
    /// <summary>
    /// Required roles
    /// </summary>
    public string[] RequiredRoles { get; }

    /// <summary>
    /// Constructor for role requirement
    /// </summary>
    /// <param name="roles">Array of role names</param>
    public RequireRoleAttribute(params string[] roles) : base()
    {
        if (roles == null || roles.Length == 0)
        {
            throw new ArgumentException("At least one role is required", nameof(roles));
        }

        RequiredRoles = roles;
        Roles = string.Join(",", roles);
    }
}
