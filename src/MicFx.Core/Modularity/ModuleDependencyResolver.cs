using Microsoft.Extensions.Logging;
using MicFx.SharedKernel.Modularity;
using MicFx.SharedKernel.Common.Exceptions;

namespace MicFx.Core.Modularity
{
    /// <summary>
    /// Class for managing and resolving dependencies between modules
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
        /// Validates all dependencies and detects circular dependencies
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
                            DependencyName = dependency,
                            IsCritical = module.IsCritical
                        });
                    }
                }
            }

            // Check for circular dependencies
            var circularDeps = DetectCircularDependencies();
            result.CircularDependencies.AddRange(circularDeps);

            // Check for version conflicts
            var versionConflicts = DetectVersionConflicts();
            result.VersionConflicts.AddRange(versionConflicts);

            // Check for module conflicts
            var moduleConflicts = DetectModuleConflicts();
            result.ModuleConflicts.AddRange(moduleConflicts);

            result.IsValid = !result.MissingDependencies.Any() &&
                           !result.CircularDependencies.Any() &&
                           !result.VersionConflicts.Any() &&
                           !result.ModuleConflicts.Any();

            _logger.LogInformation("Dependency validation completed. Valid: {IsValid}, Missing: {MissingCount}, Circular: {CircularCount}",
                result.IsValid, result.MissingDependencies.Count, result.CircularDependencies.Count);

            return result;
        }

        /// <summary>
        /// Calculates module startup order based on dependencies using topological sorting
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

            // Sort modules with same dependency level by priority
            var orderedResult = result
                .Select((name, index) => new { Name = name, Index = index, Priority = _modules[name].Priority })
                .OrderBy(x => x.Index)
                .ThenBy(x => x.Priority)
                .Select(x => x.Name)
                .ToList();

            _logger.LogInformation("Calculated startup order for {ModuleCount} modules: {StartupOrder}",
                orderedResult.Count, string.Join(" -> ", orderedResult));

            return orderedResult;
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
                    if (_modules.ContainsKey(dependency))
                    {
                        TopologicalSort(dependency, visited, visiting, result);
                    }
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
                    if (!_modules.ContainsKey(dependency)) continue;

                    if (!visited.Contains(dependency))
                    {
                        if (DetectCircularDependencyDFS(dependency, visited, recursionStack, path, circularDeps))
                            return true;
                    }
                    else if (recursionStack.Contains(dependency))
                    {
                        // Found circular dependency
                        var cycleStart = path.IndexOf(dependency);
                        var cycle = path.Skip(cycleStart).Concat(new[] { dependency }).ToList();

                        circularDeps.Add(new CircularDependency
                        {
                            Cycle = cycle
                        });
                        return true;
                    }
                }
            }

            path.RemoveAt(path.Count - 1);
            recursionStack.Remove(moduleName);
            return false;
        }

        private List<VersionConflict> DetectVersionConflicts()
        {
            var conflicts = new List<VersionConflict>();

            // Check framework version compatibility
            foreach (var module in _modules.Values)
            {
                // Assuming framework version is available from somewhere
                var frameworkVersion = "1.0.0"; // This should come from actual framework version

                if (!IsVersionCompatible(frameworkVersion, module.MinimumFrameworkVersion, module.MaximumFrameworkVersion))
                {
                    conflicts.Add(new VersionConflict
                    {
                        ModuleName = module.Name,
                        RequiredVersion = $"{module.MinimumFrameworkVersion} - {module.MaximumFrameworkVersion}",
                        ActualVersion = frameworkVersion,
                        ConflictType = "Framework"
                    });
                }
            }

            return conflicts;
        }

        private List<ModuleConflict> DetectModuleConflicts()
        {
            var conflicts = new List<ModuleConflict>();

            foreach (var module in _modules.Values)
            {
                foreach (var conflictsWith in module.ConflictsWith)
                {
                    if (_modules.ContainsKey(conflictsWith))
                    {
                        conflicts.Add(new ModuleConflict
                        {
                            ModuleName = module.Name,
                            ConflictingModuleName = conflictsWith,
                            Reason = $"{module.Name} conflicts with {conflictsWith}"
                        });
                    }
                }
            }

            return conflicts;
        }

        private bool IsVersionCompatible(string currentVersion, string minVersion, string maxVersion)
        {
            // Simplified version comparison - in real implementation, use proper version parsing
            return string.Compare(currentVersion, minVersion, StringComparison.OrdinalIgnoreCase) >= 0 &&
                   string.Compare(currentVersion, maxVersion, StringComparison.OrdinalIgnoreCase) <= 0;
        }
    }

    /// <summary>
    /// Result of dependency validation
    /// </summary>
    public class ModuleDependencyValidationResult
    {
        public bool IsValid { get; set; }
        public List<MissingDependency> MissingDependencies { get; set; } = new();
        public List<CircularDependency> CircularDependencies { get; set; } = new();
        public List<VersionConflict> VersionConflicts { get; set; } = new();
        public List<ModuleConflict> ModuleConflicts { get; set; } = new();
    }

    public class MissingDependency
    {
        public string ModuleName { get; set; } = string.Empty;
        public string DependencyName { get; set; } = string.Empty;
        public bool IsCritical { get; set; }
    }

    public class CircularDependency
    {
        public List<string> Cycle { get; set; } = new();
    }

    public class VersionConflict
    {
        public string ModuleName { get; set; } = string.Empty;
        public string RequiredVersion { get; set; } = string.Empty;
        public string ActualVersion { get; set; } = string.Empty;
        public string ConflictType { get; set; } = string.Empty;
    }

    public class ModuleConflict
    {
        public string ModuleName { get; set; } = string.Empty;
        public string ConflictingModuleName { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
    }
}