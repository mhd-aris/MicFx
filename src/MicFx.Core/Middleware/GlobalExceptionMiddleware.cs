using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using System.Net;
using System.Text.Json;
using MicFx.SharedKernel.Common;
using MicFx.SharedKernel.Common.Exceptions;
using System.Diagnostics;

namespace MicFx.Core.Middleware;

/// <summary>
/// Global exception handling middleware for the MicFx Framework
/// Handles all unhandled exceptions and converts them to standard API responses
/// </summary>
public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;
    private readonly IHostEnvironment _environment;
    private readonly JsonSerializerOptions _jsonOptions;

    public GlobalExceptionMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionMiddleware> logger,
        IHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = _environment.IsDevelopment()
        };
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception exception)
        {
            await HandleExceptionAsync(context, exception);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        // Generate trace ID untuk correlation
        var traceId = Activity.Current?.Id ?? context.TraceIdentifier;

        // Extract module context dari route atau assembly
        var moduleName = ExtractModuleName(context, exception);

        // Log exception dengan full context
        LogException(exception, traceId, moduleName, context);

        // Create standardized error response
        var response = CreateErrorResponse(exception, traceId, moduleName);

        // Set HTTP response
        context.Response.StatusCode = GetHttpStatusCode(exception);
        context.Response.ContentType = "application/json";

        // Write response
        var jsonResponse = JsonSerializer.Serialize(response, _jsonOptions);
        await context.Response.WriteAsync(jsonResponse);
    }

    private void LogException(Exception exception, string traceId, string? moduleName, HttpContext context)
    {
        var logLevel = GetLogLevel(exception);
        var requestPath = context.Request.Path.Value ?? "Unknown";
        var httpMethod = context.Request.Method;
        var userAgent = context.Request.Headers.UserAgent.FirstOrDefault() ?? "Unknown";
        var remoteIp = context.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
        var statusCode = GetHttpStatusCode(exception);

        // Comprehensive structured logging dengan Serilog properties
        using var scope = _logger.BeginScope(new Dictionary<string, object>
        {
            ["TraceId"] = traceId,
            ["Module"] = moduleName ?? "Unknown",
            ["RequestPath"] = requestPath,
            ["HttpMethod"] = httpMethod,
            ["UserAgent"] = userAgent,
            ["RemoteIp"] = remoteIp,
            ["StatusCode"] = statusCode,
            ["ExceptionType"] = exception.GetType().Name,
            ["Source"] = "GlobalExceptionMiddleware"
        });

        // Create structured log properties
        var logProperties = new Dictionary<string, object>
        {
            ["TraceId"] = traceId,
            ["Module"] = moduleName ?? "Unknown",
            ["RequestPath"] = requestPath,
            ["HttpMethod"] = httpMethod,
            ["UserAgent"] = userAgent,
            ["RemoteIp"] = remoteIp,
            ["StatusCode"] = statusCode,
            ["ExceptionType"] = exception.GetType().Name,
            ["ExceptionMessage"] = exception.Message
        };

        // Add MicFx exception specific properties
        if (exception is MicFxException micFxException)
        {
            logProperties["ErrorCode"] = micFxException.ErrorCode;
            logProperties["Category"] = micFxException.Category.ToString();
            logProperties["ModuleName"] = micFxException.ModuleName ?? moduleName ?? "Unknown";

            if (micFxException.Details.Any())
            {
                logProperties["ExceptionDetails"] = micFxException.Details;
            }

            // Add validation-specific properties
            if (exception is ValidationException validationEx && validationEx.ValidationErrors.Any())
            {
                logProperties["ValidationErrors"] = validationEx.ValidationErrors.Select(ve => new
                {
                    Field = ve.Field,
                    Message = ve.Message
                }).ToList();
            }
        }

        // Add inner exception information if available
        if (exception.InnerException != null)
        {
            logProperties["InnerExceptionType"] = exception.InnerException.GetType().Name;
            logProperties["InnerExceptionMessage"] = exception.InnerException.Message;
        }

        // Log dengan appropriate level dan structured properties
        switch (logLevel)
        {
            case LogLevel.Error:
                _logger.LogError(exception,
                    "🔥 Unhandled exception in {Module}: {ExceptionType} | {HttpMethod} {RequestPath} → {StatusCode} | TraceId: {TraceId}",
                    moduleName ?? "Unknown", exception.GetType().Name, httpMethod, requestPath, statusCode, traceId);
                break;

            case LogLevel.Warning:
                _logger.LogWarning(exception,
                    "⚠️ Business exception in {Module}: {ErrorCode} | {HttpMethod} {RequestPath} → {StatusCode} | TraceId: {TraceId}",
                    moduleName ?? "Unknown",
                    (exception as MicFxException)?.ErrorCode ?? "UNKNOWN",
                    httpMethod, requestPath, statusCode, traceId);
                break;

            case LogLevel.Information:
                _logger.LogInformation(exception,
                    "ℹ️ Validation error in {Module}: {ErrorCode} | {HttpMethod} {RequestPath} → {StatusCode} | TraceId: {TraceId}",
                    moduleName ?? "Unknown",
                    (exception as MicFxException)?.ErrorCode ?? "VALIDATION_ERROR",
                    httpMethod, requestPath, statusCode, traceId);
                break;
        }

        // Log detailed properties untuk debugging
        if (_environment.IsDevelopment())
        {
            _logger.LogDebug("Exception properties: {@LogProperties}", logProperties);
        }
    }

    private ApiResponse CreateErrorResponse(Exception exception, string traceId, string? moduleName)
    {
        ApiResponse response;

        switch (exception)
        {
            case ValidationException validationEx:
                var validationErrors = validationEx.ValidationErrors
                    .Select(ve => $"{ve.Field}: {ve.Message}")
                    .ToList();

                response = ApiResponse.Error(validationEx.Message, validationErrors);
                response.Metadata = new
                {
                    ValidationErrors = validationEx.ValidationErrors,
                    ErrorCode = validationEx.ErrorCode,
                    Category = validationEx.Category.ToString()
                };
                break;

            case BusinessException businessEx:
                response = ApiResponse.Error(businessEx.Message);
                response.Metadata = new
                {
                    ErrorCode = businessEx.ErrorCode,
                    Category = businessEx.Category.ToString(),
                    Details = _environment.IsDevelopment() ? businessEx.Details : null
                };
                break;

            case ModuleException moduleEx:
                response = ApiResponse.Error(moduleEx.Message);
                response.Metadata = new
                {
                    ErrorCode = moduleEx.ErrorCode,
                    Category = moduleEx.Category.ToString(),
                    ModuleName = moduleEx.ModuleName,
                    Details = _environment.IsDevelopment() ? moduleEx.Details : null
                };
                break;

            case SecurityException securityEx:
                response = ApiResponse.Error(securityEx.Message);
                response.Metadata = new
                {
                    ErrorCode = securityEx.ErrorCode,
                    Category = securityEx.Category.ToString(),
                    Details = _environment.IsDevelopment() ? securityEx.Details : null
                };
                break;

            case ConfigurationException configEx:
                response = ApiResponse.Error(configEx.Message);
                response.Metadata = new
                {
                    ErrorCode = configEx.ErrorCode,
                    Category = configEx.Category.ToString(),
                    ModuleName = configEx.ModuleName,
                    SectionName = configEx.SectionName,
                    Details = _environment.IsDevelopment() ? configEx.Details : null
                };
                break;

            case MicFxException micFxEx:
                var errors = new List<string> { micFxEx.Message };
                if (_environment.IsDevelopment() && micFxEx.Details.Any())
                {
                    errors.AddRange(micFxEx.Details.Select(d => $"{d.Key}: {d.Value}"));
                }

                response = ApiResponse.Error(micFxEx.Message, errors);
                response.Metadata = new
                {
                    ErrorCode = micFxEx.ErrorCode,
                    Category = micFxEx.Category.ToString(),
                    Details = _environment.IsDevelopment() ? micFxEx.Details : null
                };
                break;

            default:
                // Handle standard .NET exceptions
                var message = _environment.IsDevelopment()
                    ? exception.Message
                    : "An internal server error occurred";

                var standardErrors = new List<string> { exception.Message };

                if (_environment.IsDevelopment())
                {
                    if (!string.IsNullOrEmpty(exception.StackTrace))
                    {
                        standardErrors.Add($"StackTrace: {exception.StackTrace}");
                    }

                    var innerEx = exception.InnerException;
                    while (innerEx != null)
                    {
                        standardErrors.Add($"Inner: {innerEx.Message}");
                        innerEx = innerEx.InnerException;
                    }
                }

                response = _environment.IsDevelopment()
                    ? ApiResponse.Error(message, standardErrors)
                    : ApiResponse.Error(message);
                break;
        }

        // Set common properties
        response.TraceId = traceId;
        response.Source = moduleName ?? "MicFx.Framework";
        response.Timestamp = DateTimeOffset.UtcNow;

        return response;
    }

    private string? ExtractModuleName(HttpContext context, Exception exception)
    {
        // Priority 1: MicFxException with module name
        if (exception is MicFxException micFxException && !string.IsNullOrEmpty(micFxException.ModuleName))
        {
            return micFxException.ModuleName;
        }

        // Priority 2: From route values
        if (context.Request.RouteValues.TryGetValue("controller", out var controller))
        {
            var controllerName = controller?.ToString();
            if (!string.IsNullOrEmpty(controllerName))
            {
                // Try to extract module from controller namespace or assembly
                return ExtractModuleFromController(controllerName);
            }
        }

        // Priority 3: From exception source
        var assembly = exception.TargetSite?.DeclaringType?.Assembly;
        if (assembly != null)
        {
            var assemblyName = assembly.GetName().Name ?? "";
            if (assemblyName.StartsWith("MicFx.Modules."))
            {
                var parts = assemblyName.Split('.');
                if (parts.Length >= 3)
                {
                    return parts[2]; // MicFx.Modules.HelloWorld -> HelloWorld
                }
            }
        }

        return null;
    }

    private string? ExtractModuleFromController(string controllerName)
    {
        // Remove Controller suffix
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

    private int GetHttpStatusCode(Exception exception)
    {
        return exception switch
        {
            MicFxException micFxEx => micFxEx.HttpStatusCode,
            ArgumentNullException => (int)HttpStatusCode.BadRequest,
            ArgumentException => (int)HttpStatusCode.BadRequest,
            UnauthorizedAccessException => (int)HttpStatusCode.Unauthorized,
            NotImplementedException => (int)HttpStatusCode.NotImplemented,
            TimeoutException => (int)HttpStatusCode.RequestTimeout,
            _ => (int)HttpStatusCode.InternalServerError
        };
    }

    private LogLevel GetLogLevel(Exception exception)
    {
        return exception switch
        {
            MicFxException micFxEx => micFxEx.Category switch
            {
                ErrorCategory.Business => LogLevel.Warning,
                ErrorCategory.Validation => LogLevel.Information,
                ErrorCategory.Security => LogLevel.Warning,
                ErrorCategory.Configuration => LogLevel.Error,
                ErrorCategory.Module => LogLevel.Error,
                _ => LogLevel.Error
            },
            _ => LogLevel.Error
        };
    }
}