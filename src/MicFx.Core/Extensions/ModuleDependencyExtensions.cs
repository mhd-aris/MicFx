using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Builder;
using MicFx.Core.Modularity;
using MicFx.SharedKernel.Modularity;
using System.Collections.Concurrent;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Serilog;

namespace MicFx.Core.Extensions
{
    /// <summary>
    /// Extension methods for adding module dependency and lifecycle management
    /// with DI Container Issues fixes for production-ready implementation
    /// </summary>
    public static class ModuleDependencyExtensions
    {
        // Cache for module assemblies that have been loaded for performance
        private static readonly ConcurrentDictionary<string, bool> _loadedAssemblies = new();

        // Cache for module instances to avoid multiple instantiation
        private static readonly ConcurrentDictionary<Type, ModuleStartupBase> _moduleInstanceCache = new();

        /// <summary>
        /// Adds MicFx module dependency and lifecycle management to service collection
        /// </summary>
        public static IServiceCollection AddMicFxModuleManagement(this IServiceCollection services)
        {
            // Register ModuleDependencyResolver as singleton with proper factory
            services.AddSingleton<ModuleDependencyResolver>(serviceProvider =>
            {
                var logger = serviceProvider.GetRequiredService<Microsoft.Extensions.Logging.ILogger<ModuleDependencyResolver>>();
                return new ModuleDependencyResolver(logger);
            });

            // Register ModuleLifecycleManager as singleton with proper factory
            services.AddSingleton<ModuleLifecycleManager>(serviceProvider =>
            {
                var logger = serviceProvider.GetRequiredService<Microsoft.Extensions.Logging.ILogger<ModuleLifecycleManager>>();
                var dependencyResolver = serviceProvider.GetRequiredService<ModuleDependencyResolver>();
                return new ModuleLifecycleManager(logger, dependencyResolver, serviceProvider);
            });

            // Register module registry for storing module information with thread-safe implementation
            services.AddSingleton<IModuleRegistry, ThreadSafeModuleRegistry>();

            return services;
        }

        /// <summary>
        /// Adds MicFx modules with enhanced dependency and lifecycle management
        /// FIXED: Eliminates temporary ServiceProvider to resolve DI Container Issues
        /// </summary>
        public static IServiceCollection AddMicFxModulesWithDependencyManagement(this IServiceCollection services)
        {
            // Add module management services first
            services.AddMicFxModuleManagement();

            // Load module assemblies explicitly with performance optimization
            LoadModuleAssembliesOptimized();

            // Discover and validate module types
            var moduleDiscoveryResult = DiscoverAndValidateModules();
            if (!moduleDiscoveryResult.IsValid)
            {
                throw new InvalidOperationException($"Module discovery failed: {string.Join(", ", moduleDiscoveryResult.Errors)}");
            }

            var moduleInstances = moduleDiscoveryResult.ModuleInstances;
            Log.Information("Discovered {ModuleCount} valid modules", moduleInstances.Count);

            // FIXED: Create dependency resolver and lifecycle manager directly without temporary ServiceProvider
            var dependencyResolver = CreateDependencyResolverDirectly();
            var lifecycleManager = CreateLifecycleManagerPlaceholder();

            // Register module instances to service collection
            services.AddSingleton<IEnumerable<ModuleStartupBase>>(moduleInstances);

            // Register modules to dependency resolver and validate dependencies
            var dependencyValidationResult = RegisterAndValidateModuleDependencies(
                moduleInstances, dependencyResolver, lifecycleManager);

            if (!dependencyValidationResult.IsValid)
            {
                LogValidationErrors(dependencyValidationResult);
                throw new InvalidOperationException("Module dependency validation failed. See logs for details.");
            }

            // Configure module services in correct dependency order
            ConfigureModuleServicesInOrder(services, moduleInstances, dependencyResolver);

            // Register configured dependency resolver and lifecycle manager to service collection
            // for runtime usage (will be replaced by proper instances from DI)
            services.AddSingleton(dependencyResolver);

            return services;
        }

        /// <summary>
        /// Use MicFx modules with enhanced lifecycle management
        /// IMPROVED: Better error handling and performance optimization
        /// </summary>
        public static async Task<WebApplication> UseMicFxModulesWithLifecycleManagementAsync(this WebApplication app)
        {
            var moduleInstances = app.Services.GetRequiredService<IEnumerable<ModuleStartupBase>>();

            Log.Information("🚀 Starting MicFx modules with enhanced lifecycle management");

            try
            {
                // Configure endpoints for modules efficiently
                await ConfigureModuleEndpointsAsync(app, moduleInstances);

                // Start all modules with proper dependency ordering - lifecycle manager will be taken from DI
                var lifecycleManager = app.Services.GetRequiredService<ModuleLifecycleManager>();
                await lifecycleManager.StartAllModulesAsync();

                Log.Information("✅ All modules started successfully");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "❌ Failed to start modules");
                throw;
            }

            // Register enhanced shutdown handler with better error handling
            RegisterEnhancedShutdownHandler(app);

            return app;
        }

        /// <summary>
        /// Extension method to add module health checks with performance optimization
        /// </summary>
        public static IServiceCollection AddMicFxModuleHealthChecks(this IServiceCollection services)
        {
            services.AddHealthChecks()
                .AddCheck<ModuleHealthCheck>("modules");

            return services;
        }


        /// <summary>
        /// Load module assemblies with optimization and caching
        /// </summary>
        private static void LoadModuleAssembliesOptimized()
        {
            var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            var moduleFiles = Directory.GetFiles(baseDirectory, "MicFx.Modules.*.dll", SearchOption.AllDirectories);

            var loadedCount = 0;
            foreach (var file in moduleFiles)
            {
                var fileName = Path.GetFileName(file);
                if (_loadedAssemblies.TryAdd(fileName, true))
                {
                    try
                    {
                        System.Reflection.Assembly.LoadFrom(file);
                        loadedCount++;
                    }
                    catch (Exception ex)
                    {
                        // Log but continue - some assemblies might not be loadable
                        Log.Warning(ex, "Failed to load module assembly: {FileName}", fileName);
                    }
                }
            }

            if (loadedCount > 0)
            {
                Log.Information("Loaded {LoadedCount} module assemblies", loadedCount);
            }
        }

        /// <summary>
        /// Discover and validate modules with comprehensive error handling
        /// </summary>
        private static ModuleDiscoveryResult DiscoverAndValidateModules()
        {
            var result = new ModuleDiscoveryResult();

            try
            {
                // Find all module startup classes
                var moduleTypes = AppDomain.CurrentDomain
                    .GetAssemblies()
                    .SelectMany(x => x.GetTypes())
                    .Where(t => typeof(ModuleStartupBase).IsAssignableFrom(t) && !t.IsAbstract)
                    .ToList();

                // Create and validate module instances
                foreach (var moduleType in moduleTypes)
                {
                    try
                    {
                        var moduleInstance = GetOrCreateModuleInstance(moduleType);

                        // Validate module manifest
                        if (ValidateModuleManifest(moduleInstance.Manifest))
                        {
                            result.ModuleInstances.Add(moduleInstance);
                        }
                        else
                        {
                            result.Errors.Add($"Module {moduleType.Name} has invalid manifest");
                        }
                    }
                    catch (Exception ex)
                    {
                        var error = $"Failed to create/validate instance of module {moduleType.Name}: {ex.Message}";
                        result.Errors.Add(error);
                        Log.Error(ex, "Failed to validate module {ModuleType}", moduleType.Name);
                    }
                }

                result.IsValid = result.Errors.Count == 0 && result.ModuleInstances.Count > 0;

                if (!result.IsValid && result.ModuleInstances.Count == 0)
                {
                    result.Errors.Add("No valid modules found");
                }
            }
            catch (Exception ex)
            {
                result.Errors.Add($"Module discovery failed: {ex.Message}");
                Log.Error(ex, "Module discovery failed");
            }

            return result;
        }

        /// <summary>
        /// Get or create module instance with caching for performance
        /// </summary>
        private static ModuleStartupBase GetOrCreateModuleInstance(Type moduleType)
        {
            return _moduleInstanceCache.GetOrAdd(moduleType, type =>
            {
                var instance = (ModuleStartupBase)Activator.CreateInstance(type)!;
                return instance;
            });
        }

        /// <summary>
        /// Validate module manifest for early error detection
        /// </summary>
        private static bool ValidateModuleManifest(IModuleManifest manifest)
        {
            if (string.IsNullOrWhiteSpace(manifest.Name))
            {
                Log.Error("Module manifest validation failed: Name is required");
                return false;
            }

            if (string.IsNullOrWhiteSpace(manifest.Version))
            {
                Log.Error("Module manifest validation failed: Version is required for {ModuleName}", manifest.Name);
                return false;
            }

            // Validate version format
            if (!Version.TryParse(manifest.Version, out _))
            {
                Log.Error("Module manifest validation failed: Invalid version format '{Version}' for {ModuleName}",
                    manifest.Version, manifest.Name);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Create dependency resolver directly without DI for bootstrap phase
        /// FIXED: Eliminate temporary ServiceProvider dependency
        /// </summary>
        private static ModuleDependencyResolver CreateDependencyResolverDirectly()
        {
            // Use a minimal logger for bootstrap phase to avoid temporary logger creation
            using var loggerFactory = LoggerFactory.Create(builder => builder.SetMinimumLevel(LogLevel.Critical));
            var logger = loggerFactory.CreateLogger<ModuleDependencyResolver>();
            return new ModuleDependencyResolver(logger);
        }

        /// <summary>
        /// Create placeholder lifecycle manager for bootstrap phase
        /// FIXED: Avoid circular dependency with IServiceProvider
        /// </summary>
        private static BootstrapLifecycleManager CreateLifecycleManagerPlaceholder()
        {
            return new BootstrapLifecycleManager();
        }

        /// <summary>
        /// Register and validate module dependencies without temporary ServiceProvider
        /// FIXED: Direct dependency management without DI overhead
        /// </summary>
        private static ModuleDependencyValidationResult RegisterAndValidateModuleDependencies(
            List<ModuleStartupBase> moduleInstances,
            ModuleDependencyResolver dependencyResolver,
            BootstrapLifecycleManager lifecycleManager)
        {
            // Register all modules with dependency resolver
            foreach (var moduleInstance in moduleInstances)
            {
                try
                {
                    dependencyResolver.RegisterModule(moduleInstance.Manifest);
                    lifecycleManager.RegisterModule(moduleInstance);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Failed to register module {ModuleName}", moduleInstance.Manifest.Name);
                    throw;
                }
            }

            // Validate dependencies
            var validationResult = dependencyResolver.ValidateDependencies();

            if (validationResult.IsValid)
            {
                Log.Information("✅ All module dependencies validated successfully");
            }
            else
            {
                Log.Error("❌ Module dependency validation failed");
            }

            return validationResult;
        }

        /// <summary>
        /// Configure module services in correct dependency order
        /// IMPROVED: Better error handling and logging
        /// </summary>
        private static void ConfigureModuleServicesInOrder(
            IServiceCollection services,
            List<ModuleStartupBase> moduleInstances,
            ModuleDependencyResolver dependencyResolver)
        {
            var startupOrder = dependencyResolver.GetStartupOrder();
            Log.Information("Configuring services for {ModuleCount} modules", startupOrder.Count);

            var configuredCount = 0;
            foreach (var moduleName in startupOrder)
            {
                var moduleInstance = moduleInstances.FirstOrDefault(m => m.Manifest.Name == moduleName);
                if (moduleInstance != null)
                {
                    try
                    {
                        moduleInstance.ConfigureServices(services);
                        configuredCount++;
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "Failed to configure services for module {ModuleName}", moduleName);
                        throw new InvalidOperationException($"Failed to configure services for module {moduleName}", ex);
                    }
                }
            }

            Log.Information("✅ Successfully configured services for {ConfiguredCount} modules", configuredCount);
        }

        /// <summary>
        /// Configure module endpoints asynchronously with better performance
        /// </summary>
        private static async Task ConfigureModuleEndpointsAsync(
            WebApplication app,
            IEnumerable<ModuleStartupBase> moduleInstances)
        {
            var modules = moduleInstances.ToList();

            foreach (var moduleInstance in modules)
            {
                try
                {
                    moduleInstance.Configure(app);
                    // Allow other tasks to run
                    await Task.Yield();
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Failed to configure endpoints for module {ModuleName}",
                        moduleInstance.Manifest.Name);
                    throw;
                }
            }

            Log.Information("✅ Successfully configured endpoints for {ModuleCount} modules", modules.Count);
        }

        /// <summary>
        /// Register enhanced shutdown handler with comprehensive error handling
        /// </summary>
        private static void RegisterEnhancedShutdownHandler(WebApplication app)
        {
            app.Lifetime.ApplicationStopping.Register(async () =>
            {
                Log.Information("🛑 Application is stopping, shutting down modules gracefully...");
                var stopwatch = System.Diagnostics.Stopwatch.StartNew();

                try
                {
                    var lifecycleManager = app.Services.GetRequiredService<ModuleLifecycleManager>();
                    await lifecycleManager.StopAllModulesAsync();

                    stopwatch.Stop();
                    Log.Information("✅ All modules stopped successfully in {ElapsedMs}ms", stopwatch.ElapsedMilliseconds);
                }
                catch (Exception ex)
                {
                    stopwatch.Stop();
                    Log.Error(ex, "❌ Error during module shutdown after {ElapsedMs}ms", stopwatch.ElapsedMilliseconds);
                }
            });
        }

        /// <summary>
        /// Log validation errors using Serilog
        /// </summary>
        private static void LogValidationErrors(ModuleDependencyValidationResult validationResult)
        {
            Log.Error("❌ Module dependency validation failed:");

            if (validationResult.MissingDependencies.Any())
            {
                Log.Error("Missing dependencies:");
                foreach (var missing in validationResult.MissingDependencies)
                {
                    Log.Error("  - {ModuleName} requires {DependencyName}", missing.ModuleName, missing.DependencyName);
                }
            }

            if (validationResult.CircularDependencies.Any())
            {
                Log.Error("Circular dependencies detected:");
                foreach (var circular in validationResult.CircularDependencies)
                {
                    Log.Error("  - {Cycle}", string.Join(" -> ", circular.Cycle));
                }
            }
        }


        /// <summary>
        /// Result from module discovery with comprehensive error reporting
        /// </summary>
        private class ModuleDiscoveryResult
        {
            public bool IsValid { get; set; }
            public List<ModuleStartupBase> ModuleInstances { get; set; } = new();
            public List<string> Errors { get; set; } = new();
        }

        /// <summary>
        /// Bootstrap-only lifecycle manager for registration phase
        /// FIXED: Eliminate dependency on IServiceProvider for bootstrap
        /// </summary>
        private class BootstrapLifecycleManager
        {
            private readonly ConcurrentDictionary<string, ModuleStartupBase> _moduleInstances = new();

            public void RegisterModule(ModuleStartupBase moduleInstance)
            {
                var moduleName = moduleInstance.Manifest.Name;
                _moduleInstances[moduleName] = moduleInstance;
            }
        }

    }

    /// <summary>
    /// Thread-safe implementation for module registry
    /// FIXED: Replace Dictionary with ConcurrentDictionary to resolve thread safety issues
    /// </summary>
    public interface IModuleRegistry
    {
        IEnumerable<IModuleManifest> GetAllModules();
        IModuleManifest? GetModule(string name);
        void RegisterModule(IModuleManifest manifest);
        bool IsModuleRegistered(string name);
    }

    /// <summary>
    /// Thread-safe implementation for module registry with ConcurrentDictionary
    /// IMPROVED: Better performance and thread safety
    /// </summary>
    public class ThreadSafeModuleRegistry : IModuleRegistry
    {
        private readonly ConcurrentDictionary<string, IModuleManifest> _modules = new();

        public IEnumerable<IModuleManifest> GetAllModules()
        {
            return _modules.Values.ToList();
        }

        public IModuleManifest? GetModule(string name)
        {
            return _modules.TryGetValue(name, out var module) ? module : null;
        }

        public void RegisterModule(IModuleManifest manifest)
        {
            _modules.AddOrUpdate(manifest.Name, manifest, (key, oldValue) => manifest);
        }

        public bool IsModuleRegistered(string name)
        {
            return _modules.ContainsKey(name);
        }
    }

    /// <summary>
    /// Health check implementation for module status monitoring
    /// </summary>
    public class ModuleHealthCheck : Microsoft.Extensions.Diagnostics.HealthChecks.IHealthCheck
    {
        private readonly ModuleLifecycleManager? _lifecycleManager;

        public ModuleHealthCheck(ModuleLifecycleManager? lifecycleManager)
        {
            _lifecycleManager = lifecycleManager;
        }

        public Task<Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult> CheckHealthAsync(
            Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (_lifecycleManager == null)
                {
                    return Task.FromResult(Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Degraded(
                        "Module lifecycle manager not available"));
                }

                var moduleStates = _lifecycleManager.GetAllModuleStates();
                var totalModules = moduleStates.Count;
                var healthyModules = moduleStates.Values.Count(s => s.State == ModuleState.Started);
                var errorModules = moduleStates.Values.Count(s => s.State == ModuleState.Error);

                var data = new Dictionary<string, object>
                {
                    ["TotalModules"] = totalModules,
                    ["HealthyModules"] = healthyModules,
                    ["ErrorModules"] = errorModules,
                    ["HealthPercentage"] = totalModules > 0 ? (healthyModules * 100.0 / totalModules) : 100.0
                };

                if (errorModules > 0)
                {
                    var errorModuleNames = moduleStates.Values
                        .Where(s => s.State == ModuleState.Error)
                        .Select(s => s.ModuleName)
                        .ToList();

                    data["ErrorModuleNames"] = errorModuleNames;

                    return Task.FromResult(Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Unhealthy(
                        $"{errorModules} modules in error state", data: data));
                }

                if (healthyModules < totalModules)
                {
                    return Task.FromResult(Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Degraded(
                        $"{totalModules - healthyModules} modules not fully started", data: data));
                }

                return Task.FromResult(Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy(
                    $"All {totalModules} modules are healthy", data: data));
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error checking module health");
                return Task.FromResult(Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Unhealthy(
                    "Error checking module health", ex));
            }
        }
    }

}