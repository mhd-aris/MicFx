using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using MicFx.SharedKernel.Modularity;
using MicFx.SharedKernel.Common.Exceptions;

namespace MicFx.Core.Modularity
{
    /// <summary>
    /// Simplified module lifecycle manager for MicFx framework
    /// SIMPLIFIED: Removed complex event system and state management for better maintainability
    /// </summary>
    public class ModuleLifecycleManager
    {
        private readonly ILogger<ModuleLifecycleManager> _logger;
        private readonly ModuleDependencyResolver _dependencyResolver;
        private readonly IServiceProvider _serviceProvider;

        // SIMPLIFIED: Simple dictionaries instead of ConcurrentDictionary overhead
        private readonly Dictionary<string, ModuleStateInfo> _moduleStates = new();
        private readonly Dictionary<string, ModuleStartupBase> _moduleInstances = new();
        private readonly object _lock = new object(); // Simple lock for thread safety

        public ModuleLifecycleManager(
            ILogger<ModuleLifecycleManager> logger,
            ModuleDependencyResolver dependencyResolver,
            IServiceProvider serviceProvider)
        {
            _logger = logger;
            _dependencyResolver = dependencyResolver;
            _serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Registers a module to the lifecycle manager
        /// </summary>
        public void RegisterModule(ModuleStartupBase moduleInstance)
        {
            var moduleName = moduleInstance.Manifest.Name;

            lock (_lock)
            {
                _moduleInstances[moduleName] = moduleInstance;
                _moduleStates[moduleName] = new ModuleStateInfo
                {
                    ModuleName = moduleName,
                    State = ModuleState.NotLoaded,
                    RegisteredAt = DateTime.UtcNow,
                    Manifest = moduleInstance.Manifest
                };
            }

            _logger.LogInformation("Module {ModuleName} registered for lifecycle management", moduleName);
        }

        /// <summary>
        /// Starts all modules in correct order based on dependencies
        /// </summary>
        public async Task StartAllModulesAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Starting module lifecycle management for {ModuleCount} modules", _moduleInstances.Count);

            // Validate dependencies first
            var validationResult = _dependencyResolver.ValidateDependencies();
            if (!validationResult.IsValid)
            {
                var errorDetails = string.Join(", ", validationResult.MissingDependencies);
                throw new ModuleException($"Module dependency validation failed: {errorDetails}", "LifecycleManager");
            }

            // Get startup order
            var startupOrder = _dependencyResolver.GetStartupOrder();
            _logger.LogInformation("Module startup order: {StartupOrder}", string.Join(" -> ", startupOrder));

            // Start modules in dependency order
            foreach (var moduleName in startupOrder)
            {
                if (_moduleInstances.ContainsKey(moduleName))
                {
                    await StartModuleAsync(moduleName, cancellationToken);
                }
            }

            _logger.LogInformation("All modules started successfully");
        }

        /// <summary>
        /// Stops all modules in reverse dependency order
        /// </summary>
        public async Task StopAllModulesAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Stopping all modules");

            var shutdownOrder = _dependencyResolver.GetShutdownOrder();
            _logger.LogInformation("Module shutdown order: {ShutdownOrder}", string.Join(" -> ", shutdownOrder));

            foreach (var moduleName in shutdownOrder)
            {
                if (_moduleInstances.ContainsKey(moduleName))
                {
                    await StopModuleAsync(moduleName, cancellationToken);
                }
            }

            _logger.LogInformation("All modules stopped successfully");
        }

        /// <summary>
        /// Starts a specific module with simplified error handling
        /// SIMPLIFIED: Removed complex timeout handling and state transitions
        /// </summary>
        public async Task StartModuleAsync(string moduleName, CancellationToken cancellationToken = default)
        {
            if (!_moduleInstances.ContainsKey(moduleName))
            {
                throw new ModuleException($"Module {moduleName} is not registered", "LifecycleManager");
            }

            var moduleState = _moduleStates[moduleName];
            if (moduleState.State == ModuleState.Loaded)
            {
                _logger.LogWarning("Module {ModuleName} is already loaded", moduleName);
                return;
            }

            var module = _moduleInstances[moduleName];

            try
            {
                // Ensure dependencies are started first
                await EnsureDependenciesStartedAsync(moduleName, cancellationToken);

                // Simple state transition: Loading
                SetModuleState(moduleName, ModuleState.Loading);

                // Execute lifecycle hook if available
                if (module is IModuleLifecycle lifecycleModule)
                {
                    await lifecycleModule.InitializeAsync(cancellationToken);
                }

                // Mark as loaded
                SetModuleState(moduleName, ModuleState.Loaded);

                _logger.LogInformation("Module {ModuleName} started successfully", moduleName);
            }
            catch (Exception ex)
            {
                SetModuleState(moduleName, ModuleState.Error);
                _logger.LogError(ex, "Failed to start module {ModuleName}", moduleName);
                throw new ModuleException($"Failed to start module {moduleName}: {ex.Message}", "LifecycleManager");
            }
        }

        /// <summary>
        /// Stops a specific module with simplified error handling
        /// </summary>
        public async Task StopModuleAsync(string moduleName, CancellationToken cancellationToken = default)
        {
            if (!_moduleInstances.ContainsKey(moduleName))
            {
                _logger.LogWarning("Module {ModuleName} is not registered", moduleName);
                return;
            }

            var moduleState = _moduleStates[moduleName];
            if (moduleState.State == ModuleState.NotLoaded)
            {
                _logger.LogWarning("Module {ModuleName} is already not loaded", moduleName);
                return;
            }

            var module = _moduleInstances[moduleName];

            try
            {
                // Execute lifecycle hook if available
                if (module is IModuleLifecycle lifecycleModule)
                {
                    await lifecycleModule.ShutdownAsync(cancellationToken);
                }

                SetModuleState(moduleName, ModuleState.NotLoaded);
                _logger.LogInformation("Module {ModuleName} stopped successfully", moduleName);
            }
            catch (Exception ex)
            {
                SetModuleState(moduleName, ModuleState.Error);
                _logger.LogError(ex, "Failed to stop module {ModuleName}", moduleName);
                throw new ModuleException($"Failed to stop module {moduleName}: {ex.Message}", "LifecycleManager");
            }
        }

        /// <summary>
        /// Gets all module states (thread-safe)
        /// </summary>
        public Dictionary<string, ModuleStateInfo> GetAllModuleStates()
        {
            lock (_lock)
            {
                return new Dictionary<string, ModuleStateInfo>(_moduleStates);
            }
        }

        /// <summary>
        /// Gets specific module state
        /// </summary>
        public ModuleStateInfo? GetModuleState(string moduleName)
        {
            lock (_lock)
            {
                return _moduleStates.TryGetValue(moduleName, out var state) ? state : null;
            }
        }

        /// <summary>
        /// Simple state transition with thread safety
        /// SIMPLIFIED: Direct state setting without complex event system
        /// </summary>
        private void SetModuleState(string moduleName, ModuleState newState)
        {
            lock (_lock)
            {
                if (_moduleStates.TryGetValue(moduleName, out var stateInfo))
                {
                    stateInfo.State = newState;
                    stateInfo.LastStateChange = DateTime.UtcNow;
                    
                    if (newState == ModuleState.Error)
                    {
                        stateInfo.ErrorCount++;
                    }
                }
            }

            _logger.LogDebug("Module {ModuleName} state changed to {State}", moduleName, newState);
        }

        /// <summary>
        /// Ensure all dependencies are started before starting this module
        /// </summary>
        private async Task EnsureDependenciesStartedAsync(string moduleName, CancellationToken cancellationToken)
        {
            var dependencies = _dependencyResolver.GetDirectDependencies(moduleName);
            
            foreach (var dependency in dependencies)
            {
                var dependencyState = GetModuleState(dependency);
                if (dependencyState?.State != ModuleState.Loaded)
                {
                    await StartModuleAsync(dependency, cancellationToken);
                }
            }
        }
    }

    /// <summary>
    /// Simplified module state information
    /// SIMPLIFIED: Removed complex tracking fields
    /// </summary>
    public class ModuleStateInfo
    {
        public string ModuleName { get; set; } = string.Empty;
        public ModuleState State { get; set; }
        public DateTime RegisteredAt { get; set; }
        public DateTime LastStateChange { get; set; }
        public int ErrorCount { get; set; }
        public IModuleManifest? Manifest { get; set; }
    }
}