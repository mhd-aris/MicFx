using Microsoft.Extensions.Logging;
using MicFx.Abstractions.Logging;
using Serilog.Context;
using System.Diagnostics;

namespace MicFx.Infrastructure.Logging;

/// <summary>
/// Serilog-based implementation of IStructuredLogger
/// Provides structured logging with module context and correlation support
/// </summary>
public class StructuredLoggerImplementation : IStructuredLogger
{
    private readonly ILogger _logger;

    public StructuredLoggerImplementation(ILogger logger)
    {
        _logger = logger;
    }

    public void LogBusinessOperation(string operation, object? properties = null, string? message = null)
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
                    _logger.LogInformation("🔄 Business operation: {Operation} | {Message}",
                        operation, message ?? "Operation executed");
                }
            }
            else
            {
                _logger.LogInformation("🔄 Business operation: {Operation} | {Message}",
                    operation, message ?? "Operation executed");
            }
        }
    }

    public void LogPerformance(string operation, double duration, object? properties = null)
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
                    _logger.LogInformation("⏱️ Performance: {Operation} completed in {Duration:0.00}ms",
                        operation, duration);
                }
            }
            else
            {
                _logger.LogInformation("⏱️ Performance: {Operation} completed in {Duration:0.00}ms",
                    operation, duration);
            }
        }
    }

    public void LogSecurity(string securityEvent, string? userId = null, object? properties = null, string? message = null)
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
                    _logger.LogWarning("🔒 Security event: {SecurityEvent} | User: {UserId} | {Message}",
                        securityEvent, userId ?? "Anonymous", message ?? "Security event occurred");
                }
            }
            else
            {
                _logger.LogWarning("🔒 Security event: {SecurityEvent} | User: {UserId} | {Message}",
                    securityEvent, userId ?? "Anonymous", message ?? "Security event occurred");
            }
        }
    }

    public IDisposable BeginTimedOperation(string operation, object? properties = null)
    {
        return new TimedOperationImplementation(_logger, operation, properties);
    }

    public void LogWithContext(LogLevel logLevel, string message, params object[] args)
    {
        var moduleName = GetCallingModuleName();
        var correlationId = Activity.Current?.Id ?? Guid.NewGuid().ToString();

        using (LogContext.PushProperty("Module", moduleName))
        using (LogContext.PushProperty("CorrelationId", correlationId))
        {
            _logger.Log(logLevel, message, args);
        }
    }

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
/// Generic implementation of IStructuredLogger<T> that wraps around StructuredLoggerImplementation
/// This allows for type-safe logging while maintaining all structured logging capabilities
/// </summary>
/// <typeparam name="T">The type to create the logger for, used for category naming</typeparam>
public class StructuredLoggerImplementation<T> : IStructuredLogger<T>
{
    private readonly IStructuredLogger _logger;
    private readonly ILogger<T> _microsoftLogger;

    /// <summary>
    /// Initializes a new instance of the StructuredLoggerImplementation<T> class
    /// </summary>
    /// <param name="factory">The structured logger factory</param>
    /// <param name="microsoftLogger">The Microsoft logger for compatibility</param>
    public StructuredLoggerImplementation(IStructuredLoggerFactory factory, ILogger<T> microsoftLogger)
    {
        _logger = new StructuredLoggerImplementation(microsoftLogger);
        _microsoftLogger = microsoftLogger;
    }

    /// <summary>
    /// Gets the underlying Microsoft ILogger for compatibility
    /// </summary>
    public ILogger<T> Logger => _microsoftLogger;

    /// <summary>
    /// Logs business operation with structured context
    /// </summary>
    /// <param name="operation">Business operation name</param>
    /// <param name="properties">Additional structured properties</param>
    /// <param name="message">Optional log message</param>
    public void LogBusinessOperation(string operation, object? properties = null, string? message = null)
    {
        _logger.LogBusinessOperation(operation, properties, message);
    }

    /// <summary>
    /// Logs performance metrics for monitoring
    /// </summary>
    /// <param name="operation">Operation name</param>
    /// <param name="duration">Operation duration in milliseconds</param>
    /// <param name="properties">Additional structured properties</param>
    public void LogPerformance(string operation, double duration, object? properties = null)
    {
        _logger.LogPerformance(operation, duration, properties);
    }

    /// <summary>
    /// Logs security events for audit trail
    /// </summary>
    /// <param name="securityEvent">Security event type</param>
    /// <param name="userId">User ID involved in the security event</param>
    /// <param name="properties">Additional security properties</param>
    /// <param name="message">Security event message</param>
    public void LogSecurity(string securityEvent, string? userId = null, object? properties = null, string? message = null)
    {
        _logger.LogSecurity(securityEvent, userId, properties, message);
    }

    /// <summary>
    /// Creates a timed operation that automatically logs performance when disposed
    /// </summary>
    /// <param name="operation">Operation name</param>
    /// <param name="properties">Additional structured properties</param>
    /// <returns>Disposable timer that logs performance on disposal</returns>
    public IDisposable BeginTimedOperation(string operation, object? properties = null)
    {
        return _logger.BeginTimedOperation(operation, properties);
    }

    /// <summary>
    /// Logs with module context and correlation ID
    /// </summary>
    /// <param name="logLevel">Log level</param>
    /// <param name="message">Log message</param>
    /// <param name="args">Message arguments</param>
    public void LogWithContext(LogLevel logLevel, string message, params object[] args)
    {
        _logger.LogWithContext(logLevel, message, args);
    }
}

/// <summary>
/// Factory implementation for creating structured loggers
/// </summary>
public class StructuredLoggerFactory : IStructuredLoggerFactory
{
    private readonly ILoggerFactory _loggerFactory;

    public StructuredLoggerFactory(ILoggerFactory loggerFactory)
    {
        _loggerFactory = loggerFactory;
    }

    public IStructuredLogger<T> CreateLogger<T>()
    {
        var logger = _loggerFactory.CreateLogger<T>();
        return new StructuredLoggerImplementation<T>(this, logger);
    }

    public IStructuredLogger CreateLogger(string categoryName)
    {
        var logger = _loggerFactory.CreateLogger(categoryName);
        return new StructuredLoggerImplementation(logger);
    }
}

/// <summary>
/// Implementation of timed operation that automatically logs performance when disposed
/// </summary>
internal class TimedOperationImplementation : IDisposable
{
    private readonly ILogger _logger;
    private readonly string _operation;
    private readonly object? _properties;
    private readonly Stopwatch _stopwatch;
    private bool _disposed;

    public TimedOperationImplementation(ILogger logger, string operation, object? properties)
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
            _disposed = true;
            _stopwatch.Stop();

            var structuredLogger = new StructuredLoggerImplementation(_logger);
            structuredLogger.LogPerformance(_operation, _stopwatch.ElapsedMilliseconds, _properties);
        }
    }
} 