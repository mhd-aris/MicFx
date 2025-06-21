using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;

namespace MicFx.Infrastructure.Logging;

/// <summary>
/// Simplified Serilog configuration for the MicFx Framework
/// Provides basic structured logging without over-engineering
/// </summary>
public static class SerilogExtensions
{
    /// <summary>
    /// Adds basic Serilog logging to the MicFx Framework
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="configuration">Application configuration</param>
    /// <param name="environment">Web host environment</param>
    /// <returns>Service collection for method chaining</returns>
    public static IServiceCollection AddMicFxSerilog(
        this IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        var options = new MicFxSerilogOptions();
        services.AddSingleton(options);

        // Create basic logger configuration
        var loggerConfiguration = CreateBasicLoggerConfiguration(configuration, environment, options);
        Log.Logger = loggerConfiguration.CreateLogger();

        return services;
    }

    /// <summary>
    /// Uses basic Serilog request logging in the pipeline
    /// </summary>
    /// <param name="app">Application builder</param>
    /// <returns>Application builder for method chaining</returns>
    public static IApplicationBuilder UseMicFxSerilog(this IApplicationBuilder app)
    {
        app.UseSerilogRequestLogging(configure =>
        {
            configure.MessageTemplate = "HTTP {RequestMethod} {RequestPath} {StatusCode} in {Elapsed:0.0000}ms";
            configure.GetLevel = (ctx, _, ex) => ex != null ? LogEventLevel.Error : 
                ctx.Response.StatusCode >= 500 ? LogEventLevel.Error :
                ctx.Response.StatusCode >= 400 ? LogEventLevel.Warning : LogEventLevel.Information;
        });

        return app;
    }

    /// <summary>
    /// Creates basic logger configuration with essential settings only
    /// </summary>
    private static LoggerConfiguration CreateBasicLoggerConfiguration(
        IConfiguration configuration,
        IWebHostEnvironment environment,
        MicFxSerilogOptions options)
    {
        var loggerConfig = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .Enrich.FromLogContext()
            .Enrich.WithEnvironmentName()
            .Enrich.With<MicFxModuleEnricher>()
            .MinimumLevel.Is(options.MinimumLevel)
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("System", LogEventLevel.Warning);

        // Console logging
        if (environment.IsDevelopment())
        {
            loggerConfig.WriteTo.Console(
                outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} <m:{Module}>{NewLine}{Exception}");
        }
        else
        {
            loggerConfig.WriteTo.Console(formatter: new CompactJsonFormatter());
        }

        // File logging
        var logPath = Path.Combine("logs", "micfx-.log");
        loggerConfig.WriteTo.File(
            path: logPath,
            formatter: new CompactJsonFormatter(),
            rollingInterval: RollingInterval.Day,
            retainedFileCountLimit: 31,
            fileSizeLimitBytes: 1_073_741_824); // 1GB

        return loggerConfig;
    }
}

/// <summary>
/// Basic configuration options for MicFx Serilog
/// </summary>
public class MicFxSerilogOptions
{
    /// <summary>
    /// Minimum log level (default: Information)
    /// </summary>
    public LogEventLevel MinimumLevel { get; set; } = LogEventLevel.Information;
}