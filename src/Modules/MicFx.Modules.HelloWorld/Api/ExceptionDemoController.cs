using Microsoft.AspNetCore.Mvc;
using MicFx.SharedKernel.Common;
using MicFx.SharedKernel.Common.Exceptions;

namespace MicFx.Modules.HelloWorld.Api;

/// <summary>
/// Demo controller demonstrating Global Exception Handling usage in the MicFx Framework
/// </summary>
[ApiController]
[Route("api/hello-world/exception-demo")]
public class ExceptionDemoController : ControllerBase
{
    private readonly Manifest _manifest;

    public ExceptionDemoController()
    {
        _manifest = new Manifest();
    }

    /// <summary>
    /// Success endpoint with standard ApiResponse format
    /// </summary>
    [HttpGet("success")]
    public IActionResult GetSuccess()
    {
        var data = new
        {
            Message = "Operation successful!",
            Module = _manifest.Name,
            Timestamp = DateTime.UtcNow
        };

        var response = ApiResponse<object>.Ok(data, "Data retrieved successfully");
        response.Source = _manifest.Name;
        return Ok(response);
    }

    /// <summary>
    /// Test business exception
    /// </summary>
    [HttpGet("business-error")]
    public IActionResult TestBusinessError()
    {
        throw new BusinessException("User account is suspended", "ACCOUNT_SUSPENDED")
            .SetModule(_manifest.Name)
            .AddDetail("UserId", 123)
            .AddDetail("SuspendedAt", DateTime.UtcNow.AddDays(-5));
    }

    /// <summary>
    /// Test validation exception
    /// </summary>
    [HttpGet("validation-error")]
    public IActionResult TestValidationError()
    {
        var validationErrors = new List<ValidationError>
        {
            new ValidationError { Field = "Email", Message = "Email is required" },
            new ValidationError { Field = "Email", Message = "Email format is invalid" },
            new ValidationError { Field = "Age", Message = "Age must be between 18 and 100" }
        };

        throw new ValidationException("Validation failed for user data", validationErrors, "USER_VALIDATION_ERROR")
            .SetModule(_manifest.Name);
    }

    /// <summary>
    /// Test module exception
    /// </summary>
    [HttpGet("module-error")]
    public IActionResult TestModuleError()
    {
        throw new ModuleException("Failed to initialize module dependency", _manifest.Name, "MODULE_INIT_ERROR")
            .AddDetail("DependencyName", "DatabaseConnection")
            .AddDetail("ErrorAt", DateTime.UtcNow);
    }

    /// <summary>
    /// Test security exception
    /// </summary>
    [HttpGet("security-error")]
    public IActionResult TestSecurityError()
    {
        throw new SecurityException("Insufficient permissions to access this resource", "INSUFFICIENT_PERMISSIONS")
            .SetModule(_manifest.Name)
            .AddDetail("RequiredRole", "Admin")
            .AddDetail("UserRole", "Guest");
    }

    /// <summary>
    /// Test configuration exception
    /// </summary>
    [HttpGet("config-error")]
    public IActionResult TestConfigError()
    {
        throw new ConfigurationException("Database connection string is missing", "ConnectionStrings:DefaultConnection", "CONFIG_MISSING")
            .SetModule(_manifest.Name);
    }

    /// <summary>
    /// Test standard .NET exception
    /// </summary>
    [HttpGet("system-error")]
    public IActionResult TestSystemError()
    {
        // Simulate unexpected system exception
        throw new InvalidOperationException("Database connection failed unexpectedly");
    }

    /// <summary>
    /// Test argument exception
    /// </summary>
    [HttpGet("argument-error")]
    public IActionResult TestArgumentError()
    {
        throw new ArgumentNullException("userId", "User ID cannot be null");
    }

    /// <summary>
    /// Test not implemented exception
    /// </summary>
    [HttpGet("not-implemented")]
    public IActionResult TestNotImplemented()
    {
        throw new NotImplementedException("This feature is not yet implemented");
    }

    /// <summary>
    /// Test POST endpoint with validation
    /// </summary>
    [HttpPost("validate-user")]
    public IActionResult ValidateUser([FromBody] UserModel model)
    {
        var validationErrors = new List<ValidationError>();

        if (string.IsNullOrEmpty(model.Name))
            validationErrors.Add(new ValidationError { Field = "Name", Message = "Name is required" });

        if (string.IsNullOrEmpty(model.Email))
            validationErrors.Add(new ValidationError { Field = "Email", Message = "Email is required" });
        else if (!model.Email.Contains("@"))
            validationErrors.Add(new ValidationError { Field = "Email", Message = "Email format is invalid" });

        if (model.Age < 18 || model.Age > 100)
            validationErrors.Add(new ValidationError { Field = "Age", Message = "Age must be between 18 and 100" });

        if (validationErrors.Any())
        {
            throw new ValidationException("User validation failed", validationErrors, "USER_VALIDATION_ERROR")
                .SetModule(_manifest.Name);
        }

        // Simulate business logic error
        if (model.Email == "banned@example.com")
        {
            throw new BusinessException("This email is banned from the system", "EMAIL_BANNED")
                .SetModule(_manifest.Name)
                .AddDetail("Email", model.Email)
                .AddDetail("BannedAt", DateTime.UtcNow.AddDays(-30));
        }

        var response = ApiResponse<object>.Ok(new { Message = "User is valid", User = model });
        response.Source = _manifest.Name;
        return Ok(response);
    }

    /// <summary>
    /// Test chained exceptions
    /// </summary>
    [HttpGet("chained-error")]
    public IActionResult TestChainedError()
    {
        try
        {
            // Simulate nested exception
            throw new InvalidOperationException("Inner operation failed");
        }
        catch (Exception inner)
        {
            throw new BusinessException("Outer business operation failed", inner, "CHAINED_ERROR")
                .SetModule(_manifest.Name)
                .AddDetail("Operation", "ChainedOperation");
        }
    }

    /// <summary>
    /// Test async exception
    /// </summary>
    [HttpGet("async-error")]
    public async Task<IActionResult> TestAsyncError()
    {
        await Task.Delay(100); // Simulate async operation

        throw new BusinessException("Async operation failed", "ASYNC_ERROR")
            .SetModule(_manifest.Name)
            .AddDetail("AsyncOperation", "DataProcessing");
    }
}

/// <summary>
/// Model for testing validation
/// </summary>
public class UserModel
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int Age { get; set; }
}