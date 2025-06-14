using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MicFx.SharedKernel.Common;
using MicFx.SharedKernel.Common.Exceptions;
using System.ComponentModel.DataAnnotations;

namespace MicFx.Core.Configuration;

/// <summary>
/// Base class for module configuration implementation with validation support
/// </summary>
/// <typeparam name="T">Type of configuration class</typeparam>
public abstract class ModuleConfigurationBase<T> : IModuleConfiguration<T> where T : class, new()
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<ModuleConfigurationBase<T>> _logger;
    private T _value = new();

    protected ModuleConfigurationBase(IConfiguration configuration, ILogger<ModuleConfigurationBase<T>> logger)
    {
        _configuration = configuration;
        _logger = logger;
        LoadConfiguration();
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
    /// Parsed configuration value
    /// </summary>
    public T Value
    {
        get => _value;
        set
        {
            var oldValue = _value;
            _value = value;

            _logger.LogInformation("Configuration changed for module {ModuleName}: {OldValue} -> {NewValue}",
                ModuleName, oldValue, value);
        }
    }

    /// <summary>
    /// Load configuration from IConfiguration
    /// </summary>
    protected virtual void LoadConfiguration()
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
                    Value = configValue;

                    _logger.LogInformation("Configuration loaded for module {ModuleName} from section {SectionName}",
                        ModuleName, SectionName);
                }
                else if (IsRequired)
                {
                    throw new ConfigurationException(ModuleName, SectionName,
                        $"Failed to bind configuration section '{SectionName}' to type {typeof(T).Name}");
                }
            }
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
    /// Type-specific validation for value T using Data Annotations
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
            var errors = validationResults.SelectMany(r => r.ErrorMessage != null ? new[] { r.ErrorMessage } : Array.Empty<string>())
                                         .ToList();

            _logger.LogWarning("Configuration validation failed for module {ModuleName}: {ValidationErrors}",
                ModuleName, string.Join(", ", errors));

            return new ValidationResult($"Validation failed for module '{ModuleName}': {string.Join(", ", errors)}");
        }

        // Tambahkan validasi custom jika ada
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

    /// <summary>
    /// Reload configuration from configuration source
    /// </summary>
    public virtual void Reload()
    {
        _logger.LogInformation("Reloading configuration for module {ModuleName}", ModuleName);
        LoadConfiguration();
    }
}