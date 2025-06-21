using Microsoft.Extensions.Logging;
using MicFx.Abstractions.Logging;
using Serilog.Context;
using System.Diagnostics;

namespace MicFx.Infrastructure.Logging;

/// <summary>
/// Simplified structured logger implementation
/// SIMPLIFIED: Removed complex stack trace analysis and excessive context pushing
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
        var correlationId = Activity.Current?.Id ?? Guid.NewGuid().ToString("N")[..8];

        using (LogContext.PushProperty("CorrelationId", correlationId))
        using (LogContext.PushProperty("OperationType", "Business"))
        {
            _logger.LogInformation("🔄 Business operation: {Operation} | {Message}", 
                operation, message ?? "Operation executed");
        }
    }

    public void LogPerformance(string operation, double duration, object? properties = null)
    {
        var correlationId = Activity.Current?.Id ?? Guid.NewGuid().ToString("N")[..8];

        using (LogContext.PushProperty("CorrelationId", correlationId))
        using (LogContext.PushProperty("OperationType", "Performance"))
        using (LogContext.PushProperty("Duration", duration))
        {
            _logger.LogInformation("⏱️ Performance: {Operation} completed in {Duration:0.00}ms", 
                operation, duration);
        }
    }

    public void LogSecurity(string securityEvent, string? userId = null, object? properties = null, string? message = null)
    {
        var correlationId = Activity.Current?.Id ?? Guid.NewGuid().ToString("N")[..8];

        using (LogContext.PushProperty("CorrelationId", correlationId))
        using (LogContext.PushProperty("OperationType", "Security"))
        using (LogContext.PushProperty("UserId", userId ?? "Anonymous"))
        {
            _logger.LogWarning("🔒 Security event: {SecurityEvent} | User: {UserId} | {Message}",
                securityEvent, userId ?? "Anonymous", message ?? "Security event occurred");
        }
    }

    public IDisposable BeginTimedOperation(string operation, object? properties = null)
    {
        return new TimedOperationImplementation(_logger, operation);
    }

    public void LogWithContext(LogLevel logLevel, string message, params object[] args)
    {
        var correlationId = Activity.Current?.Id ?? Guid.NewGuid().ToString("N")[..8];

        using (LogContext.PushProperty("CorrelationId", correlationId))
        {
            _logger.Log(logLevel, message, args);
        }
    }
}

/// <summary>
/// Simple generic structured logger implementation 
/// SIMPLIFIED: Removed unnecessary complexity
/// </summary>
/// <typeparam name="T">The type to create the logger for</typeparam>
public class StructuredLoggerImplementation<T> : IStructuredLogger<T>
{
    private readonly IStructuredLogger _logger;
    private readonly ILogger<T> _microsoftLogger;

    public StructuredLoggerImplementation(ILogger<T> microsoftLogger)
    {
        _logger = new StructuredLoggerImplementation(microsoftLogger);
        _microsoftLogger = microsoftLogger;
    }

    public ILogger<T> Logger => _microsoftLogger;

    public void LogBusinessOperation(string operation, object? properties = null, string? message = null)
    {
        _logger.LogBusinessOperation(operation, properties, message);
    }

    public void LogPerformance(string operation, double duration, object? properties = null)
    {
        _logger.LogPerformance(operation, duration, properties);
    }

    public void LogSecurity(string securityEvent, string? userId = null, object? properties = null, string? message = null)
    {
        _logger.LogSecurity(securityEvent, userId, properties, message);
    }

    public IDisposable BeginTimedOperation(string operation, object? properties = null)
    {
        return _logger.BeginTimedOperation(operation, properties);
    }

    public void LogWithContext(LogLevel logLevel, string message, params object[] args)
    {
        _logger.LogWithContext(logLevel, message, args);
    }
}

/// <summary>
/// Simple structured logger factory
/// SIMPLIFIED: Straightforward factory without complex abstractions
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
        return new StructuredLoggerImplementation<T>(logger);
    }

    public IStructuredLogger CreateLogger(string categoryName)
    {
        var logger = _loggerFactory.CreateLogger(categoryName);
        return new StructuredLoggerImplementation(logger);
    }
}

/// <summary>
/// Simple timed operation implementation
/// SIMPLIFIED: Basic stopwatch functionality only
/// </summary>
internal class TimedOperationImplementation : IDisposable
{
    private readonly ILogger _logger;
    private readonly string _operation;
    private readonly Stopwatch _stopwatch;
    private bool _disposed;

    public TimedOperationImplementation(ILogger logger, string operation)
    {
        _logger = logger;
        _operation = operation;
        _stopwatch = Stopwatch.StartNew();
    }

    public void Dispose()
    {
        if (_disposed) return;

        _stopwatch.Stop();
        var duration = _stopwatch.Elapsed.TotalMilliseconds;

        using (LogContext.PushProperty("Duration", duration))
        using (LogContext.PushProperty("OperationType", "TimedOperation"))
        {
            _logger.LogInformation("⏱️ Timed operation: {Operation} completed in {Duration:0.00}ms", 
                _operation, duration);
        }

        _disposed = true;
    }
} 