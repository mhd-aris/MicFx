using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MicFx.SharedKernel.Common;
using MicFx.SharedKernel.Common.Exceptions;
using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations;

namespace MicFx.Core.Configuration;

/// <summary>
/// Implementation terpusat untuk management konfigurasi module
/// </summary>
public class MicFxConfigurationManager : IMicFxConfigurationManager
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<MicFxConfigurationManager> _logger;
    private readonly ConcurrentDictionary<string, IModuleConfiguration> _configurations = new();
    private readonly ConcurrentDictionary<Type, IModuleConfiguration> _configurationsByType = new();

    public event EventHandler<ConfigurationChangedEventArgs>? ConfigurationChanged;

    public MicFxConfigurationManager(IConfiguration configuration, ILogger<MicFxConfigurationManager> logger)
    {
        _configuration = configuration;
        _logger = logger;

        _logger.LogInformation("ConfigurationManager initialized");
    }

    /// <summary>
    /// Mendaftarkan konfigurasi module
    /// </summary>
    public void RegisterModuleConfiguration<T>(IModuleConfiguration<T> configuration) where T : class
    {
        var moduleName = configuration.ModuleName;
        var configType = typeof(T);

        if (_configurations.ContainsKey(moduleName))
        {
            _logger.LogWarning("Module configuration for {ModuleName} already registered, replacing", moduleName);
        }

        _configurations.AddOrUpdate(moduleName, configuration, (key, oldValue) => configuration);
        _configurationsByType.AddOrUpdate(configType, configuration, (key, oldValue) => configuration);

        _logger.LogInformation("Registered configuration for module {ModuleName} with type {ConfigType}",
            moduleName, configType.Name);

        // Validasi konfigurasi saat registration
        try
        {
            var validationResult = configuration.Validate();
            if (validationResult != ValidationResult.Success)
            {
                _logger.LogWarning("Configuration validation failed for module {ModuleName}: {ValidationMessage}",
                    moduleName, validationResult.ErrorMessage);

                var errors = new List<string>();
                if (!string.IsNullOrEmpty(validationResult.ErrorMessage))
                {
                    errors.Add(validationResult.ErrorMessage);
                }

                throw new ConfigurationValidationException(moduleName, configuration.SectionName, errors);
            }

            _logger.LogInformation("Configuration validation passed for module {ModuleName}", moduleName);
        }
        catch (Exception ex) when (!(ex is ConfigurationValidationException))
        {
            _logger.LogError(ex, "Error validating configuration for module {ModuleName}", moduleName);
            throw new ConfigurationException(moduleName, configuration.SectionName,
                "Error validating configuration during registration", ex);
        }
    }

    /// <summary>
    /// Mendapatkan konfigurasi module berdasarkan type
    /// </summary>
    public IModuleConfiguration<T>? GetModuleConfiguration<T>() where T : class
    {
        var configType = typeof(T);

        if (_configurationsByType.TryGetValue(configType, out var configuration))
        {
            return configuration as IModuleConfiguration<T>;
        }

        _logger.LogDebug("Configuration not found for type {ConfigType}", configType.Name);
        return null;
    }

    /// <summary>
    /// Mendapatkan konfigurasi module berdasarkan nama module
    /// </summary>
    public IModuleConfiguration? GetModuleConfiguration(string moduleName)
    {
        if (string.IsNullOrWhiteSpace(moduleName))
        {
            throw new ArgumentException("Module name cannot be null or empty", nameof(moduleName));
        }

        if (_configurations.TryGetValue(moduleName, out var configuration))
        {
            return configuration;
        }

        _logger.LogDebug("Configuration not found for module {ModuleName}", moduleName);
        return null;
    }

    /// <summary>
    /// Mendapatkan semua konfigurasi module yang terdaftar
    /// </summary>
    public IEnumerable<IModuleConfiguration> GetAllConfigurations()
    {
        return _configurations.Values.ToList();
    }

    /// <summary>
    /// Validasi semua konfigurasi module
    /// </summary>
    public ValidationResult ValidateAllConfigurations()
    {
        var validationErrors = new List<string>();

        foreach (var (moduleName, configuration) in _configurations)
        {
            try
            {
                var result = configuration.Validate();
                if (result != ValidationResult.Success && !string.IsNullOrEmpty(result.ErrorMessage))
                {
                    validationErrors.Add($"[{moduleName}] {result.ErrorMessage}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating configuration for module {ModuleName}", moduleName);
                validationErrors.Add($"[{moduleName}] Validation error: {ex.Message}");
            }
        }

        if (validationErrors.Any())
        {
            var errorMessage = $"Configuration validation failed for {validationErrors.Count} module(s):\n" +
                              string.Join("\n", validationErrors);

            _logger.LogWarning("Overall configuration validation failed: {ValidationErrors}",
                string.Join(", ", validationErrors));

            return new ValidationResult(errorMessage);
        }

        _logger.LogInformation("All module configurations validated successfully ({ConfigCount} modules)",
            _configurations.Count);

        return ValidationResult.Success!;
    }

    /// <summary>
    /// Reload konfigurasi dari configuration source
    /// </summary>
    public async Task ReloadConfigurationsAsync()
    {
        _logger.LogInformation("Starting configuration reload for {ConfigCount} registered modules",
            _configurations.Count);

        var reloadTasks = new List<Task>();

        foreach (var (moduleName, configuration) in _configurations)
        {
            reloadTasks.Add(Task.Run(() =>
            {
                try
                {
                    // FIXED: Use proper interface method instead of dangerous reflection
                    var oldValue = configuration.GetCurrentValueSnapshot();
                    
                    // Reload configuration
                    configuration.Reload();
                    
                    var newValue = configuration.GetCurrentValueSnapshot();

                    // Trigger event if there are changes
                    if (!Equals(oldValue, newValue))
                    {
                        OnConfigurationChanged(new ConfigurationChangedEventArgs
                        {
                            ModuleName = moduleName,
                            SectionName = configuration.SectionName,
                            OldValue = oldValue,
                            NewValue = newValue,
                            ChangedAt = DateTime.UtcNow
                        });
                    }

                    _logger.LogDebug("Configuration reloaded successfully for module {ModuleName}", moduleName);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to reload configuration for module {ModuleName}", moduleName);
                }
            }));
        }

        await Task.WhenAll(reloadTasks);

        _logger.LogInformation("Configuration reload completed for all modules");
    }

    /// <summary>
    /// Trigger event perubahan konfigurasi
    /// </summary>
    protected virtual void OnConfigurationChanged(ConfigurationChangedEventArgs e)
    {
        _logger.LogInformation("Configuration changed for module {ModuleName} in section {SectionName}",
            e.ModuleName, e.SectionName);

        ConfigurationChanged?.Invoke(this, e);
    }

    /// <summary>
    /// Get summary informasi konfigurasi untuk monitoring
    /// </summary>
    public ConfigurationSummary GetConfigurationSummary()
    {
        var summary = new ConfigurationSummary
        {
            TotalConfigurations = _configurations.Count,
            ValidConfigurations = 0,
            InvalidConfigurations = 0,
            RequiredConfigurations = 0,
            OptionalConfigurations = 0,
            ModuleDetails = new List<ModuleConfigurationDetail>()
        };

        foreach (var (moduleName, configuration) in _configurations)
        {
            var detail = new ModuleConfigurationDetail
            {
                ModuleName = moduleName,
                SectionName = configuration.SectionName,
                IsRequired = configuration.IsRequired,
                IsValid = false,
                ValidationMessage = string.Empty
            };

            try
            {
                var validationResult = configuration.Validate();
                detail.IsValid = validationResult == ValidationResult.Success;
                detail.ValidationMessage = validationResult?.ErrorMessage ?? string.Empty;

                if (detail.IsValid)
                {
                    summary.ValidConfigurations++;
                }
                else
                {
                    summary.InvalidConfigurations++;
                }
            }
            catch (Exception ex)
            {
                detail.IsValid = false;
                detail.ValidationMessage = ex.Message;
                summary.InvalidConfigurations++;
            }

            if (configuration.IsRequired)
            {
                summary.RequiredConfigurations++;
            }
            else
            {
                summary.OptionalConfigurations++;
            }

            summary.ModuleDetails.Add(detail);
        }

        return summary;
    }
}

/// <summary>
/// Summary informasi konfigurasi
/// </summary>
public class ConfigurationSummary
{
    public int TotalConfigurations { get; set; }
    public int ValidConfigurations { get; set; }
    public int InvalidConfigurations { get; set; }
    public int RequiredConfigurations { get; set; }
    public int OptionalConfigurations { get; set; }
    public List<ModuleConfigurationDetail> ModuleDetails { get; set; } = new();
}

/// <summary>
/// Detail konfigurasi per module
/// </summary>
public class ModuleConfigurationDetail
{
    public string ModuleName { get; set; } = string.Empty;
    public string SectionName { get; set; } = string.Empty;
    public bool IsRequired { get; set; }
    public bool IsValid { get; set; }
    public string ValidationMessage { get; set; } = string.Empty;
}