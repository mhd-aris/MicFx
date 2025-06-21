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
        /// Starts a specific module (simplified)
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
            var operationCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            _moduleOperations[moduleName] = operationCts;

            try
            {
                // Simple timeout
                operationCts.CancelAfter(TimeSpan.FromSeconds(30));

                // Ensure dependencies are started first
                await EnsureDependenciesStartedAsync(moduleName, operationCts.Token);

                // SIMPLIFIED: Loading -> Loaded
                await TransitionModuleStateAsync(moduleName, ModuleState.Loading, operationCts.Token);

                // Simplified lifecycle hook - InitializeAsync
                if (module is IModuleLifecycle lifecycleModule)
                {
                    await lifecycleModule.InitializeAsync(operationCts.Token);
                }

                // Perform actual module startup
                await PerformModuleStartupAsync(module, operationCts.Token);

                await TransitionModuleStateAsync(moduleName, ModuleState.Loaded, operationCts.Token);

                _logger.LogInformation("Module {ModuleName} loaded successfully", moduleName);
            }
            catch (OperationCanceledException) when (operationCts.Token.IsCancellationRequested)
            {
                await TransitionModuleStateAsync(moduleName, ModuleState.Error, CancellationToken.None);
                _logger.LogError("Module {ModuleName} startup timed out after 30 seconds", moduleName);
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
        /// Stops a specific module (simplified)
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
                // Simplified lifecycle hook - ShutdownAsync
                if (module is IModuleLifecycle lifecycleModule)
                {
                    await lifecycleModule.ShutdownAsync(cancellationToken);
                }

                // Stop dependent modules first
                await StopDependentModulesAsync(moduleName, cancellationToken);

                await TransitionModuleStateAsync(moduleName, ModuleState.NotLoaded, cancellationToken);

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
                Status = moduleState.State == ModuleState.Loaded ? ModuleHealthStatus.Healthy : ModuleHealthStatus.Unhealthy,
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

            // Simplified error handling - no complex lifecycle hooks
            // Module will need to be restarted manually
        }

        private async Task EnsureDependenciesStartedAsync(string moduleName, CancellationToken cancellationToken)
        {
            var dependencies = _dependencyResolver.GetAllDependencies(moduleName);

            foreach (var dependency in dependencies)
            {
                if (_moduleStates.TryGetValue(dependency, out var depState) && depState.State != ModuleState.Loaded)
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
                if (_moduleStates.TryGetValue(dependent, out var depState) && depState.State == ModuleState.Loaded)
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