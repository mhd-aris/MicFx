using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MicFx.SharedKernel.Common;
using System.Diagnostics;

namespace MicFx.Modules.HelloWorld.Api;

/// <summary>
/// Demo controller for testing Serilog logging functionality
/// Demonstrates various logging patterns and structured logging
/// </summary>
[ApiController]
[Route("api/hello-world/logging-demo")]
public class LoggingDemoController : ControllerBase
{
    private readonly ILogger<LoggingDemoController> _logger;

    public LoggingDemoController(ILogger<LoggingDemoController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Demonstrates basic structured logging
    /// </summary>
    [HttpGet("basic-logging")]
    public async Task<ActionResult<ApiResponse<object>>> BasicLogging()
    {
        _logger.LogInformation("🔄 Basic logging demo started");

        // Simulate some work
        await Task.Delay(50);

        _logger.LogInformation("✅ Basic logging demo completed successfully");

        return Ok(ApiResponse<object>.Ok(new
        {
            Message = "Basic logging executed - check logs for structured output",
            Timestamp = DateTime.UtcNow,
            Module = "HelloWorld"
        }));
    }

    /// <summary>
    /// Demonstrates structured properties logging
    /// </summary>
    [HttpGet("structured-logging")]
    public async Task<ActionResult<ApiResponse<object>>> StructuredLogging()
    {
        var userId = "user123";
        var operation = "DataProcessing";
        var itemCount = 42;

        _logger.LogInformation("🔄 Processing {ItemCount} items for user {UserId} in operation {Operation}",
            itemCount, userId, operation);

        // Simulate work with structured context
        await Task.Delay(100);

        var result = new
        {
            UserId = userId,
            Operation = operation,
            ItemsProcessed = itemCount,
            ProcessingTime = "100ms",
            Status = "Completed"
        };

        _logger.LogInformation("✅ Operation {Operation} completed successfully. Result: {@Result}",
            operation, result);

        return Ok(ApiResponse<object>.Ok(result));
    }

    /// <summary>
    /// Demonstrates business operation logging using MicFx helpers
    /// </summary>
    [HttpPost("business-operation")]
    public async Task<ActionResult<ApiResponse<object>>> BusinessOperationLogging([FromBody] BusinessOperationRequest request)
    {
        var operationName = $"Process{request.OperationType}";

        _logger.LogInformation("🔄 Business operation {OperationName} started for request {RequestId} with type {OperationType}",
            operationName, request.RequestId, request.OperationType);

        // Simulate business logic with error handling
        try
        {
            await Task.Delay(200); // Simulate work

            var result = new
            {
                RequestId = request.RequestId,
                Result = $"Processed {request.OperationType} successfully",
                ProcessedAt = DateTime.UtcNow,
                Data = request.Data
            };

            _logger.LogInformation("✅ Business operation {OperationName} completed successfully for request {RequestId}. Result: {@Result}",
                operationName, request.RequestId, result);

            return Ok(ApiResponse<object>.Ok(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Business operation {Operation} failed for request {RequestId}",
                operationName, request.RequestId);
            throw;
        }
    }

    /// <summary>
    /// Demonstrates performance logging with automatic timing
    /// </summary>
    [HttpGet("performance-logging")]
    public async Task<ActionResult<ApiResponse<object>>> PerformanceLogging()
    {
        var stopwatch = Stopwatch.StartNew();
        _logger.LogInformation("🔄 Starting performance operation: {OperationType} with parameters: {@Parameters}",
            "DatabaseQuery", new { QueryType = "UserSelection", Parameters = new { Limit = 100, Offset = 0 } });

        // Simulate expensive operation
        await Task.Delay(Random.Shared.Next(100, 500));

        var result = new
        {
            RecordsFound = 42,
            QueryExecuted = true,
            OptimizationApplied = true
        };

        stopwatch.Stop();
        _logger.LogInformation("✅ Performance operation completed in {ElapsedMs}ms. Result: {@Result}",
            stopwatch.ElapsedMilliseconds, result);

        return Ok(ApiResponse<object>.Ok(result));
    }

    /// <summary>
    /// Demonstrates security event logging
    /// </summary>
    [HttpPost("security-event")]
    public async Task<ActionResult<ApiResponse<object>>> SecurityEventLogging([FromBody] SecurityEventRequest request)
    {
        _logger.LogWarning("🔒 Security event {EventType} for user {UserId} from IP {IPAddress}. Event data: {@EventData}",
            request.EventType, request.UserId, 
            Request.HttpContext.Connection.RemoteIpAddress?.ToString(),
            request.EventData);

        await Task.Delay(50);

        return Ok(ApiResponse<object>.Ok(new
        {
            EventType = request.EventType,
            UserId = request.UserId,
            Logged = true,
            Message = "Security event has been logged for audit trail"
        }));
    }

    /// <summary>
    /// Demonstrates correlation ID tracking across operations
    /// </summary>
    [HttpGet("correlation-tracking")]
    public async Task<ActionResult<ApiResponse<object>>> CorrelationTracking()
    {
        var correlationId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;

        _logger.LogInformation("🔗 Starting correlation tracking demo with ID: {CorrelationId}", correlationId);

        // Simulate multiple operations with same correlation ID
        await SimulateOperation1(correlationId);
        await SimulateOperation2(correlationId);
        await SimulateOperation3(correlationId);

        _logger.LogInformation("✅ Correlation tracking demo completed. All operations linked via: {CorrelationId}", correlationId);

        return Ok(ApiResponse<object>.Ok(new
        {
            CorrelationId = correlationId,
            OperationsCompleted = 3,
            Message = "Check logs to see how all operations are correlated"
        }));
    }

    /// <summary>
    /// Demonstrates error logging with context preservation
    /// </summary>
    [HttpGet("error-logging")]
    public async Task<ActionResult<ApiResponse<object>>> ErrorLogging()
    {
        _logger.LogInformation("🧪 Testing error logging capabilities");

        try
        {
            await Task.Delay(50);
            throw new InvalidOperationException("This is a test exception for demonstrating error logging");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Demonstration error occurred during operation {Operation} with context {@Context}",
                "ErrorLoggingDemo", new
                {
                    UserId = "demo-user",
                    Operation = "ErrorLoggingDemo",
                    Timestamp = DateTime.UtcNow,
                    RequestId = HttpContext.TraceIdentifier
                });

            // Return error response instead of throwing
            return Ok(ApiResponse<object>.Error("Demonstration error - check logs for structured error information"));
        }
    }

    /// <summary>
    /// Demonstrates different log levels
    /// </summary>
    [HttpGet("log-levels")]
    public async Task<ActionResult<ApiResponse<object>>> LogLevels()
    {
        _logger.LogTrace("🔍 Trace level: Very detailed information for debugging");
        _logger.LogDebug("🐛 Debug level: Diagnostic information useful during development");
        _logger.LogInformation("ℹ️ Information level: General application flow information");
        _logger.LogWarning("⚠️ Warning level: Something unexpected happened but app can continue");

        await Task.Delay(50);

        return Ok(ApiResponse<object>.Ok(new
        {
            Message = "Different log levels demonstrated - check console/file logs",
            LogLevels = new[] { "Trace", "Debug", "Information", "Warning" },
            Note = "Error and Critical levels demonstrated in error endpoints"
        }));
    }


    private async Task SimulateOperation1(string correlationId)
    {
        _logger.LogInformation("🔄 Operation 1 executing with correlation ID: {CorrelationId}", correlationId);
        await Task.Delay(100);
        _logger.LogInformation("✅ Operation 1 completed");
    }

    private async Task SimulateOperation2(string correlationId)
    {
        _logger.LogInformation("🔄 Operation 2 executing with correlation ID: {CorrelationId}", correlationId);
        await Task.Delay(150);
        _logger.LogInformation("✅ Operation 2 completed");
    }

    private async Task SimulateOperation3(string correlationId)
    {
        _logger.LogInformation("🔄 Operation 3 executing with correlation ID: {CorrelationId}", correlationId);
        await Task.Delay(75);
        _logger.LogInformation("✅ Operation 3 completed");
    }


    public class BusinessOperationRequest
    {
        public string RequestId { get; set; } = Guid.NewGuid().ToString();
        public string OperationType { get; set; } = string.Empty;
        public object? Data { get; set; }
    }

    public class SecurityEventRequest
    {
        public string EventType { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public object? EventData { get; set; }
    }

}