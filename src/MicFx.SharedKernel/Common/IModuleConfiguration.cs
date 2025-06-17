using System.ComponentModel.DataAnnotations;

namespace MicFx.SharedKernel.Common;

/// <summary>
/// Base interface for module configuration that can be validated
/// </summary>
public interface IModuleConfiguration
{
    /// <summary>
    /// Module name that owns this configuration
    /// </summary>
    string ModuleName { get; }

    /// <summary>
    /// Section name in appsettings for this configuration
    /// </summary>
    string SectionName { get; }

    /// <summary>
    /// Validate module configuration
    /// </summary>
    /// <returns>Validation result with error list if any</returns>
    ValidationResult Validate();

    /// <summary>
    /// Whether this configuration is required
    /// </summary>
    bool IsRequired { get; }

    /// <summary>
    /// Get current configuration value snapshot for change detection
    /// </summary>
    object? GetCurrentValueSnapshot();

    /// <summary>
    /// Reload configuration from source
    /// </summary>
    void Reload();
}

/// <summary>
/// Generic interface for strongly-typed module configuration
/// </summary>
/// <typeparam name="T">Type of configuration class</typeparam>
public interface IModuleConfiguration<T> : IModuleConfiguration where T : class
{
    /// <summary>
    /// Parsed configuration value
    /// </summary>
    T Value { get; set; }

    /// <summary>
    /// Type-specific validation for T
    /// </summary>
    /// <param name="value">Configuration value to validate</param>
    /// <returns>Validation result</returns>
    ValidationResult ValidateValue(T value);
}