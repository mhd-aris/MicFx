using Microsoft.Extensions.Logging;
using MicFx.SharedKernel.Modularity;
using MicFx.SharedKernel.Common.Exceptions;

namespace MicFx.Core.Modularity
{
    /// <summary>
    /// Class for managing and resolving dependencies between modules
    /// Simplified from over-engineered version for better maintainability
    /// </summary>
    public class ModuleDependencyResolver
    {
        private readonly ILogger<ModuleDependencyResolver> _logger;
        private readonly Dictionary<string, IModuleManifest> _modules = new();
        private readonly Dictionary<string, List<string>> _dependencyGraph = new();
        private readonly Dictionary<string, List<string>> _reverseDependencyGraph = new();

        public ModuleDependencyResolver(ILogger<ModuleDependencyResolver> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Registers a module into the dependency resolver
        /// </summary>
        public void RegisterModule(IModuleManifest manifest)
        {
            if (string.IsNullOrEmpty(manifest.Name))
            {
                throw new ModuleException("Module name cannot be null or empty", "DependencyResolver");
            }

            if (_modules.ContainsKey(manifest.Name))
            {
                _logger.LogWarning("Module {ModuleName} is already registered. Overwriting...", manifest.Name);
            }

            _modules[manifest.Name] = manifest;
            _dependencyGraph[manifest.Name] = new List<string>(manifest.Dependencies);

            // Build reverse dependency graph
            _reverseDependencyGraph[manifest.Name] = new List<string>();
            foreach (var dependency in manifest.Dependencies)
            {
                if (!_reverseDependencyGraph.ContainsKey(dependency))
                {
                    _reverseDependencyGraph[dependency] = new List<string>();
                }
                _reverseDependencyGraph[dependency].Add(manifest.Name);
            }

            _logger.LogInformation("Registered module {ModuleName} with {DependencyCount} dependencies",
                manifest.Name, manifest.Dependencies.Length);
        }

        /// <summary>
        /// Validates dependencies - simplified version focused on essential validations
        /// </summary>
        public ModuleDependencyValidationResult ValidateDependencies()
        {
            var result = new ModuleDependencyValidationResult();

            // Check for missing dependencies
            foreach (var module in _modules.Values)
            {
                foreach (var dependency in module.Dependencies)
                {
                    if (!_modules.ContainsKey(dependency))
                    {
                        result.MissingDependencies.Add(new MissingDependency
                        {
                            ModuleName = module.Name,
                            DependencyName = dependency
                        });
                    }
                }
            }

            // Check for circular dependencies
            var circularDeps = DetectCircularDependencies();
            result.CircularDependencies.AddRange(circularDeps);

            result.IsValid = !result.MissingDependencies.Any() && !result.CircularDependencies.Any();

            _logger.LogInformation("Dependency validation completed. Valid: {IsValid}, Missing: {MissingCount}, Circular: {CircularCount}",
                result.IsValid, result.MissingDependencies.Count, result.CircularDependencies.Count);

            return result;
        }

        /// <summary>
        /// Calculates module startup order based on dependencies using topological sorting
        /// Simplified without complex priority handling
        /// </summary>
        public List<string> GetStartupOrder()
        {
            var result = new List<string>();
            var visited = new HashSet<string>();
            var visiting = new HashSet<string>();

            foreach (var moduleName in _modules.Keys)
            {
                if (!visited.Contains(moduleName))
                {
                    TopologicalSort(moduleName, visited, visiting, result);
                }
            }

            _logger.LogInformation("Calculated startup order for {ModuleCount} modules: {StartupOrder}",
                result.Count, string.Join(" -> ", result));

            return result;
        }

        /// <summary>
        /// Calculates module shutdown order (reverse of startup order)
        /// </summary>
        public List<string> GetShutdownOrder()
        {
            var startupOrder = GetStartupOrder();
            startupOrder.Reverse();

            _logger.LogInformation("Calculated shutdown order for {ModuleCount} modules: {ShutdownOrder}",
                startupOrder.Count, string.Join(" -> ", startupOrder));

            return startupOrder;
        }

        /// <summary>
        /// Gets all dependencies for a specific module (including transitive dependencies)
        /// </summary>
        public List<string> GetAllDependencies(string moduleName)
        {
            var dependencies = new HashSet<string>();
            var queue = new Queue<string>();

            if (!_modules.ContainsKey(moduleName))
            {
                return new List<string>();
            }

            queue.Enqueue(moduleName);

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();

                if (_dependencyGraph.ContainsKey(current))
                {
                    foreach (var dependency in _dependencyGraph[current])
                    {
                        if (!dependencies.Contains(dependency))
                        {
                            dependencies.Add(dependency);
                            queue.Enqueue(dependency);
                        }
                    }
                }
            }

            return dependencies.ToList();
        }

        /// <summary>
        /// Gets all modules that depend on a specific module
        /// </summary>
        public List<string> GetDependents(string moduleName)
        {
            if (!_reverseDependencyGraph.ContainsKey(moduleName))
            {
                return new List<string>();
            }

            return _reverseDependencyGraph[moduleName].ToList();
        }

        private void TopologicalSort(string moduleName, HashSet<string> visited, HashSet<string> visiting, List<string> result)
        {
            if (visiting.Contains(moduleName))
            {
                throw new ModuleException($"Circular dependency detected involving module: {moduleName}", "DependencyResolver");
            }

            if (visited.Contains(moduleName))
            {
                return;
            }

            visiting.Add(moduleName);

            if (_dependencyGraph.ContainsKey(moduleName))
            {
                foreach (var dependency in _dependencyGraph[moduleName])
                {
                    TopologicalSort(dependency, visited, visiting, result);
                }
            }

            visiting.Remove(moduleName);
            visited.Add(moduleName);
            result.Add(moduleName);
        }

        private List<CircularDependency> DetectCircularDependencies()
        {
            var circularDeps = new List<CircularDependency>();
            var visited = new HashSet<string>();
            var recursionStack = new HashSet<string>();

            foreach (var moduleName in _modules.Keys)
            {
                if (!visited.Contains(moduleName))
                {
                    var path = new List<string>();
                    DetectCircularDependencyDFS(moduleName, visited, recursionStack, path, circularDeps);
                }
            }

            return circularDeps;
        }

        private bool DetectCircularDependencyDFS(string moduleName, HashSet<string> visited, 
            HashSet<string> recursionStack, List<string> path, List<CircularDependency> circularDeps)
        {
            visited.Add(moduleName);
            recursionStack.Add(moduleName);
            path.Add(moduleName);

            if (_dependencyGraph.ContainsKey(moduleName))
            {
                foreach (var dependency in _dependencyGraph[moduleName])
                {
                    if (!visited.Contains(dependency))
                    {
                        if (DetectCircularDependencyDFS(dependency, visited, recursionStack, path, circularDeps))
                        {
                            return true;
                        }
                    }
                    else if (recursionStack.Contains(dependency))
                    {
                        // Found circular dependency
                        var cycle = new List<string>(path.SkipWhile(m => m != dependency));
                        cycle.Add(dependency); // Complete the cycle

                        circularDeps.Add(new CircularDependency { Cycle = cycle });
                        return true;
                    }
                }
            }

            path.RemoveAt(path.Count - 1);
            recursionStack.Remove(moduleName);
            return false;
        }
    }

    /// <summary>
    /// Simplified dependency validation result - removed over-engineered features
    /// </summary>
    public class ModuleDependencyValidationResult
    {
        public bool IsValid { get; set; }
        public List<MissingDependency> MissingDependencies { get; set; } = new();
        public List<CircularDependency> CircularDependencies { get; set; } = new();
    }

    /// <summary>
    /// Simplified missing dependency class
    /// </summary>
    public class MissingDependency
    {
        public string ModuleName { get; set; } = string.Empty;
        public string DependencyName { get; set; } = string.Empty;
    }

    /// <summary>
    /// Circular dependency detection result
    /// </summary>
    public class CircularDependency
    {
        public List<string> Cycle { get; set; } = new();
    }
}