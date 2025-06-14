using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using MicFx.Modules.Auth.Domain.Configuration;

namespace MicFx.Modules.Auth.Services
{
    /// <summary>
    /// Service untuk mengkonfigurasi authorization policies
    /// </summary>
    public static class AuthorizationPolicyService
    {
        public const string AdminAreaPolicy = "AdminAreaAccess";
        public const string UserManagementPolicy = "UserManagement";
        public const string SuperAdminPolicy = "SuperAdminOnly";

        /// <summary>
        /// Configure authorization policies berdasarkan AuthConfig
        /// </summary>
        public static void ConfigureAuthorizationPolicies(this IServiceCollection services, AuthConfig config)
        {
            services.AddAuthorization(options =>
            {
                // Policy untuk akses admin area
                options.AddPolicy(AdminAreaPolicy, policy =>
                {
                    policy.RequireAuthenticatedUser();
                    policy.RequireRole(config.AdminRoles.ToArray());
                });

                // Policy untuk user management
                options.AddPolicy(UserManagementPolicy, policy =>
                {
                    policy.RequireAuthenticatedUser();
                    policy.RequireRole(config.UserManagementRoles.ToArray());
                });

                // Policy khusus SuperAdmin
                options.AddPolicy(SuperAdminPolicy, policy =>
                {
                    policy.RequireAuthenticatedUser();
                    policy.RequireRole("SuperAdmin");
                });

                // Policy berdasarkan hierarchy level
                options.AddPolicy("MinimumEditor", policy =>
                {
                    policy.RequireAuthenticatedUser();
                    policy.RequireAssertion(context =>
                    {
                        var userRoles = context.User.Claims
                            .Where(c => c.Type == "role" || c.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role")
                            .Select(c => c.Value);

                        return userRoles.Any(role => 
                            config.RoleHierarchy.ContainsKey(role) && 
                            config.RoleHierarchy[role] >= config.RoleHierarchy["Editor"]);
                    });
                });
            });
        }
    }
} 