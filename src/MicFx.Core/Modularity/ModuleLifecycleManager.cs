using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using MicFx.SharedKernel.Modularity;
using MicFx.SharedKernel.Common.Exceptions;
using System.Collections.Concurrent;

namespace MicFx.Core.Modularity
{
    /// <summary>
    /// Class for managing lifecycle of all modules in the framework
    /// </summary>
    public class ModuleLifecycleManager
    {
        private readonly ILogger<ModuleLifecycleManager> _logger;
        private readonly ModuleDependencyResolver _dependencyResolver;
        private readonly IServiceProvider _serviceProvider;

        private readonly ConcurrentDictionary<string, ModuleStateInfo> _moduleStates = new();
        private readonly ConcurrentDictionary<string, ModuleStartupBase> _moduleInstances = new();
        private readonly ConcurrentDictionary<string, CancellationTokenSource> _moduleOperations = new();

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
        /// Event that is triggered when module state changes
        /// </summary>
        public event EventHandler<ModuleStateChangedEventArgs>? ModuleStateChanged;

        /// <summary>
        /// Registers a module to the lifecycle manager
        /// </summary>
        public void RegisterModule(ModuleStartupBase moduleInstance)
        {
            var moduleName = moduleInstance.Manifest.Name;

            _moduleInstances[moduleName] = moduleInstance;
            _moduleStates[moduleName] = new ModuleStateInfo
            {
                ModuleName = moduleName,
                State = ModuleState.NotLoaded,
                RegisteredAt = DateTime.UtcNow,
                Manifest = moduleInstance.Manifest
            };

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
                var errorDetails = FormatValidationErrors(validationResult);
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
        /// Stops all modules in correct order
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
        /// Starts a specific module
        /// </summary>
        public async Task StartModuleAsync(string moduleName, CancellationToken cancellationToken = default)
        {
            if (!_moduleInstances.ContainsKey(moduleName))
            {
                throw new ModuleException($"Module {moduleName} is not registered", "LifecycleManager");
            }

            var moduleState = _moduleStates[moduleName];
            if (moduleState.State == ModuleState.Started)
            {
                _logger.LogWarning("Module {ModuleName} is already started", moduleName);
                return;
            }

            var module = _moduleInstances[moduleName];
            var operationCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            _moduleOperations[moduleName] = operationCts;

            try
            {
                // Set timeout - use default or get from hot reload manifest
                var timeoutSeconds = 30; // Default timeout
                if (module.Manifest is IHotReloadModuleManifest hotReloadManifest)
                {
                    timeoutSeconds = hotReloadManifest.StartupTimeoutSeconds;
                }
                
                operationCts.CancelAfter(TimeSpan.FromSeconds(timeoutSeconds));

                // Ensure dependencies are started first
                await EnsureDependenciesStartedAsync(moduleName, operationCts.Token);

                // SIMPLIFIED: Only two essential states - Loading and Started
                await TransitionModuleStateAsync(moduleName, ModuleState.Loading, operationCts.Token);

                // Single lifecycle hook - OnStartingAsync
                if (module is IModuleLifecycle lifecycleModule)
                {
                    await lifecycleModule.OnStartingAsync(operationCts.Token);
                }

                // Perform actual module startup
                await PerformModuleStartupAsync(module, operationCts.Token);

                await TransitionModuleStateAsync(moduleName, ModuleState.Started, operationCts.Token);

                // Single post-start hook - OnStartedAsync
                if (module is IModuleLifecycle lifecycleModulePost)
                {
                    await lifecycleModulePost.OnStartedAsync(operationCts.Token);
                }

                _logger.LogInformation("Module {ModuleName} started successfully", moduleName);
            }
            catch (OperationCanceledException) when (operationCts.Token.IsCancellationRequested)
            {
                await TransitionModuleStateAsync(moduleName, ModuleState.Error, CancellationToken.None);
                var timeoutSeconds = module.Manifest is IHotReloadModuleManifest hotReloadManifest ? 
                    hotReloadManifest.StartupTimeoutSeconds : 30;
                _logger.LogError("Module {ModuleName} startup timed out after {TimeoutSeconds} seconds",
                    moduleName, timeoutSeconds);
                throw new ModuleException($"Module {moduleName} startup timed out", "LifecycleManager");
            }
            catch (Exception ex)
            {
                await HandleModuleErrorAsync(moduleName, ex);
                throw;
            }
            finally
            {
                _moduleOperations.TryRemove(moduleName, out _);
                operationCts.Dispose();
            }
        }

        /// <summary>
        /// Stops a specific module
        /// </summary>
        public async Task StopModuleAsync(string moduleName, CancellationToken cancellationToken = default)
        {
            if (!_moduleInstances.ContainsKey(moduleName))
            {
                _logger.LogWarning("Module {ModuleName} is not registered", moduleName);
                return;
            }

            var moduleState = _moduleStates[moduleName];
            if (moduleState.State == ModuleState.Stopped || moduleState.State == ModuleState.NotLoaded)
            {
                _logger.LogWarning("Module {ModuleName} is already stopped", moduleName);
                return;
            }

            var module = _moduleInstances[moduleName];

            try
            {
                await TransitionModuleStateAsync(moduleName, ModuleState.Stopping, cancellationToken);

                if (module is IModuleLifecycle lifecycleModule)
                {
                    await lifecycleModule.OnStoppingAsync(cancellationToken);
                }

                // Stop dependent modules first
                await StopDependentModulesAsync(moduleName, cancellationToken);

                await TransitionModuleStateAsync(moduleName, ModuleState.Stopped, cancellationToken);

                if (module is IModuleLifecycle lifecycleModule2)
                {
                    await lifecycleModule2.OnStoppedAsync(cancellationToken);
                }

                _logger.LogInformation("Module {ModuleName} stopped successfully", moduleName);
            }
            catch (Exception ex)
            {
                await HandleModuleErrorAsync(moduleName, ex);
                throw;
            }
        }

        /// <summary>
        /// Restarts a module (stop then start)
        /// </summary>
        public async Task RestartModuleAsync(string moduleName, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Restarting module {ModuleName}", moduleName);

            await StopModuleAsync(moduleName, cancellationToken);
            await StartModuleAsync(moduleName, cancellationToken);

            _logger.LogInformation("Module {ModuleName} restarted successfully", moduleName);
        }

        /// <summary>
        /// Reloads a module (for hot reload)
        /// </summary>
        public async Task ReloadModuleAsync(string moduleName, CancellationToken cancellationToken = default)
        {
            if (!_moduleInstances.ContainsKey(moduleName))
            {
                throw new ModuleException($"Module {moduleName} is not registered", "LifecycleManager");
            }

            var module = _moduleInstances[moduleName];
            
            // Check if module supports hot reload
            if (module.Manifest is not IHotReloadModuleManifest hotReloadManifest || !hotReloadManifest.SupportsHotReload)
            {
                throw new ModuleException($"Module {moduleName} does not support hot reload", "LifecycleManager");
            }

            if (module is not IModuleHotReload hotReloadModule)
            {
                throw new ModuleException($"Module {moduleName} does not implement IModuleHotReload", "LifecycleManager");
            }

            try
            {
                _logger.LogInformation("Hot reloading module {ModuleName}", moduleName);

                // Check if module can be reloaded
                if (!await hotReloadModule.CanReloadAsync())
                {
                    throw new ModuleException($"Module {moduleName} is not in a safe state for reload", "LifecycleManager");
                }

                await TransitionModuleStateAsync(moduleName, ModuleState.Reloading, cancellationToken);

                await hotReloadModule.OnReloadingAsync(cancellationToken);

                // Get reloadable resources
                var resources = await hotReloadModule.GetReloadResourcesAsync(cancellationToken);
                _logger.LogInformation("Reloading {ResourceCount} resources for module {ModuleName}",
    resources.Count(), moduleName);

                // Perform actual reload logic here
                // This would involve reloading assemblies, configurations, etc.

                await hotReloadModule.OnReloadedAsync(cancellationToken);

                await TransitionModuleStateAsync(moduleName, ModuleState.Started, cancellationToken);

                _logger.LogInformation("Module {ModuleName} hot reloaded successfully", moduleName);
            }
            catch (Exception ex)
            {
                await HandleModuleErrorAsync(moduleName, ex);
                throw;
            }
        }

        /// <summary>
        /// Gets status of all modules
        /// </summary>
        public Dictionary<string, ModuleStateInfo> GetAllModuleStates()
        {
            return _moduleStates.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }

        /// <summary>
        /// Gets status of a specific module
        /// </summary>
        public ModuleStateInfo? GetModuleState(string moduleName)
        {
            return _moduleStates.TryGetValue(moduleName, out var state) ? state : null;
        }

        /// <summary>
        /// Health check for a specific module
        /// </summary>
        public async Task<ModuleHealthDetails> CheckModuleHealthAsync(string moduleName, CancellationToken cancellationToken = default)
        {
            if (!_moduleInstances.ContainsKey(moduleName))
            {
                return new ModuleHealthDetails
                {
                    Status = ModuleHealthStatus.Unhealthy,
                    Description = "Module not found"
                };
            }

            var module = _moduleInstances[moduleName];

            if (module is IModuleHealthCheck healthCheckModule)
            {
                var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                try
                {
                    var details = await healthCheckModule.GetHealthDetailsAsync(cancellationToken);
                    details.Duration = stopwatch.Elapsed;
                    return details;
                }
                catch (Exception ex)
                {
                    return new ModuleHealthDetails
                    {
                        Status = ModuleHealthStatus.Unhealthy,
                        Description = "Health check failed",
                        Exception = ex,
                        Duration = stopwatch.Elapsed
                    };
                }
            }

            // Default health check based on module state
            var moduleState = _moduleStates[moduleName];
            return new ModuleHealthDetails
            {
                Status = moduleState.State == ModuleState.Started ? ModuleHealthStatus.Healthy : ModuleHealthStatus.Unhealthy,
                Description = $"Module state: {moduleState.State}",
                Data = new Dictionary<string, object>
                {
                    { "State", moduleState.State.ToString() },
                    { "LastStateChange", moduleState.LastStateChange },
                    { "ErrorCount", moduleState.ErrorCount }
                }
            };
        }

        private Task TransitionModuleStateAsync(string moduleName, ModuleState newState, CancellationToken cancellationToken)
        {
            if (_moduleStates.TryGetValue(moduleName, out var currentStateInfo))
            {
                var oldState = currentStateInfo.State;
                currentStateInfo.State = newState;
                currentStateInfo.LastStateChange = DateTime.UtcNow;

                if (newState == ModuleState.Error)
                {
                    currentStateInfo.ErrorCount++;
                }

                _logger.LogInformation("Module {ModuleName} state changed from {OldState} to {NewState}",
                    moduleName, oldState, newState);

                // Fire event
                ModuleStateChanged?.Invoke(this, new ModuleStateChangedEventArgs
                {
                    ModuleName = moduleName,
                    OldState = oldState,
                    NewState = newState,
                    Timestamp = currentStateInfo.LastStateChange
                });
            }

            return Task.CompletedTask;
        }

        private async Task HandleModuleErrorAsync(string moduleName, Exception error)
        {
            await TransitionModuleStateAsync(moduleName, ModuleState.Error, CancellationToken.None);

            _logger.LogError(error, "Error in module {ModuleName}", moduleName);

            var module = _moduleInstances[moduleName];
            if (module is IModuleLifecycle lifecycleModule)
            {
                try
                {
                    await lifecycleModule.OnErrorAsync(error, CancellationToken.None);
                }
                catch (Exception lifecycleEx)
                {
                    _logger.LogError(lifecycleEx, "Error in module {ModuleName} error handler", moduleName);
                }
            }
        }

        private async Task EnsureDependenciesStartedAsync(string moduleName, CancellationToken cancellationToken)
        {
            var dependencies = _dependencyResolver.GetAllDependencies(moduleName);

            foreach (var dependency in dependencies)
            {
                if (_moduleStates.TryGetValue(dependency, out var depState) && depState.State != ModuleState.Started)
                {
                    await StartModuleAsync(dependency, cancellationToken);
                }
            }
        }

        private async Task StopDependentModulesAsync(string moduleName, CancellationToken cancellationToken)
        {
            var dependents = _dependencyResolver.GetDependents(moduleName);

            foreach (var dependent in dependents)
            {
                if (_moduleStates.TryGetValue(dependent, out var depState) && depState.State == ModuleState.Started)
                {
                    await StopModuleAsync(dependent, cancellationToken);
                }
            }
        }

        private Task PerformModuleStartupAsync(ModuleStartupBase module, CancellationToken cancellationToken)
        {
            // This is where we would call the actual module startup logic
            // For now, we assume the module startup happens via DI registration
            return Task.CompletedTask;
        }

        private string FormatValidationErrors(ModuleDependencyValidationResult result)
        {
            var errors = new List<string>();

            if (result.MissingDependencies.Any())
            {
                errors.Add($"Missing dependencies: {string.Join(", ", result.MissingDependencies.Select(md => $"{md.ModuleName} -> {md.DependencyName}"))}");
            }

            if (result.CircularDependencies.Any())
            {
                errors.Add($"Circular dependencies: {string.Join(", ", result.CircularDependencies.Select(cd => string.Join(" -> ", cd.Cycle)))}");
            }

            return string.Join("; ", errors);
        }
    }

    /// <summary>
    /// Module state information
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

    /// <summary>
    /// Event args for module state change
    /// </summary>
    public class ModuleStateChangedEventArgs : EventArgs
    {
        public string ModuleName { get; set; } = string.Empty;
        public ModuleState OldState { get; set; }
        public ModuleState NewState { get; set; }
        public DateTime Timestamp { get; set; }
    }
}