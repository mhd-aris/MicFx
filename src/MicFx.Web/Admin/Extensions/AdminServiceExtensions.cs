using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Caching.Memory;
using MicFx.SharedKernel.Interfaces;
using MicFx.Web.Admin.Services;

namespace MicFx.Web.Admin.Extensions
{
    /// <summary>
    /// Extension methods for registering admin services
    /// Simple and minimal approach - no over-engineering
    /// </summary>
    public static class AdminServiceExtensions
    {
        /// <summary>
        /// Adds admin navigation services to the service collection with manual registration
        /// </summary>
        /// <param name="services">Service collection</param>
        /// <returns>Service collection for chaining</returns>
        public static IServiceCollection AddAdminNavigation(this IServiceCollection services)
        {
            // Register core services
            services.AddMemoryCache(); // Required for caching
            services.AddScoped<AdminNavDiscoveryService>();
            
            // Register known module contributors - fast and predictable
            services.AddModuleAdminContributors();
            
            return services;
        }
        
        /// <summary>
        /// Registers admin navigation contributors from known modules
        /// Manual registration for better performance and predictability
        /// </summary>
        /// <param name="services">Service collection</param>
        /// <returns>Service collection for chaining</returns>
        public static IServiceCollection AddModuleAdminContributors(this IServiceCollection services)
        {
            // Manual registration of module admin contributors
            // This approach is fast, predictable, and avoids reflection overhead
            
            // HelloWorld Module Contributor
            services.AddTransient<IAdminNavContributor, MicFx.Modules.HelloWorld.Areas.Admin.HelloWorldAdminNavContributor>();
            
            // Auth Module Contributor  
            services.AddTransient<IAdminNavContributor, MicFx.Modules.Auth.Areas.Admin.AuthAdminNavContributor>();
            
            // Future modules should be added here manually for better performance
            // Example: services.AddTransient<IAdminNavContributor, SomeModule.SomeAdminNavContributor>();
            
            return services;
        }
    }
} 