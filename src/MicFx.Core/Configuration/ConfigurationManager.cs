using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MicFx.SharedKernel.Common;
using MicFx.SharedKernel.Common.Exceptions;
using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations;

namespace MicFx.Core.Configuration;

/// <summary>
/// Simplified implementation untuk management konfigurasi module
/// Removed complex monitoring and hot reload features for better maintainability
/// </summary>
public class SimpleMicFxConfigurationManager : IMicFxConfigurationManager
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<SimpleMicFxConfigurationManager> _logger;
    private readonly ConcurrentDictionary<string, IModuleConfiguration> _configurations = new();
    private readonly ConcurrentDictionary<Type, IModuleConfiguration> _configurationsByType = new();

    public SimpleMicFxConfigurationManager(IConfiguration configuration, ILogger<SimpleMicFxConfigurationManager> logger)
    {
        _configuration = configuration;
        _logger = logger;

        _logger.LogInformation("Simple ConfigurationManager initialized");
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

        // Simple validation saat registration
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

                throw new ConfigurationException(moduleName, configuration.SectionName, 
                    "Configuration validation failed during registration");
            }

            _logger.LogInformation("Configuration validation passed for module {ModuleName}", moduleName);
        }
        catch (Exception ex) when (!(ex is ConfigurationException))
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
    /// Validasi semua konfigurasi module (simplified)
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
}