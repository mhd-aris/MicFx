using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MicFx.SharedKernel.Common;
using MicFx.Core.Configuration;

namespace MicFx.Core.Extensions;

/// <summary>
/// Extension methods untuk konfigurasi management
/// Simplified version without complex monitoring features
/// </summary>
public static class ConfigurationExtensions
{
    /// <summary>
    /// Menambahkan MicFx Configuration Management ke service collection
    /// Simplified version without monitoring and hot reload
    /// </summary>
    public static IServiceCollection AddMicFxConfigurationManagement(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<SimpleConfigurationOptions>? configureOptions = null)
    {
        var options = new SimpleConfigurationOptions();
        configureOptions?.Invoke(options);

        // Register simplified configuration manager
        services.AddSingleton<IMicFxConfigurationManager, SimpleMicFxConfigurationManager>();

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

        // Register configuration value sebagai scoped service
        services.AddScoped<T>(serviceProvider =>
        {
            var config = serviceProvider.GetRequiredService<IModuleConfiguration<T>>();
            return config.Value;
        });

        // Auto-register ke configuration manager saat service provider dibuat
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
/// Simplified options untuk konfigurasi Configuration Management
/// </summary>
public class SimpleConfigurationOptions
{
    /// <summary>
    /// Apakah validasi konfigurasi saat startup
    /// </summary>
    public bool ValidateOnStartup { get; set; } = true;

    /// <summary>
    /// Apakah throw exception jika ada konfigurasi tidak valid
    /// </summary>
    public bool ThrowOnValidationFailure { get; set; } = false;
}

/// <summary>
/// Interface untuk registration tracking
/// </summary>
public interface IConfigurationRegistration<T> where T : class
{
    IModuleConfiguration<T> Configuration { get; }
}

/// <summary>
/// Implementation untuk registration tracking
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
/// Simple hosted service untuk validasi konfigurasi saat startup
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