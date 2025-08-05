using Microsoft.Extensions.Logging;
using MicFx.SharedKernel.Modularity;

namespace MicFx.Core.Modularity;

/// <summary>
/// Module loader with priority-based loading
/// </summary>
public class ModuleLoader
{
    private readonly ILogger<ModuleLoader> _logger;
    private readonly List<IModuleManifest> _modules = new();

    public ModuleLoader(ILogger<ModuleLoader> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Register a module for loading
    /// </summary>
    public void RegisterModule(IModuleManifest manifest)
    {
        if (manifest == null)
            throw new ArgumentNullException(nameof(manifest));

        if (string.IsNullOrWhiteSpace(manifest.Name))
            throw new ArgumentException("Module name cannot be empty", nameof(manifest));

        _modules.Add(manifest);
        _logger.LogInformation("Registered module '{ModuleName}' with priority {Priority}", 
            manifest.Name, manifest.Priority);
    }

    /// <summary>
    /// Get modules in startup order (by priority, then alphabetically)
    /// </summary>
    public IReadOnlyList<IModuleManifest> GetStartupOrder()
    {
        var ordered = _modules
            .OrderBy(m => m.Priority)  // Lower number = higher priority (loads first)
            .ThenBy(m => m.Name, StringComparer.OrdinalIgnoreCase)  // Alphabetical for consistency
            .ToList()
            .AsReadOnly();

        _logger.LogInformation("Startup order for {ModuleCount} modules: {StartupOrder}",
            ordered.Count, string.Join(" → ", ordered.Select(m => m.Name)));

        return ordered;
    }

    /// <summary>
    /// Get count of registered modules
    /// </summary>
    public int ModuleCount => _modules.Count;

    /// <summary>
    /// Validate module registration
    /// </summary>
    public bool ValidateRegistration()
    {
        var criticalModules = _modules.Where(m => m.IsCritical).ToList();
        
        foreach (var critical in criticalModules)
        {
            _logger.LogInformation("Critical module '{ModuleName}' registered successfully", critical.Name);
        }

        _logger.LogInformation("Module registration validation completed. {TotalModules} modules, {CriticalModules} critical",
            _modules.Count, criticalModules.Count);

        return true;
    }
}
