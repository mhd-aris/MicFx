using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using MicFx.SharedKernel.Common;
using MicFx.SharedKernel.Common.Exceptions;
using System.Diagnostics;

namespace MicFx.Core.Filters;

/// <summary>
/// ModuleExceptionFilter is a module-level exception filter for handling exceptions that occur in controllers.
/// It works as a second line of defense before GlobalExceptionMiddleware.
/// </summary>
public class ModuleExceptionFilter : IExceptionFilter
{
    private readonly ILogger<ModuleExceptionFilter> _logger;
    private readonly IHostEnvironment _environment;

    public ModuleExceptionFilter(ILogger<ModuleExceptionFilter> logger, IHostEnvironment environment)
    {
        _logger = logger;
        _environment = environment;
    }

    public void OnException(ExceptionContext context)
    {
        // Skip if already handled
        if (context.ExceptionHandled)
            return;

        var exception = context.Exception;
        var traceId = Activity.Current?.Id ?? context.HttpContext.TraceIdentifier;
        var moduleName = ExtractModuleName(context);

        // Log exception dengan module context
        LogModuleException(exception, traceId, moduleName, context);

        // Create response berdasarkan exception type
        var response = CreateModuleErrorResponse(exception, traceId, moduleName);
        var statusCode = GetHttpStatusCode(exception);

        // Set result
        context.Result = new ObjectResult(response)
        {
            StatusCode = statusCode
        };

        // Mark as handled untuk prevent middleware dari processing
        context.ExceptionHandled = true;
    }

    private void LogModuleException(Exception exception, string traceId, string moduleName, ExceptionContext context)
    {
        var controller = context.RouteData.Values["controller"]?.ToString() ?? "Unknown";
        var action = context.RouteData.Values["action"]?.ToString() ?? "Unknown";
        var httpMethod = context.HttpContext.Request.Method;

        using var scope = _logger.BeginScope(new Dictionary<string, object>
        {
            ["TraceId"] = traceId,
            ["Module"] = moduleName,
            ["Controller"] = controller,
            ["Action"] = action,
            ["HttpMethod"] = httpMethod
        });

        switch (exception)
        {
            case ValidationException validationEx:
                _logger.LogInformation(exception,
                    "Validation error in module {Module} - {Controller}.{Action}. TraceId: {TraceId}. Errors: {@ValidationErrors}",
                    moduleName, controller, action, traceId, validationEx.ValidationErrors);
                break;

            case BusinessException businessEx:
                _logger.LogWarning(exception,
                    "Business error in module {Module} - {Controller}.{Action}. TraceId: {TraceId}. ErrorCode: {ErrorCode}",
                    moduleName, controller, action, traceId, businessEx.ErrorCode);
                break;

            case ModuleException moduleEx:
                _logger.LogError(exception,
                    "Module error in {Module} - {Controller}.{Action}. TraceId: {TraceId}. ErrorCode: {ErrorCode}",
                    moduleName, controller, action, traceId, moduleEx.ErrorCode);
                break;

            case SecurityException securityEx:
                _logger.LogWarning(exception,
                    "Security error in module {Module} - {Controller}.{Action}. TraceId: {TraceId}. ErrorCode: {ErrorCode}",
                    moduleName, controller, action, traceId, securityEx.ErrorCode);
                break;

            default:
                _logger.LogError(exception,
                    "Unhandled exception in module {Module} - {Controller}.{Action}. TraceId: {TraceId}",
                    moduleName, controller, action, traceId);
                break;
        }
    }

    private ApiResponse CreateModuleErrorResponse(Exception exception, string traceId, string moduleName)
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
                    Category = businessEx.Category.ToString()
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
                    Category = securityEx.Category.ToString()
                };
                break;

            case MicFxException micFxEx:
                response = ApiResponse.Error(micFxEx.Message);
                response.Metadata = new
                {
                    ErrorCode = micFxEx.ErrorCode,
                    Category = micFxEx.Category.ToString(),
                    Details = _environment.IsDevelopment() ? micFxEx.Details : null
                };
                break;

            default:
                // Fallback untuk standard exceptions
                var message = _environment.IsDevelopment()
                    ? exception.Message
                    : "An error occurred while processing your request";

                response = ApiResponse.Error(message);

                if (_environment.IsDevelopment())
                {
                    response.Metadata = new
                    {
                        ExceptionType = exception.GetType().Name,
                        StackTrace = exception.StackTrace
                    };
                }
                break;
        }

        // Set common properties
        response.TraceId = traceId;
        response.Source = moduleName;
        response.Timestamp = DateTimeOffset.UtcNow;

        return response;
    }

    private string ExtractModuleName(ExceptionContext context)
    {
        // Try from exception first
        if (context.Exception is MicFxException micFxException && !string.IsNullOrEmpty(micFxException.ModuleName))
        {
            return micFxException.ModuleName;
        }

        // Try from controller
        var controller = context.RouteData.Values["controller"]?.ToString();
        if (!string.IsNullOrEmpty(controller))
        {
            return ExtractModuleFromController(controller);
        }

        // Try from action descriptor
        if (context.ActionDescriptor is Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor controllerActionDescriptor)
        {
            var assembly = controllerActionDescriptor.ControllerTypeInfo.Assembly;
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

        return "Unknown";
    }

    private string ExtractModuleFromController(string controllerName)
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
            ArgumentNullException => 400,
            ArgumentException => 400,
            UnauthorizedAccessException => 401,
            NotImplementedException => 501,
            TimeoutException => 408,
            _ => 500
        };
    }
}