using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MicFx.Mvc.Web.Admin.Services;
using MicFx.SharedKernel.Interfaces;

namespace MicFx.Mvc.Web.Admin.Extensions
{
    /// <summary>
    /// Extension methods for registering admin services
    /// </summary>
    public static class AdminServiceExtensions
    {
        /// <summary>
        /// Adds admin navigation services to the service collection with auto-discovery
        /// </summary>
        /// <param name="services">Service collection</param>
        /// <param name="enableAutoDiscovery">Enable automatic discovery of navigation contributors</param>
        /// <returns>Service collection for chaining</returns>
        public static IServiceCollection AddAdminNavigation(this IServiceCollection services, bool enableAutoDiscovery = true)
        {
            // Register core services
            services.AddMemoryCache(); // Required for caching
            services.AddScoped<AdminNavDiscoveryService>();
            services.AddSingleton<AdminModuleScanner>();
            
            // Auto-discovery of navigation contributors
            if (enableAutoDiscovery)
            {
                var serviceProvider = services.BuildServiceProvider();
                var scanner = serviceProvider.GetRequiredService<AdminModuleScanner>();
                var contributorsFound = scanner.ScanAndRegisterContributors(services);
                
                // Log the discovery results
                var loggerFactory = serviceProvider.GetService<ILoggerFactory>();
                var logger = loggerFactory?.CreateLogger("AdminServiceExtensions");
                logger?.LogInformation("🎯 Auto-discovery enabled: {ContributorsFound} navigation contributors registered", contributorsFound);
            }
            
            return services;
        }
        
        /// <summary>
        /// Adds an admin navigation contributor to the service collection
        /// </summary>
        /// <typeparam name="T">Type of the navigation contributor</typeparam>
        /// <param name="services">Service collection</param>
        /// <returns>Service collection for chaining</returns>
        public static IServiceCollection AddAdminNavContributor<T>(this IServiceCollection services)
            where T : class, IAdminNavContributor
        {
            services.AddTransient<IAdminNavContributor, T>();
            return services;
        }
        
        /// <summary>
        /// Adds multiple admin navigation contributors to the service collection
        /// </summary>
        /// <param name="services">Service collection</param>
        /// <param name="contributorTypes">Types of navigation contributors</param>
        /// <returns>Service collection for chaining</returns>
        public static IServiceCollection AddAdminNavContributors(this IServiceCollection services, params Type[] contributorTypes)
        {
            foreach (var contributorType in contributorTypes)
            {
                if (typeof(IAdminNavContributor).IsAssignableFrom(contributorType))
                {
                    services.AddTransient(typeof(IAdminNavContributor), contributorType);
                }
            }
            
            return services;
        }
        
        /// <summary>
        /// Automatically discovers and registers admin navigation contributors from assemblies
        /// </summary>
        /// <param name="services">Service collection</param>
        /// <param name="assemblies">Assemblies to scan for contributors</param>
        /// <returns>Service collection for chaining</returns>
        public static IServiceCollection AddAdminNavContributorsFromAssemblies(this IServiceCollection services, params System.Reflection.Assembly[] assemblies)
        {
            foreach (var assembly in assemblies)
            {
                var contributorTypes = assembly.GetTypes()
                    .Where(t => t.IsClass && !t.IsAbstract && typeof(IAdminNavContributor).IsAssignableFrom(t));
                
                foreach (var contributorType in contributorTypes)
                {
                    services.AddTransient(typeof(IAdminNavContributor), contributorType);
                }
            }
            
            return services;
        }
    }
} 