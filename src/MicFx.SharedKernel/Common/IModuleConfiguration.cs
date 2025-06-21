using System.ComponentModel.DataAnnotations;

namespace MicFx.SharedKernel.Common;

/// <summary>
/// Base interface for module configuration with startup-time loading and validation
/// SIMPLIFIED: Removed hot reload and change detection for better stability
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
}

/// <summary>
/// Generic interface for strongly-typed module configuration
/// SIMPLIFIED: Immutable configuration loaded at startup
/// </summary>
/// <typeparam name="T">Type of configuration class</typeparam>
public interface IModuleConfiguration<T> : IModuleConfiguration where T : class
{
    /// <summary>
    /// Loaded configuration value (immutable after startup)
    /// SIMPLIFIED: Read-only after initial load
    /// </summary>
    T Value { get; }

    /// <summary>
    /// Type-specific validation for T
    /// </summary>
    /// <param name="value">Configuration value to validate</param>
    /// <returns>Validation result</returns>
    ValidationResult ValidateValue(T value);
}