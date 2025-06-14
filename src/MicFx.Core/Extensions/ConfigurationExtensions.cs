using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MicFx.Core.Configuration;
using MicFx.SharedKernel.Common;

namespace MicFx.Core.Extensions;

/// <summary>
/// Extension methods untuk konfigurasi Configuration Management
/// </summary>
public static class ConfigurationExtensions
{
    /// <summary>
    /// Mendaftarkan MicFx Configuration Management ke service container
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="configuration">IConfiguration instance</param>
    /// <param name="configureOptions">Optional configuration options</param>
    /// <returns>Service collection untuk method chaining</returns>
    public static IServiceCollection AddMicFxConfigurationManagement(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<ConfigurationManagementOptions>? configureOptions = null)
    {
        var options = new ConfigurationManagementOptions();
        configureOptions?.Invoke(options);

        // Register configuration management service sebagai singleton
        services.AddSingleton<IMicFxConfigurationManager>(serviceProvider =>
        {
            var logger = serviceProvider.GetRequiredService<ILogger<MicFxConfigurationManager>>();
            var configManager = new MicFxConfigurationManager(configuration, logger);

            // Auto-register semua konfigurasi yang sudah didefinisikan
            if (options.AutoRegisterConfigurations)
            {
                AutoRegisterConfigurations(configManager, serviceProvider);
            }

            return configManager;
        });

        // Register hosted service untuk configuration monitoring jika diperlukan
        if (options.EnableConfigurationMonitoring)
        {
            services.AddHostedService<ConfigurationMonitoringService>();
        }

        // Register configuration change notification service
        if (options.EnableChangeNotifications)
        {
            services.AddSingleton<IConfigurationChangeNotificationService, ConfigurationChangeNotificationService>();
        }

        return services;
    }

    /// <summary>
    /// Mendaftarkan konfigurasi module ke Configuration Manager
    /// </summary>
    /// <typeparam name="T">Type dari configuration class</typeparam>
    /// <param name="services">Service collection</param>
    /// <param name="configurationFactory">Factory function untuk membuat instance configuration</param>
    /// <returns>Service collection untuk method chaining</returns>
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

    /// <summary>
    /// Auto-register semua konfigurasi yang sudah didefinisikan
    /// </summary>
    private static void AutoRegisterConfigurations(IMicFxConfigurationManager configManager, IServiceProvider serviceProvider)
    {
        var logger = serviceProvider.GetRequiredService<ILogger<MicFxConfigurationManager>>();

        try
        {
            // Cari semua service yang implement IModuleConfiguration
            var configurationServices = serviceProvider.GetServices<IModuleConfiguration>();

            foreach (var config in configurationServices)
            {
                // Register via reflection karena kita tidak tau type T
                var configType = config.GetType();
                var interfaces = configType.GetInterfaces()
                    .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IModuleConfiguration<>))
                    .ToList();

                foreach (var interfaceType in interfaces)
                {
                    var genericType = interfaceType.GetGenericArguments()[0];
                    var registerMethod = typeof(IMicFxConfigurationManager)
                        .GetMethod(nameof(IMicFxConfigurationManager.RegisterModuleConfiguration))
                        ?.MakeGenericMethod(genericType);

                    registerMethod?.Invoke(configManager, new object[] { config });

                    logger.LogDebug("Auto-registered configuration for type {ConfigType}", genericType.Name);
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during auto-registration of configurations");
        }
    }
}

/// <summary>
/// Options untuk konfigurasi Configuration Management
/// </summary>
public class ConfigurationManagementOptions
{
    /// <summary>
    /// Apakah auto-register semua konfigurasi saat startup
    /// </summary>
    public bool AutoRegisterConfigurations { get; set; } = true;

    /// <summary>
    /// Enable monitoring konfigurasi untuk hot-reload
    /// </summary>
    public bool EnableConfigurationMonitoring { get; set; } = true;

    /// <summary>
    /// Enable notification saat konfigurasi berubah
    /// </summary>
    public bool EnableChangeNotifications { get; set; } = true;

    /// <summary>
    /// Interval monitoring dalam milidetik
    /// </summary>
    public int MonitoringIntervalMs { get; set; } = 30000; // 30 detik

    /// <summary>
    /// Apakah validasi konfigurasi saat startup
    /// </summary>
    public bool ValidateOnStartup { get; set; } = true;

    /// <summary>
    /// Apakah throw exception jika ada konfigurasi tidak valid
    /// </summary>
    public bool ThrowOnValidationFailure { get; set; } = true;
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
/// Interface untuk notification service
/// </summary>
public interface IConfigurationChangeNotificationService
{
    event EventHandler<ConfigurationChangedEventArgs>? ConfigurationChanged;
    void NotifyConfigurationChanged(ConfigurationChangedEventArgs args);
}

/// <summary>
/// Implementation untuk notification service
/// </summary>
internal class ConfigurationChangeNotificationService : IConfigurationChangeNotificationService
{
    public event EventHandler<ConfigurationChangedEventArgs>? ConfigurationChanged;

    public void NotifyConfigurationChanged(ConfigurationChangedEventArgs args)
    {
        ConfigurationChanged?.Invoke(this, args);
    }
}

/// <summary>
/// Hosted service untuk monitoring konfigurasi
/// </summary>
internal class ConfigurationMonitoringService : IHostedService, IDisposable
{
    private readonly IMicFxConfigurationManager _configurationManager;
    private readonly ILogger<ConfigurationMonitoringService> _logger;
    private readonly ConfigurationManagementOptions _options;
    private Timer? _timer;

    public ConfigurationMonitoringService(
        IMicFxConfigurationManager configurationManager,
        ILogger<ConfigurationMonitoringService> logger,
        IConfiguration configuration)
    {
        _configurationManager = configurationManager;
        _logger = logger;

        _options = new ConfigurationManagementOptions();
        configuration.GetSection("MicFx:ConfigurationManagement").Bind(_options);
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Configuration monitoring service started with interval {IntervalMs}ms",
            _options.MonitoringIntervalMs);

        _timer = new Timer(MonitorConfigurations, null, TimeSpan.Zero, TimeSpan.FromMilliseconds(_options.MonitoringIntervalMs));

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Configuration monitoring service stopped");
        _timer?.Change(Timeout.Infinite, 0);
        return Task.CompletedTask;
    }

    private async void MonitorConfigurations(object? state)
    {
        try
        {
            await _configurationManager.ReloadConfigurationsAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during configuration monitoring");
        }
    }

    public void Dispose()
    {
        _timer?.Dispose();
    }
}