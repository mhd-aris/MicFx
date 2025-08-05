using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MicFx.SharedKernel.Common;
using MicFx.Core.Configuration;

namespace MicFx.Core.Extensions;

/// <summary>
/// Extension methods for configuration management
/// </summary>
public static class ConfigurationExtensions
{
    /// <summary>
    /// Adds MicFx Configuration Management to the service collection
    /// </summary>
    public static IServiceCollection AddMicFxConfigurationManagement(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<SimpleConfigurationOptions>? configureOptions = null)
    {
        var options = new SimpleConfigurationOptions();
        configureOptions?.Invoke(options);

        services.AddSingleton<IMicFxConfigurationManager, MicFxConfigurationManager>();

        // Validate configurations on startup if enabled
        if (options.ValidateOnStartup)
        {
            services.AddHostedService<SimpleConfigurationValidationService>();
        }

        return services;
    }

    /// <summary>
    /// Menambahkan konfigurasi module ke service collection
    /// </summary>
    public static IServiceCollection AddModuleConfiguration<T>(
        this IServiceCollection services,
        Func<IServiceProvider, IModuleConfiguration<T>> configurationFactory) where T : class
    {
        // Register configuration instance
        services.AddSingleton(configurationFactory);

        // Register as IModuleConfiguration<T>
        services.AddScoped<T>(serviceProvider =>
        {
            var config = serviceProvider.GetRequiredService<IModuleConfiguration<T>>();
            return config.Value;
        });

        // Add registration for IConfigurationRegistration<T>
        // This allows other services to access the configuration registration
        services.AddSingleton<IConfigurationRegistration<T>>(serviceProvider =>
        {
            var configManager = serviceProvider.GetRequiredService<IMicFxConfigurationManager>();
            var configuration = serviceProvider.GetRequiredService<IModuleConfiguration<T>>();

            configManager.RegisterModuleConfiguration(configuration);

            return new ConfigurationRegistration<T>(configuration);
        });

        return services;
    }
}

/// <summary>
/// Options for simple configuration management
/// </summary>
public class SimpleConfigurationOptions
{
    /// <summary>
    /// Whether to validate configuration at startup
    /// </summary>
    public bool ValidateOnStartup { get; set; } = true;

    /// <summary>
    /// Whether to throw an exception if any configuration is invalid
    /// </summary>
    public bool ThrowOnValidationFailure { get; set; } = false;
}

/// <summary>
/// Interface for module configuration registration
/// </summary>
public interface IConfigurationRegistration<T> where T : class
{
    IModuleConfiguration<T> Configuration { get; }
}

/// <summary>
/// Implementation of IConfigurationRegistration for module configurations
/// </summary>
internal class ConfigurationRegistration<T> : IConfigurationRegistration<T> where T : class
{
    public IModuleConfiguration<T> Configuration { get; }

    public ConfigurationRegistration(IModuleConfiguration<T> configuration)
    {
        Configuration = configuration;
    }
}

/// <summary>
/// Service to validate configurations at startup
/// </summary>
internal class SimpleConfigurationValidationService : IHostedService
{
    private readonly IMicFxConfigurationManager _configurationManager;
    private readonly ILogger<SimpleConfigurationValidationService> _logger;
    private readonly SimpleConfigurationOptions _options;

    public SimpleConfigurationValidationService(
        IMicFxConfigurationManager configurationManager,
        ILogger<SimpleConfigurationValidationService> logger,
        IConfiguration configuration)
    {
        _configurationManager = configurationManager;
        _logger = logger;

        _options = new SimpleConfigurationOptions();
        configuration.GetSection("MicFx:Configuration").Bind(_options);
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting configuration validation");

        try
        {
            var validationResult = _configurationManager.ValidateAllConfigurations();
            
            if (validationResult != System.ComponentModel.DataAnnotations.ValidationResult.Success)
            {
                _logger.LogError("Configuration validation failed: {ErrorMessage}", validationResult.ErrorMessage);
                
                if (_options.ThrowOnValidationFailure)
                {
                    throw new InvalidOperationException($"Configuration validation failed: {validationResult.ErrorMessage}");
                }
            }
            else
            {
                _logger.LogInformation("All configurations validated successfully");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during configuration validation");
            
            if (_options.ThrowOnValidationFailure)
            {
                throw;
            }
        }

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}