using Microsoft.Extensions.Logging;

namespace MicFx.Abstractions.Logging;

/// <summary>
/// Interface for structured logging with MicFx framework support
/// Provides business operation logging, performance tracking, and security event logging
/// </summary>
public interface IStructuredLogger
{
    /// <summary>
    /// Logs business operation with structured context
    /// </summary>
    /// <param name="operation">Business operation name</param>
    /// <param name="properties">Additional structured properties</param>
    /// <param name="message">Optional log message</param>
    void LogBusinessOperation(string operation, object? properties = null, string? message = null);

    /// <summary>
    /// Logs performance metrics for monitoring
    /// </summary>
    /// <param name="operation">Operation name</param>
    /// <param name="duration">Operation duration in milliseconds</param>
    /// <param name="properties">Additional structured properties</param>
    void LogPerformance(string operation, double duration, object? properties = null);

    /// <summary>
    /// Logs security events for audit trail
    /// </summary>
    /// <param name="securityEvent">Security event type</param>
    /// <param name="userId">User ID involved in the security event</param>
    /// <param name="properties">Additional security properties</param>
    /// <param name="message">Security event message</param>
    void LogSecurity(string securityEvent, string? userId = null, object? properties = null, string? message = null);

    /// <summary>
    /// Creates a timed operation that automatically logs performance when disposed
    /// </summary>
    /// <param name="operation">Operation name</param>
    /// <param name="properties">Additional structured properties</param>
    /// <returns>Disposable timer that logs performance on disposal</returns>
    IDisposable BeginTimedOperation(string operation, object? properties = null);

    /// <summary>
    /// Logs with module context and correlation ID
    /// </summary>
    /// <param name="logLevel">Log level</param>
    /// <param name="message">Log message</param>
    /// <param name="args">Message arguments</param>
    void LogWithContext(LogLevel logLevel, string message, params object[] args);
}

/// <summary>
/// Generic interface for structured logger with strongly typed context
/// </summary>
/// <typeparam name="T">The type whose name is used for the logger category name</typeparam>
public interface IStructuredLogger<out T> : IStructuredLogger
{
    /// <summary>
    /// Gets the underlying ILogger instance for advanced scenarios
    /// </summary>
    ILogger<T> Logger { get; }
}

/// <summary>
/// Factory interface for creating structured loggers
/// </summary>
public interface IStructuredLoggerFactory
{
    /// <summary>
    /// Creates a structured logger for the specified type
    /// </summary>
    /// <typeparam name="T">The type whose name is used for the logger category name</typeparam>
    /// <returns>Structured logger instance</returns>
    IStructuredLogger<T> CreateLogger<T>();

    /// <summary>
    /// Creates a structured logger for the specified category name
    /// </summary>
    /// <param name="categoryName">The category name for messages produced by the logger</param>
    /// <returns>Structured logger instance</returns>
    IStructuredLogger CreateLogger(string categoryName);
} 