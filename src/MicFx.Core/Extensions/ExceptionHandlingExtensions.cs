using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MicFx.Core.Middleware;
using MicFx.Core.Filters;
using MicFx.SharedKernel.Common.Exceptions;

namespace MicFx.Core.Extensions;

/// <summary>
/// Extension methods for enabling global exception handling in the MicFx Framework
/// </summary>
public static class ExceptionHandlingExtensions
{
    /// <summary>
    /// Adds MicFx exception handling services to the DI container
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="configureOptions">Configuration options for exception handling</param>
    /// <returns>Service collection for method chaining</returns>
    public static IServiceCollection AddMicFxExceptionHandling(
        this IServiceCollection services,
        Action<MicFxExceptionOptions>? configureOptions = null)
    {
        // Configure options
        var options = new MicFxExceptionOptions();
        configureOptions?.Invoke(options);
        services.AddSingleton(options);

        // Register exception filter
        services.AddScoped<ModuleExceptionFilter>();

        // Add to MVC options
        services.Configure<Microsoft.AspNetCore.Mvc.MvcOptions>(mvcOptions =>
        {
            if (options.EnableModuleExceptionFilter)
            {
                mvcOptions.Filters.Add<ModuleExceptionFilter>();
            }
        });

        return services;
    }

    /// <summary>
    /// Adds global exception middleware to the pipeline
    /// </summary>
    /// <param name="app">Application builder</param>
    /// <param name="options">Exception handling options</param>
    /// <returns>Application builder for method chaining</returns>
    public static IApplicationBuilder UseMicFxExceptionHandling(
        this IApplicationBuilder app,
        MicFxExceptionOptions? options = null)
    {
        // Get options from DI if not provided
        options ??= app.ApplicationServices.GetService<MicFxExceptionOptions>() ?? new MicFxExceptionOptions();

        if (options.EnableGlobalExceptionMiddleware)
        {
            app.UseMiddleware<GlobalExceptionMiddleware>();
        }

        return app;
    }

    /// <summary>
    /// Shortcut for adding exception handling with default options
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <returns>Service collection for method chaining</returns>
    public static IServiceCollection AddMicFxExceptionHandling(this IServiceCollection services)
    {
        return services.AddMicFxExceptionHandling(options =>
        {
            options.EnableGlobalExceptionMiddleware = true;
            options.EnableModuleExceptionFilter = true;
            options.IncludeStackTraceInDevelopment = true;
            options.LogExceptionDetails = true;
        });
    }

    /// <summary>
    /// Shortcut for using exception handling with default options
    /// </summary>
    /// <param name="app">Application builder</param>
    /// <returns>Application builder for method chaining</returns>
    public static IApplicationBuilder UseMicFxExceptionHandling(this IApplicationBuilder app)
    {
        return app.UseMicFxExceptionHandling(new MicFxExceptionOptions
        {
            EnableGlobalExceptionMiddleware = true,
            EnableModuleExceptionFilter = true,
            IncludeStackTraceInDevelopment = true,
            LogExceptionDetails = true
        });
    }
}

/// <summary>
/// Configuration options for MicFx exception handling
/// </summary>
public class MicFxExceptionOptions
{
    /// <summary>
    /// Enable global exception middleware (default: true)
    /// </summary>
    public bool EnableGlobalExceptionMiddleware { get; set; } = true;

    /// <summary>
    /// Enable module exception filter (default: true)
    /// </summary>
    public bool EnableModuleExceptionFilter { get; set; } = true;

    /// <summary>
    /// Include stack trace dalam response di development environment (default: true)
    /// </summary>
    public bool IncludeStackTraceInDevelopment { get; set; } = true;

    /// <summary>
    /// Log exception details (default: true)
    /// </summary>
    public bool LogExceptionDetails { get; set; } = true;

    /// <summary>
    /// Custom error message untuk production environment
    /// </summary>
    public string DefaultProductionErrorMessage { get; set; } = "An internal server error occurred";

    /// <summary>
    /// Minimum log level untuk exception logging
    /// </summary>
    public LogLevel MinimumLogLevel { get; set; } = LogLevel.Warning;

    /// <summary>
    /// Custom exception handlers untuk specific exception types
    /// </summary>
    public Dictionary<Type, Func<Exception, string, object>> CustomExceptionHandlers { get; set; } = new();

    /// <summary>
    /// Custom response headers untuk error responses
    /// </summary>
    public Dictionary<string, string> CustomHeaders { get; set; } = new();

    /// <summary>
    /// Enable correlation ID tracking (default: true)
    /// </summary>
    public bool EnableCorrelationId { get; set; } = true;

    /// <summary>
    /// Add custom exception handler
    /// </summary>
    /// <typeparam name="T">Exception type</typeparam>
    /// <param name="handler">Handler function</param>
    public void AddCustomExceptionHandler<T>(Func<T, string, object> handler) where T : Exception
    {
        CustomExceptionHandlers[typeof(T)] = (ex, traceId) => handler((T)ex, traceId);
    }
}

/// <summary>
/// Extension methods untuk ModuleStartupBase untuk mudah menambahkan exception handling
/// </summary>
public static class ModuleExceptionExtensions
{
    /// <summary>
    /// Helper method untuk module untuk throw business exception dengan module context
    /// </summary>
    /// <param name="moduleName">Nama module</param>
    /// <param name="message">Error message</param>
    /// <param name="errorCode">Error code</param>
    /// <returns>Business exception dengan module context</returns>
    public static BusinessException CreateBusinessException(
        string moduleName,
        string message,
        string errorCode = "BUSINESS_ERROR")
    {
        return (BusinessException)new BusinessException(message, errorCode).SetModule(moduleName);
    }

    /// <summary>
    /// Helper method untuk module untuk throw validation exception dengan module context
    /// </summary>
    /// <param name="moduleName">Nama module</param>
    /// <param name="message">Error message</param>
    /// <param name="validationErrors">Validation errors</param>
    /// <param name="errorCode">Error code</param>
    /// <returns>Validation exception dengan module context</returns>
    public static ValidationException CreateValidationException(
        string moduleName,
        string message,
        List<ValidationError> validationErrors,
        string errorCode = "VALIDATION_ERROR")
    {
        return (ValidationException)new ValidationException(message, validationErrors, errorCode).SetModule(moduleName);
    }

    /// <summary>
    /// Helper method untuk module untuk throw module exception
    /// </summary>
    /// <param name="moduleName">Nama module</param>
    /// <param name="message">Error message</param>
    /// <param name="errorCode">Error code</param>
    /// <returns>Module exception</returns>
    public static ModuleException CreateModuleException(
        string moduleName,
        string message,
        string errorCode = "MODULE_ERROR")
    {
        return new ModuleException(message, moduleName, errorCode);
    }

    /// <summary>
    /// Helper method untuk module untuk throw security exception dengan module context
    /// </summary>
    /// <param name="moduleName">Nama module</param>
    /// <param name="message">Error message</param>
    /// <param name="errorCode">Error code</param>
    /// <returns>Security exception dengan module context</returns>
    public static SecurityException CreateSecurityException(
        string moduleName,
        string message,
        string errorCode = "SECURITY_ERROR")
    {
        return (SecurityException)new SecurityException(message, errorCode).SetModule(moduleName);
    }
}