using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using MicFx.Core.Modularity;
using MicFx.SharedKernel.Modularity;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Serilog;

namespace MicFx.Core.Extensions
{
    /// <summary>
    /// Extension methods for module management
    /// </summary>
    public static class ModuleDependencyExtensions
    {
        /// <summary>
        /// Adds MicFx module management services
        /// </summary>
        public static IServiceCollection AddMicFxModuleManagement(this IServiceCollection services)
        {
            services.AddSingleton<ModuleLoader>();
            services.AddSingleton<ModuleManager>();

            return services;
        }

        /// <summary>
        /// Adds health checks for MicFx modules
        /// </summary>
        public static IServiceCollection AddMicFxModuleHealthChecks(this IServiceCollection services)
        {
            services.AddHealthChecks()
                .AddCheck<ModuleHealthCheck>("micfx_modules");

            return services;
        }

        /// <summary>
        /// Adds MicFx modules with priority-based loading
        /// </summary>
        public static IServiceCollection AddMicFxModules(this IServiceCollection services)
        {
            // Add module management services
            services.AddMicFxModuleManagement();

            // Discover modules via assembly scanning
            var moduleInstances = DiscoverModules();
            
            Log.Information("Discovered {ModuleCount} modules", moduleInstances.Count);

            // Register module instances
            foreach (var moduleInstance in moduleInstances)
            {
                services.AddSingleton(moduleInstance);
            }

            // Configure module services
            ConfigureModuleServices(services, moduleInstances);

            return services;
        }

        /// <summary>
        /// Configure MicFx modules with the application pipeline
        /// </summary>
        public static async Task<IApplicationBuilder> UseMicFxModulesAsync(this IApplicationBuilder app)
        {
            var moduleManager = app.ApplicationServices.GetRequiredService<ModuleManager>();
            
            // Get all registered module instances and register them with ModuleManager
            var moduleInstances = app.ApplicationServices.GetServices<ModuleStartupBase>().ToList();
            
            foreach (var moduleInstance in moduleInstances)
            {
                moduleManager.RegisterModule(moduleInstance);
            }
            
            // Start all modules
            await moduleManager.StartAllModulesAsync();

            Log.Information("MicFx modules configured successfully. {ModuleCount} modules active", moduleManager.ModuleCount);

            return app;
        }

        /// <summary>
        /// Discover modules using assembly scanning
        /// </summary>
        private static List<ModuleStartupBase> DiscoverModules()
        {
            var moduleInstances = new List<ModuleStartupBase>();

            try
            {
                // Scan assemblies with MicFx.Modules prefix
                var moduleTypes = AppDomain.CurrentDomain.GetAssemblies()
                    .Where(a => a.GetName().Name?.StartsWith("MicFx.Modules.") == true)
                    .SelectMany(a => a.GetTypes())
                    .Where(t => typeof(ModuleStartupBase).IsAssignableFrom(t) && !t.IsAbstract)
                    .ToList();

                Log.Information("Found {ModuleTypeCount} module types", moduleTypes.Count);

                // Create instances
                foreach (var moduleType in moduleTypes)
                {
                    try
                    {
                        var moduleInstance = (ModuleStartupBase)Activator.CreateInstance(moduleType)!;
                        
                        if (IsValidModule(moduleInstance))
                        {
                            moduleInstances.Add(moduleInstance);
                            Log.Information("Loaded module: {ModuleName} v{Version}", 
                                moduleInstance.Manifest.Name, moduleInstance.Manifest.Version);
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Warning(ex, "Failed to create instance of module type: {ModuleType}", moduleType.Name);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Module discovery failed");
                throw;
            }

            return moduleInstances;
        }

        /// <summary>
        /// Configure module services in priority order
        /// </summary>
        private static void ConfigureModuleServices(IServiceCollection services, List<ModuleStartupBase> moduleInstances)
        {
            // Create logger
            using var loggerFactory = Microsoft.Extensions.Logging.LoggerFactory.Create(builder =>
            {
                builder.AddConsole();
            });
            var logger = loggerFactory.CreateLogger<ModuleLoader>();

            var moduleLoader = new ModuleLoader(logger);

            // Register all modules
            foreach (var moduleInstance in moduleInstances)
            {
                moduleLoader.RegisterModule(moduleInstance.Manifest);
                Log.Information("Registered module '{ModuleName}' with priority {Priority}", 
                    moduleInstance.Manifest.Name, moduleInstance.Manifest.Priority);
            }

            // Validate registration
            var validationResult = moduleLoader.ValidateRegistration();
            Log.Information("Module registration validation completed. Valid: {IsValid}", validationResult);

            // Configure services in priority order
            var startupOrder = moduleLoader.GetStartupOrder();
            Log.Information("Startup order for {ModuleCount} modules: {StartupOrder}", 
                moduleInstances.Count, string.Join(" → ", startupOrder.Select(m => m.Name)));

            foreach (var manifest in startupOrder)
            {
                var moduleInstance = moduleInstances.FirstOrDefault(m => m.Manifest.Name == manifest.Name);
                if (moduleInstance != null)
                {
                    moduleInstance.ConfigureServices(services);
                    Log.Information("Configured services for module: {ModuleName}", manifest.Name);
                }
            }
        }

        /// <summary>
        /// Validate module instance
        /// </summary>
        private static bool IsValidModule(ModuleStartupBase moduleInstance)
        {
            if (string.IsNullOrWhiteSpace(moduleInstance.Manifest.Name))
            {
                Log.Warning("Module has no name, skipping");
                return false;
            }

            if (string.IsNullOrWhiteSpace(moduleInstance.Manifest.Version))
            {
                Log.Warning("Module {ModuleName} has no version, skipping", moduleInstance.Manifest.Name);
                return false;
            }

            return true;
        }
    }

    /// <summary>
    /// Health check for modules
    /// </summary>
    public class ModuleHealthCheck : IHealthCheck
    {
        private readonly ModuleManager? _moduleManager;

        public ModuleHealthCheck(ModuleManager? moduleManager)
        {
            _moduleManager = moduleManager;
        }

        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            if (_moduleManager == null)
            {
                return Task.FromResult(HealthCheckResult.Unhealthy("Module manager not available"));
            }

            // Verify module count
            var moduleCount = _moduleManager.ModuleCount;
            var data = new Dictionary<string, object>
            {
                ["TotalModules"] = moduleCount,
                ["Status"] = "All modules registered"
            };

            return Task.FromResult(HealthCheckResult.Healthy($"{moduleCount} modules registered", data));
        }
    }
}
