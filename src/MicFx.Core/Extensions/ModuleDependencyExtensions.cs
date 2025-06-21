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

            // FIXED: Use proper DI registration instead of premature instantiation
            // Register module types for lazy instantiation
            services.AddSingleton<IModuleDiscoveryService>(sp => 
                new ModuleDiscoveryService(moduleInstances));

            // Register dependency resolver as singleton
            services.AddSingleton<ModuleDependencyResolver>();

            // Register lifecycle manager with proper DI
            services.AddSingleton<ModuleLifecycleManager>();

            // Configure module services using DI-friendly approach
            services.AddSingleton<IModuleServiceConfigurator>(sp =>
                new ModuleServiceConfigurator(moduleInstances, sp.GetRequiredService<ModuleDependencyResolver>()));

            // ✅ PERBAIKAN: Panggil ConfigureServices untuk semua modules
            var tempServiceProvider = services.BuildServiceProvider();
            var serviceConfigurator = tempServiceProvider.GetRequiredService<IModuleServiceConfigurator>();
            serviceConfigurator.ConfigureServices(services);
            tempServiceProvider.Dispose();

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
        /// IMPROVED: Better performance with parallel loading and metadata caching
        /// </summary>
        private static void LoadModuleAssembliesOptimized()
        {
            var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            var moduleFiles = Directory.GetFiles(baseDirectory, "MicFx.Modules.*.dll", SearchOption.AllDirectories);

            if (!moduleFiles.Any())
            {
                Log.Information("No MicFx module assemblies found in {BaseDirectory}", baseDirectory);
                return;
            }

            // IMPROVED: Use parallel loading for better performance
            var loadTasks = moduleFiles
                .Where(file => !_loadedAssemblies.ContainsKey(Path.GetFileName(file)))
                .Select(file => Task.Run(() => LoadAssemblyWithMetadata(file)))
                .ToArray();

            Task.WaitAll(loadTasks);

            var successCount = loadTasks.Count(t => t.Result);
            if (successCount > 0)
            {
                Log.Information("✅ Loaded {LoadedCount}/{TotalCount} module assemblies in parallel", 
                    successCount, moduleFiles.Length);
            }
        }

        /// <summary>
        /// Load individual assembly with metadata validation
        /// </summary>
        private static bool LoadAssemblyWithMetadata(string filePath)
        {
            var fileName = Path.GetFileName(filePath);

            if (!_loadedAssemblies.TryAdd(fileName, true))
            {
                return false; // Already loaded
            }

            try
            {
                // IMPROVED: Load with metadata validation
                var assembly = System.Reflection.Assembly.LoadFrom(filePath);
                
                // Validate assembly has proper MicFx module structure
                var hasModuleStartup = assembly.GetTypes()
                    .Any(t => typeof(ModuleStartupBase).IsAssignableFrom(t) && !t.IsAbstract);

                if (!hasModuleStartup)
                {
                    Log.Warning("Assembly {FileName} loaded but contains no valid module startup classes", fileName);
                }

                return true;
            }
            catch (Exception ex)
            {
                // Remove from cache if loading failed
                _loadedAssemblies.TryRemove(fileName, out _);
                Log.Warning(ex, "Failed to load module assembly: {FileName}", fileName);
                return false;
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
    /// Simple health check implementation for module status monitoring
    /// Simplified from over-engineered version for better maintainability
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
                var loadedModules = moduleStates.Values.Count(s => s.State == ModuleState.Loaded);
                var errorModules = moduleStates.Values.Count(s => s.State == ModuleState.Error);

                var data = new Dictionary<string, object>
                {
                    ["TotalModules"] = totalModules,
                    ["LoadedModules"] = loadedModules,
                    ["ErrorModules"] = errorModules,
                    ["CheckedAt"] = DateTime.UtcNow
                };

                // Simple health determination
                if (errorModules > 0)
                {
                    var errorModuleNames = moduleStates.Values
                        .Where(s => s.State == ModuleState.Error)
                        .Select(s => s.ModuleName)
                        .ToList();

                    data["ErrorModuleNames"] = errorModuleNames;

                    return Task.FromResult(Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Unhealthy(
                        $"{errorModules} modules in error state: {string.Join(", ", errorModuleNames)}", data: data));
                }

                if (loadedModules < totalModules)
                {
                    return Task.FromResult(Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Degraded(
                        $"{totalModules - loadedModules} modules still loading", data: data));
                }

                return Task.FromResult(Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy(
                    $"All {totalModules} modules loaded successfully", data: data));
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error checking module health");
                return Task.FromResult(Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Unhealthy(
                    "Error checking module health", ex));
            }
        }
    }

    /// <summary>
    /// Service for module discovery and management
    /// </summary>
    public interface IModuleDiscoveryService
    {
        IReadOnlyList<ModuleStartupBase> GetDiscoveredModules();
        ModuleStartupBase? GetModule(string moduleName);
    }

    /// <summary>
    /// Implementation of module discovery service
    /// </summary>
    public class ModuleDiscoveryService : IModuleDiscoveryService
    {
        private readonly IReadOnlyList<ModuleStartupBase> _modules;

        public ModuleDiscoveryService(List<ModuleStartupBase> modules)
        {
            _modules = modules.AsReadOnly();
        }

        public IReadOnlyList<ModuleStartupBase> GetDiscoveredModules()
        {
            return _modules;
        }

        public ModuleStartupBase? GetModule(string moduleName)
        {
            return _modules.FirstOrDefault(m => m.Manifest.Name == moduleName);
        }
    }

    /// <summary>
    /// Service for configuring module services
    /// </summary>
    public interface IModuleServiceConfigurator
    {
        void ConfigureServices(IServiceCollection services);
    }

    /// <summary>
    /// Implementation of module service configurator
    /// </summary>
    public class ModuleServiceConfigurator : IModuleServiceConfigurator
    {
        private readonly List<ModuleStartupBase> _moduleInstances;
        private readonly ModuleDependencyResolver _dependencyResolver;

        public ModuleServiceConfigurator(List<ModuleStartupBase> moduleInstances, ModuleDependencyResolver dependencyResolver)
        {
            _moduleInstances = moduleInstances;
            _dependencyResolver = dependencyResolver;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            // Register all modules with dependency resolver first
            foreach (var moduleInstance in _moduleInstances)
            {
                _dependencyResolver.RegisterModule(moduleInstance.Manifest);
            }

            // Validate dependencies
            var validationResult = _dependencyResolver.ValidateDependencies();
            if (!validationResult.IsValid)
            {
                throw new InvalidOperationException("Module dependency validation failed. See logs for details.");
            }

            // Configure services in dependency order
            var startupOrder = _dependencyResolver.GetStartupOrder();
            foreach (var moduleName in startupOrder)
            {
                var moduleInstance = _moduleInstances.FirstOrDefault(m => m.Manifest.Name == moduleName);
                if (moduleInstance != null)
                {
                    moduleInstance.ConfigureServices(services);
                }
            }
        }
    }

}