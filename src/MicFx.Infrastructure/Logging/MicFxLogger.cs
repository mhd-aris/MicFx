using Microsoft.Extensions.Logging;
using Serilog.Context;
using System.Diagnostics;
using System.Reflection;

namespace MicFx.Infrastructure.Logging;

/// <summary>
/// MicFx Framework logger helper that provides structured logging capabilities
/// With automatic module context and correlation ID support
/// </summary>
public static class MicFxLogger
{
    /// <summary>
    /// Creates a logger with module context for structured logging
    /// </summary>
    /// <typeparam name="T">Type requesting the logger (usually the class that will log)</typeparam>
    /// <param name="loggerFactory">Logger factory from DI</param>
    /// <returns>Configured logger with module context</returns>
    public static ILogger<T> CreateModuleLogger<T>(ILoggerFactory loggerFactory)
    {
        var logger = loggerFactory.CreateLogger<T>();
        return new ModuleLogger<T>(logger);
    }

    /// <summary>
    /// Logs with module context and correlation ID
    /// </summary>
    /// <param name="logger">Logger instance</param>
    /// <param name="logLevel">Log level</param>
    /// <param name="message">Log message</param>
    /// <param name="args">Message arguments</param>
    public static void LogWithContext(this ILogger logger, LogLevel logLevel, string message, params object[] args)
    {
        var moduleName = GetCallingModuleName();
        var correlationId = Activity.Current?.Id ?? Guid.NewGuid().ToString();

        using (LogContext.PushProperty("Module", moduleName))
        using (LogContext.PushProperty("CorrelationId", correlationId))
        {
            logger.Log(logLevel, message, args);
        }
    }

    /// <summary>
    /// Logs business operation with structured context
    /// </summary>
    /// <param name="logger">Logger instance</param>
    /// <param name="operation">Business operation name</param>
    /// <param name="properties">Additional properties</param>
    /// <param name="message">Log message</param>
    /// <param name="args">Message arguments</param>
    public static void LogBusinessOperation(this ILogger logger, string operation,
        object? properties = null, string? message = null, params object[] args)
    {
        var moduleName = GetCallingModuleName();
        var correlationId = Activity.Current?.Id ?? Guid.NewGuid().ToString();

        using (LogContext.PushProperty("Module", moduleName))
        using (LogContext.PushProperty("CorrelationId", correlationId))
        using (LogContext.PushProperty("Operation", operation))
        using (LogContext.PushProperty("OperationType", "Business"))
        {
            if (properties != null)
            {
                using (LogContext.PushProperty("OperationProperties", properties, true))
                {
                    logger.LogInformation("🔄 Business operation: {Operation} | {Message}",
                        operation, message ?? "Operation executed");
                }
            }
            else
            {
                logger.LogInformation("🔄 Business operation: {Operation} | {Message}",
                    operation, message ?? "Operation executed");
            }
        }
    }

    /// <summary>
    /// Logs performance metrics for monitoring
    /// </summary>
    /// <param name="logger">Logger instance</param>
    /// <param name="operation">Operation name</param>
    /// <param name="duration">Operation duration in milliseconds</param>
    /// <param name="properties">Additional properties</param>
    public static void LogPerformance(this ILogger logger, string operation, double duration, object? properties = null)
    {
        var moduleName = GetCallingModuleName();
        var correlationId = Activity.Current?.Id ?? Guid.NewGuid().ToString();

        using (LogContext.PushProperty("Module", moduleName))
        using (LogContext.PushProperty("CorrelationId", correlationId))
        using (LogContext.PushProperty("Operation", operation))
        using (LogContext.PushProperty("Duration", duration))
        using (LogContext.PushProperty("OperationType", "Performance"))
        {
            if (properties != null)
            {
                using (LogContext.PushProperty("PerformanceProperties", properties, true))
                {
                    logger.LogInformation("⏱️ Performance: {Operation} completed in {Duration:0.00}ms",
                        operation, duration);
                }
            }
            else
            {
                logger.LogInformation("⏱️ Performance: {Operation} completed in {Duration:0.00}ms",
                    operation, duration);
            }
        }
    }

    /// <summary>
    /// Logs security events for audit trail
    /// </summary>
    /// <param name="logger">Logger instance</param>
    /// <param name="securityEvent">Security event type</param>
    /// <param name="userId">User ID involved</param>
    /// <param name="properties">Additional security properties</param>
    /// <param name="message">Security message</param>
    public static void LogSecurity(this ILogger logger, string securityEvent, string? userId = null,
        object? properties = null, string? message = null)
    {
        var moduleName = GetCallingModuleName();
        var correlationId = Activity.Current?.Id ?? Guid.NewGuid().ToString();

        using (LogContext.PushProperty("Module", moduleName))
        using (LogContext.PushProperty("CorrelationId", correlationId))
        using (LogContext.PushProperty("SecurityEvent", securityEvent))
        using (LogContext.PushProperty("UserId", userId ?? "Anonymous"))
        using (LogContext.PushProperty("OperationType", "Security"))
        {
            if (properties != null)
            {
                using (LogContext.PushProperty("SecurityProperties", properties, true))
                {
                    logger.LogWarning("🔒 Security event: {SecurityEvent} | User: {UserId} | {Message}",
                        securityEvent, userId ?? "Anonymous", message ?? "Security event occurred");
                }
            }
            else
            {
                logger.LogWarning("🔒 Security event: {SecurityEvent} | User: {UserId} | {Message}",
                    securityEvent, userId ?? "Anonymous", message ?? "Security event occurred");
            }
        }
    }

    /// <summary>
    /// Creates a performance timer that automatically logs when disposed
    /// </summary>
    /// <param name="logger">Logger instance</param>
    /// <param name="operation">Operation name</param>
    /// <param name="properties">Additional properties</param>
    /// <returns>Disposable timer</returns>
    public static IDisposable BeginTimedOperation(this ILogger logger, string operation, object? properties = null)
    {
        return new TimedOperation(logger, operation, properties);
    }

    /// <summary>
    /// Gets module name from calling assembly
    /// </summary>
    private static string GetCallingModuleName()
    {
        try
        {
            var stackTrace = new StackTrace();
            var frames = stackTrace.GetFrames();

            foreach (var frame in frames.Skip(2)) // Skip current and immediate caller
            {
                var method = frame.GetMethod();
                if (method?.DeclaringType?.Assembly != null)
                {
                    var assemblyName = method.DeclaringType.Assembly.GetName().Name ?? "";

                    if (assemblyName.StartsWith("MicFx.Modules."))
                    {
                        var parts = assemblyName.Split('.');
                        if (parts.Length >= 3)
                        {
                            return parts[2];
                        }
                    }
                    else if (assemblyName.StartsWith("MicFx."))
                    {
                        var parts = assemblyName.Split('.');
                        if (parts.Length >= 2)
                        {
                            return parts[1];
                        }
                    }
                    else if (!assemblyName.StartsWith("System.") && !assemblyName.StartsWith("Microsoft."))
                    {
                        return assemblyName;
                    }
                }
            }

            return "Unknown";
        }
        catch
        {
            return "Unknown";
        }
    }
}

/// <summary>
/// Module-aware logger wrapper that automatically adds module context
/// </summary>
public class ModuleLogger<T> : ILogger<T>
{
    private readonly ILogger<T> _logger;
    private readonly string _moduleName;

    public ModuleLogger(ILogger<T> logger)
    {
        _logger = logger;
        _moduleName = ExtractModuleName(typeof(T));
    }

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull
    {
        return _logger.BeginScope(state);
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return _logger.IsEnabled(logLevel);
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        using (LogContext.PushProperty("Module", _moduleName))
        {
            _logger.Log(logLevel, eventId, state, exception, formatter);
        }
    }

    private static string ExtractModuleName(Type type)
    {
        var assemblyName = type.Assembly.GetName().Name ?? "";

        if (assemblyName.StartsWith("MicFx.Modules."))
        {
            var parts = assemblyName.Split('.');
            if (parts.Length >= 3)
            {
                return parts[2];
            }
        }
        else if (assemblyName.StartsWith("MicFx."))
        {
            var parts = assemblyName.Split('.');
            if (parts.Length >= 2)
            {
                return parts[1];
            }
        }

        return assemblyName;
    }
}

/// <summary>
/// Performance timer for automatic logging of operation duration
/// </summary>
internal class TimedOperation : IDisposable
{
    private readonly ILogger _logger;
    private readonly string _operation;
    private readonly object? _properties;
    private readonly Stopwatch _stopwatch;
    private bool _disposed;

    public TimedOperation(ILogger logger, string operation, object? properties)
    {
        _logger = logger;
        _operation = operation;
        _properties = properties;
        _stopwatch = Stopwatch.StartNew();
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _stopwatch.Stop();
            _logger.LogPerformance(_operation, _stopwatch.Elapsed.TotalMilliseconds, _properties);
            _disposed = true;
        }
    }
}