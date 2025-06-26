using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using MicFx.Modules.Auth.Authorization;
using MicFx.Modules.Auth.Services;

namespace MicFx.Modules.Auth.Extensions
{
    /// <summary>
    /// Extensions untuk registrasi permission-based authorization system
    /// </summary>
    public static class AuthPermissionExtensions
    {
        /// <summary>
        /// Register permission-based authorization system
        /// </summary>
        /// <param name="services">Service collection</param>
        /// <returns>Service collection untuk method chaining</returns>
        public static IServiceCollection AddPermissionBasedAuthorization(this IServiceCollection services)
        {
            // Register core permission services
            services.AddScoped<IPermissionService, PermissionService>();
            
            // Register authorization handlers
            services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();
            
            // Register custom authorization policy provider
            services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
            
            // Add memory cache if not already registered
            services.AddMemoryCache();
            
            return services;
        }

        /// <summary>
        /// Configure authorization dengan default policies
        /// </summary>
        /// <param name="services">Service collection</param>
        /// <returns>Service collection untuk method chaining</returns>
        public static IServiceCollection ConfigurePermissionAuthorization(this IServiceCollection services)
        {
            services.AddAuthorization(options =>
            {
                // Default policy tetap require authenticated user
                options.DefaultPolicy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .Build();

                // Example static policies (optional)
                options.AddPolicy("RequireAdminRole", policy =>
                    policy.RequireRole("Admin"));

                options.AddPolicy("RequireManagerRole", policy =>
                    policy.RequireRole("Manager"));
            });

            return services;
        }

        /// <summary>
        /// Add full permission-based authorization system
        /// </summary>
        /// <param name="services">Service collection</param>
        /// <returns>Service collection untuk method chaining</returns>
        public static IServiceCollection AddMicFxPermissions(this IServiceCollection services)
        {
            return services
                .AddPermissionBasedAuthorization()
                .ConfigurePermissionAuthorization();
        }
    }
} 