using Microsoft.Extensions.Logging;
using MicFx.SharedKernel.Modularity;

namespace MicFx.Core.Modularity
{
    /// <summary>
    /// Module manager for module lifecycle management
    /// </summary>
    public class ModuleManager
    {
        private readonly ILogger<ModuleManager> _logger;
        private readonly ModuleLoader _moduleLoader;
        private readonly List<ModuleStartupBase> _moduleInstances = new();

        public ModuleManager(ILogger<ModuleManager> logger, ModuleLoader moduleLoader)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _moduleLoader = moduleLoader ?? throw new ArgumentNullException(nameof(moduleLoader));
        }

        /// <summary>
        /// Register a module instance
        /// </summary>
        public void RegisterModule(ModuleStartupBase moduleInstance)
        {
            if (moduleInstance == null)
                throw new ArgumentNullException(nameof(moduleInstance));

            _moduleInstances.Add(moduleInstance);
            _moduleLoader.RegisterModule(moduleInstance.Manifest);

            _logger.LogInformation("Registered module '{ModuleName}' instance", moduleInstance.Manifest.Name);
        }

        /// <summary>
        /// Start all modules in priority order
        /// </summary>
        public async Task StartAllModulesAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Starting {ModuleCount} modules", _moduleInstances.Count);

            // Validate registration
            _moduleLoader.ValidateRegistration();

            // Get startup order
            var startupOrder = _moduleLoader.GetStartupOrder();

            // Start modules in order
            foreach (var manifest in startupOrder)
            {
                var moduleInstance = _moduleInstances.FirstOrDefault(m => m.Manifest.Name == manifest.Name);
                if (moduleInstance != null)
                {
                    try
                    {
                        _logger.LogInformation("Starting module '{ModuleName}'", manifest.Name);
                        await moduleInstance.InitializeAsync(cancellationToken);
                        _logger.LogInformation("Module '{ModuleName}' started successfully", manifest.Name);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to start module '{ModuleName}'", manifest.Name);
                        
                        // Critical modules fail fast, others continue
                        if (manifest.IsCritical)
                        {
                            throw new InvalidOperationException($"Critical module '{manifest.Name}' failed to start", ex);
                        }
                    }
                }
            }

            _logger.LogInformation("All modules started successfully");
        }

        /// <summary>
        /// Get count of registered modules
        /// </summary>
        public int ModuleCount => _moduleInstances.Count;
    }
}
