using System.Text.Json.Serialization;

namespace MicFx.SharedKernel.Common;

/// <summary>
/// Standard API response format for the MicFx Framework
/// Provides consistent structure for all responses including error handling
/// </summary>
/// <typeparam name="T">Type of data to be returned</typeparam>
public class ApiResponse<T>
{
    /// <summary>
    /// Indicates whether the operation was successful
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Human-readable message describing the result
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// The actual data payload (null for error responses)
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public T? Data { get; set; }

    /// <summary>
    /// List of error details (null for successful responses)
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IEnumerable<string>? Errors { get; set; }

    /// <summary>
    /// Additional metadata about the response
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public object? Metadata { get; set; }

    /// <summary>
    /// Timestamp when the response was generated
    /// </summary>
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Trace ID for debugging and correlation
    /// </summary>
    public string? TraceId { get; set; }

    /// <summary>
    /// Module that generated this response
    /// </summary>
    public string? Source { get; set; }

    // Factory methods for common response types
    public static ApiResponse<T> Ok(T data, string message = "Operation successful")
    {
        return new ApiResponse<T>
        {
            Success = true,
            Message = message,
            Data = data
        };
    }

    public static ApiResponse<T> Error(string message, IEnumerable<string>? errors = null)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Message = message,
            Errors = errors
        };
    }

    public static ApiResponse<T> Error(Exception exception, bool includeStackTrace = false)
    {
        var errors = new List<string> { exception.Message };

        if (includeStackTrace && !string.IsNullOrEmpty(exception.StackTrace))
        {
            errors.Add($"StackTrace: {exception.StackTrace}");
        }

        var innerEx = exception.InnerException;
        while (innerEx != null)
        {
            errors.Add($"Inner: {innerEx.Message}");
            innerEx = innerEx.InnerException;
        }

        return new ApiResponse<T>
        {
            Success = false,
            Message = "An error occurred while processing the request",
            Errors = errors
        };
    }
}

/// <summary>
/// Non-generic version for responses without data payload
/// </summary>
public class ApiResponse : ApiResponse<object>
{
    public static ApiResponse Ok(string message = "Operation successful")
    {
        return new ApiResponse
        {
            Success = true,
            Message = message
        };
    }

    public static new ApiResponse Error(string message, IEnumerable<string>? errors = null)
    {
        return new ApiResponse
        {
            Success = false,
            Message = message,
            Errors = errors
        };
    }

    public static new ApiResponse Error(Exception exception, bool includeStackTrace = false)
    {
        var errors = new List<string> { exception.Message };

        if (includeStackTrace && !string.IsNullOrEmpty(exception.StackTrace))
        {
            errors.Add($"StackTrace: {exception.StackTrace}");
        }

        return new ApiResponse
        {
            Success = false,
            Message = "An error occurred while processing the request",
            Errors = errors
        };
    }
}