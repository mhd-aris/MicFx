using System.ComponentModel.DataAnnotations;

namespace MicFx.SharedKernel.Common;

/// <summary>
/// Interface for centralized module configuration management
/// </summary>
public interface IMicFxConfigurationManager
{
    /// <summary>
    /// Register module configuration
    /// </summary>
    /// <typeparam name="T">Type of configuration class</typeparam>
    /// <param name="configuration">Module configuration instance</param>
    void RegisterModuleConfiguration<T>(IModuleConfiguration<T> configuration) where T : class;

    /// <summary>
    /// Get module configuration by type
    /// </summary>
    /// <typeparam name="T">Type of configuration class</typeparam>
    /// <returns>Configuration instance or null if not found</returns>
    IModuleConfiguration<T>? GetModuleConfiguration<T>() where T : class;

    /// <summary>
    /// Get module configuration by module name
    /// </summary>
    /// <param name="moduleName">Module name</param>
    /// <returns>Configuration instance or null if not found</returns>
    IModuleConfiguration? GetModuleConfiguration(string moduleName);

    /// <summary>
    /// Get all registered module configurations
    /// </summary>
    /// <returns>List of all module configurations</returns>
    IEnumerable<IModuleConfiguration> GetAllConfigurations();

    /// <summary>
    /// Validate all module configurations
    /// </summary>
    /// <returns>Validation result with error list if any</returns>
    ValidationResult ValidateAllConfigurations();
}