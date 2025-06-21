using Microsoft.Extensions.Logging;
using MicFx.SharedKernel.Modularity;
using MicFx.SharedKernel.Common.Exceptions;
using System.Linq;

namespace MicFx.Core.Modularity
{
    /// <summary>
    /// Simple module dependency resolver focused on current framework needs
    /// Provides priority-based ordering and basic dependency validation
    /// </summary>
    public class ModuleDependencyResolver
    {
        private readonly ILogger<ModuleDependencyResolver> _logger;
        private readonly Dictionary<string, IModuleManifest> _registeredModules = new();

        public ModuleDependencyResolver(ILogger<ModuleDependencyResolver> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Registers a module for dependency resolution
        /// </summary>
        /// <param name="manifest">Module manifest to register</param>
        /// <exception cref="ArgumentNullException">When manifest is null</exception>
        /// <exception cref="ModuleException">When module name is invalid</exception>
        public void RegisterModule(IModuleManifest manifest)
        {
            if (manifest == null)
                throw new ArgumentNullException(nameof(manifest));

            if (string.IsNullOrWhiteSpace(manifest.Name))
                throw new ModuleException("Module name cannot be null or empty", "DependencyResolver");

            var moduleName = manifest.Name.Trim();

            if (_registeredModules.ContainsKey(moduleName))
            {
                _logger.LogWarning("Module '{ModuleName}' is already registered. Replacing existing registration", moduleName);
            }

            _registeredModules[moduleName] = manifest;

            _logger.LogInformation("Registered module '{ModuleName}' with priority {Priority}", moduleName, manifest.Priority);
        }

        /// <summary>
        /// Validates that all module dependencies are satisfied
        /// </summary>
        /// <returns>Validation result with any missing dependencies</returns>
        public DependencyValidationResult ValidateDependencies()
        {
            var missingDependencies = new List<string>();

            foreach (var module in _registeredModules.Values)
            {
                foreach (var dependency in module.Dependencies ?? Array.Empty<string>())
                {
                    if (string.IsNullOrWhiteSpace(dependency))
                        continue;

                    var dependencyName = dependency.Trim();
                    if (!_registeredModules.ContainsKey(dependencyName))
                    {
                        missingDependencies.Add($"Module '{module.Name}' requires '{dependencyName}' but it is not registered");
                    }
                }
            }

            var isValid = !missingDependencies.Any();
            _logger.LogInformation("Dependency validation completed. Valid: {IsValid}, Missing: {MissingCount}", 
                isValid, missingDependencies.Count);

            if (!isValid)
            {
                _logger.LogWarning("Missing dependencies: {MissingDependencies}", string.Join(", ", missingDependencies));
            }

            return new DependencyValidationResult(isValid, missingDependencies.AsReadOnly());
        }

        /// <summary>
        /// Gets module startup order based on Priority (lower number = higher priority, loads first)
        /// </summary>
        /// <returns>Ordered list of module names for startup</returns>
        public IReadOnlyList<string> GetStartupOrder()
        {
            if (!_registeredModules.Any())
            {
                _logger.LogInformation("No modules registered for startup ordering");
                return Array.Empty<string>();
            }

            var orderedModules = _registeredModules.Values
                .OrderBy(m => m.Priority)                // Lower number = higher priority
                .ThenBy(m => m.Name, StringComparer.OrdinalIgnoreCase)  // Alphabetical for deterministic ordering
                .Select(m => m.Name)
                .ToList()
                .AsReadOnly();

            _logger.LogInformation("Startup order for {ModuleCount} modules: {StartupOrder}",
                orderedModules.Count, string.Join(" → ", orderedModules));

            return orderedModules;
        }

        /// <summary>
        /// Gets module shutdown order (reverse of startup order)
        /// </summary>
        /// <returns>Ordered list of module names for shutdown</returns>
        public IReadOnlyList<string> GetShutdownOrder()
        {
            var startupOrder = GetStartupOrder().ToList();
            startupOrder.Reverse();

            var shutdownOrder = startupOrder.AsReadOnly();
            
            _logger.LogInformation("Shutdown order for {ModuleCount} modules: {ShutdownOrder}",
                shutdownOrder.Count, string.Join(" → ", shutdownOrder));

            return shutdownOrder;
        }

        /// <summary>
        /// Gets direct dependencies for a specific module
        /// </summary>
        /// <param name="moduleName">Name of the module</param>
        /// <returns>List of direct dependency names</returns>
        public IReadOnlyList<string> GetDirectDependencies(string moduleName)
        {
            if (string.IsNullOrWhiteSpace(moduleName))
                return Array.Empty<string>();

            var trimmedName = moduleName.Trim();
            if (!_registeredModules.TryGetValue(trimmedName, out var module))
            {
                _logger.LogDebug("Module '{ModuleName}' not found when getting dependencies", trimmedName);
                return Array.Empty<string>();
            }

            var dependencies = (module.Dependencies ?? Array.Empty<string>())
                .Where(d => !string.IsNullOrWhiteSpace(d))
                .Select(d => d.Trim())
                .ToList()
                .AsReadOnly();

            return dependencies;
        }

        /// <summary>
        /// Gets modules that directly depend on the specified module
        /// </summary>
        /// <param name="moduleName">Name of the module</param>
        /// <returns>List of dependent module names</returns>
        public IReadOnlyList<string> GetDirectDependents(string moduleName)
        {
            if (string.IsNullOrWhiteSpace(moduleName))
                return Array.Empty<string>();

            var trimmedName = moduleName.Trim();
            var dependents = _registeredModules.Values
                .Where(m => (m.Dependencies ?? Array.Empty<string>())
                    .Any(d => string.Equals(d?.Trim(), trimmedName, StringComparison.OrdinalIgnoreCase)))
                .Select(m => m.Name)
                .OrderBy(name => name, StringComparer.OrdinalIgnoreCase)
                .ToList()
                .AsReadOnly();

            return dependents;
        }

        /// <summary>
        /// Gets total count of registered modules
        /// </summary>
        public int RegisteredModuleCount => _registeredModules.Count;

        // GetModulePriority method removed - Priority is now directly available from interface
    }

    /// <summary>
    /// Result of dependency validation with immutable properties
    /// </summary>
    public class DependencyValidationResult
    {
        public bool IsValid { get; }
        public IReadOnlyList<string> MissingDependencies { get; }

        public DependencyValidationResult(bool isValid, IReadOnlyList<string> missingDependencies)
        {
            IsValid = isValid;
            MissingDependencies = missingDependencies ?? Array.Empty<string>();
        }
    }
}