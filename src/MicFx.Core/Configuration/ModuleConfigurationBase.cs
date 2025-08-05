using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MicFx.SharedKernel.Common;
using MicFx.SharedKernel.Common.Exceptions;
using System.ComponentModel.DataAnnotations;

namespace MicFx.Core.Configuration;

/// <summary>
/// Base class for module configuration with startup-time loading and validation
/// </summary>
/// <typeparam name="T">Type of configuration class</typeparam>
public abstract class ModuleConfigurationBase<T> : IModuleConfiguration<T> where T : class, new()
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<ModuleConfigurationBase<T>> _logger;

    protected ModuleConfigurationBase(IConfiguration configuration, ILogger<ModuleConfigurationBase<T>> logger)
    {
        _configuration = configuration;
        _logger = logger;
        Value = LoadConfiguration();
    }

    /// <summary>
    /// Module name - must be implemented by child class
    /// </summary>
    public abstract string ModuleName { get; }

    /// <summary>
    /// Section name in appsettings - defaults to ModuleName
    /// </summary>
    public virtual string SectionName => ModuleName;

    /// <summary>
    /// Whether this configuration is required - defaults to true
    /// </summary>
    public virtual bool IsRequired => true;

    /// <summary>
    /// Loaded configuration value (immutable after startup)
    /// </summary>
    public T Value { get; private set; }

    /// <summary>
    /// Load configuration from IConfiguration at startup
    /// </summary>
    private T LoadConfiguration()
    {
        try
        {
            var section = _configuration.GetSection(SectionName);

            if (!section.Exists() && IsRequired)
            {
                throw new ConfigurationException(ModuleName, SectionName,
                    $"Required configuration section '{SectionName}' not found for module '{ModuleName}'");
            }

            if (section.Exists())
            {
                var configValue = section.Get<T>();
                if (configValue != null)
                {
                    _logger.LogInformation("Configuration loaded for module {ModuleName} from section {SectionName}",
                        ModuleName, SectionName);
                    return configValue;
                }
                else if (IsRequired)
                {
                    throw new ConfigurationException(ModuleName, SectionName,
                        $"Failed to bind configuration section '{SectionName}' to type {typeof(T).Name}");
                }
            }

            // Return default instance for optional configurations
            _logger.LogInformation("Using default configuration for optional module {ModuleName}", ModuleName);
            return new T();
        }
        catch (Exception ex) when (!(ex is ConfigurationException))
        {
            _logger.LogError(ex, "Error loading configuration for module {ModuleName} from section {SectionName}",
                ModuleName, SectionName);

            throw new ConfigurationException(ModuleName, SectionName,
                $"Error loading configuration for module '{ModuleName}' from section '{SectionName}'", ex);
        }
    }

    /// <summary>
    /// Validate configuration using Data Annotations
    /// </summary>
    /// <returns>Validation result</returns>
    public virtual ValidationResult Validate()
    {
        return ValidateValue(Value);
    }

    /// <summary>
    /// Validate configuration value using Data Annotations and custom rules
    /// </summary>
    /// <param name="value">Value to validate</param>
    /// <returns>Validation result</returns>
    public virtual ValidationResult ValidateValue(T value)
    {
        var validationContext = new ValidationContext(value);
        var validationResults = new List<ValidationResult>();

        bool isValid = Validator.TryValidateObject(value, validationContext, validationResults, true);

        if (!isValid)
        {
            var errors = validationResults
                .Where(r => !string.IsNullOrEmpty(r.ErrorMessage))
                .Select(r => r.ErrorMessage!)
                .ToList();

            _logger.LogWarning("Configuration validation failed for module {ModuleName}: {ValidationErrors}",
                ModuleName, string.Join(", ", errors));

            return new ValidationResult($"Validation failed for module '{ModuleName}': {string.Join(", ", errors)}");
        }

        // Apply custom validation rules if any
        var customValidation = ValidateCustomRules(value);
        if (customValidation != ValidationResult.Success)
        {
            return customValidation;
        }

        _logger.LogDebug("Configuration validation passed for module {ModuleName}", ModuleName);
        return ValidationResult.Success!;
    }

    /// <summary>
    /// Override to add custom validation rules besides Data Annotations
    /// </summary>
    /// <param name="value">Value to validate</param>
    /// <returns>Custom validation result</returns>
    protected virtual ValidationResult ValidateCustomRules(T value)
    {
        return ValidationResult.Success!;
    }
}