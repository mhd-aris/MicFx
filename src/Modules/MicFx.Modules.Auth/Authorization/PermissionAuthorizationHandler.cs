using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using MicFx.Modules.Auth.Services;

namespace MicFx.Modules.Auth.Authorization
{
    /// <summary>
    /// Authorization handler untuk permission-based access control
    /// Integrates dengan IPermissionService untuk wildcard support
    /// </summary>
    public class PermissionAuthorizationHandler : IAuthorizationHandler
    {
        private readonly IPermissionService _permissionService;
        private readonly ILogger<PermissionAuthorizationHandler> _logger;

        public PermissionAuthorizationHandler(
            IPermissionService permissionService,
            ILogger<PermissionAuthorizationHandler> logger)
        {
            _permissionService = permissionService;
            _logger = logger;
        }

        public async Task HandleAsync(AuthorizationHandlerContext context)
        {
            // Handle semua permission requirements
            var permissionRequirements = context.Requirements.OfType<PermissionRequirement>().ToList();
            var anyPermissionRequirements = context.Requirements.OfType<AnyPermissionRequirement>().ToList();

            // Process single permission requirements
            foreach (var requirement in permissionRequirements)
            {
                await HandlePermissionRequirementAsync(context, requirement);
            }

            // Process any permission requirements
            foreach (var requirement in anyPermissionRequirements)
            {
                await HandleAnyPermissionRequirementAsync(context, requirement);
            }
        }

        private async Task HandlePermissionRequirementAsync(
            AuthorizationHandlerContext context,
            PermissionRequirement requirement)
        {
            if (context.User.Identity?.IsAuthenticated != true)
            {
                _logger.LogDebug("User not authenticated for permission {Permission}", requirement.Permission);
                return;
            }

            try
            {
                var hasPermission = await _permissionService.HasPermissionAsync(context.User, requirement.Permission);

                if (hasPermission)
                {
                    context.Succeed(requirement);
                    _logger.LogDebug("Permission {Permission} granted for user {UserId}",
                        requirement.Permission, GetUserId(context.User));
                }
                else
                {
                    _logger.LogWarning("Permission {Permission} denied for user {UserId}",
                        requirement.Permission, GetUserId(context.User));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking permission {Permission} for user {UserId}",
                    requirement.Permission, GetUserId(context.User));
            }
        }

        private async Task HandleAnyPermissionRequirementAsync(
            AuthorizationHandlerContext context,
            AnyPermissionRequirement requirement)
        {
            if (context.User.Identity?.IsAuthenticated != true)
            {
                _logger.LogDebug("User not authenticated for any permission check: {Permissions}", 
                    string.Join(", ", requirement.Permissions));
                return;
            }

            try
            {
                // Check if user has any of the required permissions
                foreach (var permission in requirement.Permissions)
                {
                    var hasPermission = await _permissionService.HasPermissionAsync(context.User, permission);
                    
                    if (hasPermission)
                    {
                        context.Succeed(requirement);
                        _logger.LogDebug("Any permission requirement satisfied: {Permission} granted for user {UserId}",
                            permission, GetUserId(context.User));
                        return;
                    }
                }

                _logger.LogWarning("Any permission requirement failed: none of {Permissions} granted for user {UserId}",
                    string.Join(", ", requirement.Permissions), GetUserId(context.User));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking any permission requirement {Permissions} for user {UserId}",
                    string.Join(", ", requirement.Permissions), GetUserId(context.User));
            }
        }

        private static string? GetUserId(System.Security.Claims.ClaimsPrincipal user)
        {
            return user.FindFirst("user_id")?.Value ?? 
                   user.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        }
    }

    /// <summary>
    /// Requirement untuk single permission
    /// </summary>
    public class PermissionRequirement : IAuthorizationRequirement
    {
        /// <summary>
        /// Required permission name
        /// </summary>
        public string Permission { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        public PermissionRequirement(string permission)
        {
            Permission = permission ?? throw new ArgumentNullException(nameof(permission));
        }
    }

    /// <summary>
    /// Requirement untuk multiple permissions (ANY logic)
    /// </summary>
    public class AnyPermissionRequirement : IAuthorizationRequirement
    {
        /// <summary>
        /// Array of permission names (user needs any one)
        /// </summary>
        public string[] Permissions { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        public AnyPermissionRequirement(params string[] permissions)
        {
            if (permissions == null || permissions.Length == 0)
            {
                throw new ArgumentException("At least one permission is required", nameof(permissions));
            }

            Permissions = permissions;
        }
    }

    /// <summary>
    /// Authorization policy provider untuk dynamic policy creation
    /// </summary>
    public class PermissionPolicyProvider : DefaultAuthorizationPolicyProvider
    {
        public PermissionPolicyProvider(Microsoft.Extensions.Options.IOptions<AuthorizationOptions> options) 
            : base(options) 
        {
        }

        public override async Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
        {
            // Check for existing policies first
            var policy = await base.GetPolicyAsync(policyName);
            if (policy != null)
            {
                return policy;
            }

            // Handle dynamic permission policies
            if (policyName.StartsWith("Permission:"))
            {
                var permission = policyName.Substring("Permission:".Length);
                return new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .AddRequirements(new PermissionRequirement(permission))
                    .Build();
            }

            // Handle dynamic any permission policies
            if (policyName.StartsWith("AnyPermission:"))
            {
                var permissionsString = policyName.Substring("AnyPermission:".Length);
                var permissions = permissionsString.Split(',', StringSplitOptions.RemoveEmptyEntries);
                
                return new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .AddRequirements(new AnyPermissionRequirement(permissions))
                    .Build();
            }

            return null;
        }
    }
} 