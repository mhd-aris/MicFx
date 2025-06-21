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
    /// Simplified extension methods for module dependency and lifecycle management
    /// SIMPLIFIED: Removed over-engineered optimizations for better maintainability
    /// </summary>
    public static class ModuleDependencyExtensions
    {
        /// <summary>
        /// Adds basic MicFx module management services
        /// </summary>
        public static IServiceCollection AddMicFxModuleManagement(this IServiceCollection services)
        {
            services.AddSingleton<ModuleDependencyResolver>();
            services.AddSingleton<ModuleLifecycleManager>();

            return services;
        }

        /// <summary>
        /// Adds MicFx modules with simplified dependency management
        /// SIMPLIFIED: Removed complex optimization and caching for clarity
        /// </summary>
        public static IServiceCollection AddMicFxModules(this IServiceCollection services)
        {
            // Add basic module management services
            services.AddMicFxModuleManagement();

            // Simple module discovery without complex optimizations
            var moduleInstances = DiscoverModules();
            
            Log.Information("Discovered {ModuleCount} modules", moduleInstances.Count);

            // Register module instances directly in DI
            foreach (var moduleInstance in moduleInstances)
            {
                services.AddSingleton(moduleInstance);
            }

            // Configure module services in simple way
            ConfigureModuleServices(services, moduleInstances);

            return services;
        }

        /// <summary>
        /// Use MicFx modules with simplified lifecycle management
        /// SIMPLIFIED: Straightforward module startup without complex optimizations
        /// </summary>
        public static async Task<WebApplication> UseMicFxModulesAsync(this WebApplication app)
        {
            Log.Information("Starting MicFx modules");

            try
            {
                // Configure module endpoints
                ConfigureModuleEndpoints(app);

                // Start modules using lifecycle manager
                var lifecycleManager = app.Services.GetRequiredService<ModuleLifecycleManager>();
                await lifecycleManager.StartAllModulesAsync();

                Log.Information("All modules started successfully");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to start modules");
                throw;
            }

            // Register simple shutdown handler
            RegisterShutdownHandler(app);

            return app;
        }

        /// <summary>
        /// Add simple module health checks
        /// </summary>
        public static IServiceCollection AddMicFxModuleHealthChecks(this IServiceCollection services)
        {
            services.AddHealthChecks()
                .AddCheck<SimpleModuleHealthCheck>("modules");

            return services;
        }

        /// <summary>
        /// Simple module discovery without complex optimizations
        /// SIMPLIFIED: Sequential loading, no caching, clear and debuggable
        /// </summary>
        private static List<ModuleStartupBase> DiscoverModules()
        {
            var moduleInstances = new List<ModuleStartupBase>();

            try
            {
                // Find module assemblies in simple way
                var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
                var moduleFiles = Directory.GetFiles(baseDirectory, "MicFx.Modules.*.dll", SearchOption.AllDirectories);

                // Load assemblies sequentially (simpler, more debuggable)
                foreach (var file in moduleFiles)
                {
                    try
                    {
                        var assembly = System.Reflection.Assembly.LoadFrom(file);
                        
                        // Find module startup classes
                        var moduleTypes = assembly.GetTypes()
                            .Where(t => typeof(ModuleStartupBase).IsAssignableFrom(t) && !t.IsAbstract)
                            .ToList();

                        // Create module instances
                        foreach (var moduleType in moduleTypes)
                        {
                            var moduleInstance = (ModuleStartupBase)Activator.CreateInstance(moduleType)!;
                            
                            // Simple validation
                            if (IsValidModule(moduleInstance))
                            {
                                moduleInstances.Add(moduleInstance);
                                Log.Information("Loaded module: {ModuleName}", moduleInstance.Manifest.Name);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Warning(ex, "Failed to load module from: {FilePath}", file);
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
        /// Simple module validation
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

        /// <summary>
        /// Configure module services in dependency order
        /// SIMPLIFIED: Clear, sequential configuration
        /// </summary>
        private static void ConfigureModuleServices(IServiceCollection services, List<ModuleStartupBase> moduleInstances)
        {
            var dependencyResolver = new ModuleDependencyResolver(
                services.BuildServiceProvider().GetRequiredService<ILogger<ModuleDependencyResolver>>());

            // Register all modules with dependency resolver
            foreach (var moduleInstance in moduleInstances)
            {
                dependencyResolver.RegisterModule(moduleInstance.Manifest);
            }

            // Validate dependencies
            var validationResult = dependencyResolver.ValidateDependencies();
            if (!validationResult.IsValid)
            {
                var errors = string.Join(", ", validationResult.MissingDependencies);
                throw new InvalidOperationException($"Module dependency validation failed: {errors}");
            }

            // Configure services in dependency order
            var startupOrder = dependencyResolver.GetStartupOrder();
            foreach (var moduleName in startupOrder)
            {
                var moduleInstance = moduleInstances.FirstOrDefault(m => m.Manifest.Name == moduleName);
                if (moduleInstance != null)
                {
                    moduleInstance.ConfigureServices(services);
                    Log.Information("Configured services for module: {ModuleName}", moduleName);
                }
            }
        }

        /// <summary>
        /// Configure module endpoints in simple way
        /// </summary>
        private static void ConfigureModuleEndpoints(WebApplication app)
        {
            var moduleInstances = app.Services.GetServices<ModuleStartupBase>();

            foreach (var moduleInstance in moduleInstances)
            {
                try
                {
                    moduleInstance.Configure(app);
                    Log.Information("Configured endpoints for module: {ModuleName}", moduleInstance.Manifest.Name);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Failed to configure endpoints for module: {ModuleName}", moduleInstance.Manifest.Name);
                    throw;
                }
            }
        }

        /// <summary>
        /// Register simple shutdown handler
        /// </summary>
        private static void RegisterShutdownHandler(WebApplication app)
        {
            var applicationLifetime = app.Services.GetRequiredService<IHostApplicationLifetime>();
            
            applicationLifetime.ApplicationStopping.Register(() =>
            {
                Log.Information("Application stopping, shutting down modules...");
                
                var lifecycleManager = app.Services.GetService<ModuleLifecycleManager>();
                if (lifecycleManager != null)
                {
                    try
                    {
                        lifecycleManager.StopAllModulesAsync().GetAwaiter().GetResult();
                        Log.Information("All modules shut down successfully");
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "Error during module shutdown");
                    }
                }
            });
        }
    }

    /// <summary>
    /// Simple health check for modules
    /// SIMPLIFIED: Basic health check without complex diagnostics
    /// </summary>
    public class SimpleModuleHealthCheck : IHealthCheck
    {
        private readonly ModuleLifecycleManager? _lifecycleManager;

        public SimpleModuleHealthCheck(ModuleLifecycleManager? lifecycleManager)
        {
            _lifecycleManager = lifecycleManager;
        }

        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            if (_lifecycleManager == null)
            {
                return Task.FromResult(HealthCheckResult.Unhealthy("Module lifecycle manager not available"));
            }

            try
            {
                var moduleStates = _lifecycleManager.GetAllModuleStates();
                var errorCount = moduleStates.Values.Count(s => s.State == ModuleState.Error);

                if (errorCount > 0)
                {
                    return Task.FromResult(HealthCheckResult.Unhealthy($"{errorCount} modules in error state"));
                }

                return Task.FromResult(HealthCheckResult.Healthy($"{moduleStates.Count} modules running"));
            }
            catch (Exception ex)
            {
                return Task.FromResult(HealthCheckResult.Unhealthy("Error checking module health", ex));
            }
        }
    }
}