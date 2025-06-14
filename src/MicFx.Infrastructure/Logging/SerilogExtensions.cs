using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting.Compact;
using Serilog.Enrichers.CorrelationId;

namespace MicFx.Infrastructure.Logging;

/// <summary>
/// Serilog configuration extensions for the MicFx Framework
/// Provides consistent structured logging for all modules
/// </summary>
public static class SerilogExtensions
{
    /// <summary>
    /// Adds Serilog logging infrastructure to the MicFx Framework
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="configuration">Application configuration</param>
    /// <param name="environment">Web host environment</param>
    /// <param name="configureOptions">Optional configuration for custom settings</param>
    /// <returns>Service collection for method chaining</returns>
    public static IServiceCollection AddMicFxSerilog(
        this IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment environment,
        Action<MicFxSerilogOptions>? configureOptions = null)
    {
        // Configure options
        var options = new MicFxSerilogOptions();
        configureOptions?.Invoke(options);
        services.AddSingleton(options);

        // Create logger configuration
        var loggerConfiguration = CreateLoggerConfiguration(configuration, environment, options);

        // Create static logger for early initialization
        Log.Logger = loggerConfiguration.CreateLogger();

        // Add Serilog to DI container
        services.AddSerilog();

        // Add correlation ID support
        services.AddHttpContextAccessor();

        return services;
    }

    /// <summary>
    /// Uses Serilog logging in the pipeline
    /// </summary>
    /// <param name="app">Application builder</param>
    /// <param name="options">Serilog options</param>
    /// <returns>Application builder for method chaining</returns>
    public static IApplicationBuilder UseMicFxSerilog(
        this IApplicationBuilder app,
        MicFxSerilogOptions? options = null)
    {
        // Get options from DI if not provided
        options ??= app.ApplicationServices.GetService<MicFxSerilogOptions>() ?? new MicFxSerilogOptions();

        if (options.EnableRequestLogging)
        {
            // Use Serilog request logging
            app.UseSerilogRequestLogging(configure =>
            {
                configure.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
                configure.GetLevel = GetRequestLogLevel;
                configure.EnrichDiagnosticContext = EnrichFromRequest;
            });
        }

        return app;
    }

    /// <summary>
    /// Creates logger configuration with default MicFx settings
    /// </summary>
    private static LoggerConfiguration CreateLoggerConfiguration(
        IConfiguration configuration,
        IWebHostEnvironment environment,
        MicFxSerilogOptions options)
    {
        var loggerConfig = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .Enrich.FromLogContext()
            .Enrich.WithMachineName()
            .Enrich.WithEnvironmentName()
            .Enrich.WithProcessId()
            .Enrich.WithProcessName()
            .Enrich.WithThreadId()
            .Enrich.WithCorrelationId()
            .Enrich.With<MicFxModuleEnricher>()
            .Filter.ByExcluding(excludeFilter => excludeFilter.MessageTemplate?.Text?.Contains("/_health") == true);

        // Set minimum level
        loggerConfig.MinimumLevel.Is(options.MinimumLevel);

        // Override for specific namespaces
        loggerConfig.MinimumLevel.Override("Microsoft", LogEventLevel.Warning);
        loggerConfig.MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information);
        loggerConfig.MinimumLevel.Override("System", LogEventLevel.Warning);
        loggerConfig.MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning);

        // Console sink
        if (options.EnableConsoleSink)
        {
            if (environment.IsDevelopment())
            {
                loggerConfig.WriteTo.Console(
                    outputTemplate: options.ConsoleOutputTemplateBasic,
                    theme: Serilog.Sinks.SystemConsole.Themes.AnsiConsoleTheme.Code,
                    restrictedToMinimumLevel: options.ConsoleMinimumLevel);
            }
            else
            {
                loggerConfig.WriteTo.Console(
                    formatter: new CompactJsonFormatter(),
                    restrictedToMinimumLevel: options.ConsoleMinimumLevel);
            }
        }

        // File sink
        if (options.EnableFileSink)
        {
            var logPath = Path.Combine(options.LogFileBasePath, "micfx-.log");

            loggerConfig.WriteTo.File(
                path: logPath,
                formatter: new CompactJsonFormatter(),
                rollingInterval: options.FileRollingInterval,
                retainedFileCountLimit: options.RetainedFileCountLimit,
                fileSizeLimitBytes: options.FileSizeLimitBytes,
                restrictedToMinimumLevel: options.FileMinimumLevel);
        }

        // Seq sink for centralized logging
        if (options.EnableSeqSink && !string.IsNullOrEmpty(options.SeqServerUrl))
        {
            loggerConfig.WriteTo.Seq(
                serverUrl: options.SeqServerUrl,
                apiKey: options.SeqApiKey,
                restrictedToMinimumLevel: options.SeqMinimumLevel);
        }

        // Custom sinks
        foreach (var customSink in options.CustomSinks)
        {
            customSink(loggerConfig);
        }

        return loggerConfig;
    }

    /// <summary>
    /// Determines log level for HTTP requests
    /// </summary>
    private static LogEventLevel GetRequestLogLevel(HttpContext ctx, double _, Exception? ex)
    {
        if (ex != null) return LogEventLevel.Error;

        return ctx.Response.StatusCode switch
        {
            >= 500 => LogEventLevel.Error,
            >= 400 => LogEventLevel.Warning,
            >= 300 => LogEventLevel.Information,
            _ => LogEventLevel.Information
        };
    }

    /// <summary>
    /// Enriches diagnostic context with additional request information
    /// </summary>
    private static void EnrichFromRequest(IDiagnosticContext diagnosticContext, HttpContext httpContext)
    {
        var request = httpContext.Request;

        diagnosticContext.Set("RequestHost", request.Host.Value);
        diagnosticContext.Set("RequestScheme", request.Scheme);
        diagnosticContext.Set("UserAgent", request.Headers.UserAgent.ToString());

        var clientIp = httpContext.Connection.RemoteIpAddress?.ToString();
        if (!string.IsNullOrEmpty(clientIp))
        {
            diagnosticContext.Set("ClientIP", clientIp);
        }

        if (request.QueryString.HasValue)
        {
            diagnosticContext.Set("QueryString", request.QueryString.Value);
        }

        // Extract module information from route
        if (httpContext.Request.RouteValues.TryGetValue("controller", out var controller))
        {
            var moduleName = ExtractModuleName(controller?.ToString());
            if (!string.IsNullOrEmpty(moduleName))
            {
                diagnosticContext.Set("Module", moduleName);
            }
        }

        // Add correlation ID if available
        if (httpContext.Items.TryGetValue("CorrelationId", out var correlationId) && correlationId != null)
        {
            diagnosticContext.Set("CorrelationId", correlationId);
        }
    }

    /// <summary>
    /// Extracts module name from controller name
    /// </summary>
    private static string? ExtractModuleName(string? controllerName)
    {
        if (string.IsNullOrEmpty(controllerName))
            return null;

        var cleanName = controllerName.Replace("Controller", "");

        // Check if from MicFx module
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
        var moduleAssembly = assemblies.FirstOrDefault(a =>
            a.GetName().Name?.Contains($"MicFx.Modules.{cleanName}") == true);

        if (moduleAssembly != null)
        {
            var assemblyName = moduleAssembly.GetName().Name ?? "";
            var parts = assemblyName.Split('.');
            if (parts.Length >= 3 && parts[0] == "MicFx" && parts[1] == "Modules")
            {
                return parts[2];
            }
        }

        return cleanName;
    }
}

/// <summary>
/// Configuration options for MicFx Serilog
/// </summary>
public class MicFxSerilogOptions
{
    /// <summary>
    /// Minimum log level (default: Information)
    /// </summary>
    public LogEventLevel MinimumLevel { get; set; } = LogEventLevel.Information;

    /// <summary>
    /// Enable console logging (default: true)
    /// </summary>
    public bool EnableConsoleSink { get; set; } = true;

    /// <summary>
    /// Console minimum level (default: Information)
    /// </summary>
    public LogEventLevel ConsoleMinimumLevel { get; set; } = LogEventLevel.Information;

    /// <summary>
    /// Console output template for development
    /// </summary>
    public string ConsoleOutputTemplate { get; set; } =
        "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} <s:{SourceContext}> <m:{Module}> <t:{TraceId}> {NewLine}{Exception}";
    public string ConsoleOutputTemplateBasic { get; set; } =
        "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}  <m:{Module}> {NewLine}{Exception}";



    /// <summary>
    /// Enable file logging (default: true)
    /// </summary>
    public bool EnableFileSink { get; set; } = true;

    /// <summary>
    /// Base path for log files (default: logs)
    /// </summary>
    public string LogFileBasePath { get; set; } = "logs";

    /// <summary>
    /// File minimum level (default: Information)
    /// </summary>
    public LogEventLevel FileMinimumLevel { get; set; } = LogEventLevel.Information;

    /// <summary>
    /// File rolling interval (default: Day)
    /// </summary>
    public RollingInterval FileRollingInterval { get; set; } = RollingInterval.Day;

    /// <summary>
    /// Retained file count limit (default: 31)
    /// </summary>
    public int? RetainedFileCountLimit { get; set; } = 31;

    /// <summary>
    /// File size limit in bytes (default: 1GB)
    /// </summary>
    public long? FileSizeLimitBytes { get; set; } = 1_073_741_824; // 1GB

    /// <summary>
    /// Enable Seq logging (default: false)
    /// </summary>
    public bool EnableSeqSink { get; set; } = false;

    /// <summary>
    /// Seq server URL
    /// </summary>
    public string? SeqServerUrl { get; set; }

    /// <summary>
    /// Seq API key
    /// </summary>
    public string? SeqApiKey { get; set; }

    /// <summary>
    /// Seq minimum level (default: Information)
    /// </summary>
    public LogEventLevel SeqMinimumLevel { get; set; } = LogEventLevel.Information;

    /// <summary>
    /// Enable HTTP request logging (default: true)
    /// </summary>
    public bool EnableRequestLogging { get; set; } = true;

    /// <summary>
    /// Custom sinks configuration
    /// </summary>
    public List<Action<LoggerConfiguration>> CustomSinks { get; set; } = new();

    /// <summary>
    /// Add custom sink
    /// </summary>
    /// <param name="configureSink">Sink configuration action</param>
    public void AddCustomSink(Action<LoggerConfiguration> configureSink)
    {
        CustomSinks.Add(configureSink);
    }
}