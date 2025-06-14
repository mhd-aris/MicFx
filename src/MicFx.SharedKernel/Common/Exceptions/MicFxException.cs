namespace MicFx.SharedKernel.Common.Exceptions;

/// <summary>
/// Base exception class for MicFx Framework
/// Provides additional context for module and error handling
/// </summary>
public abstract class MicFxException : Exception
{
    /// <summary>
    /// Error code that can be used for error categorization
    /// </summary>
    public string ErrorCode { get; set; }

    /// <summary>
    /// Module that caused the exception
    /// </summary>
    public string? ModuleName { get; set; }

    /// <summary>
    /// Error category (Business, Validation, Technical, etc.)
    /// </summary>
    public ErrorCategory Category { get; set; }

    /// <summary>
    /// HTTP status code appropriate for this error
    /// </summary>
    public int HttpStatusCode { get; set; }

    /// <summary>
    /// Additional details that can help with debugging
    /// </summary>
    public Dictionary<string, object> Details { get; set; } = new();

    protected MicFxException(string message, string errorCode, ErrorCategory category = ErrorCategory.Technical, int httpStatusCode = 500)
        : base(message)
    {
        ErrorCode = errorCode;
        Category = category;
        HttpStatusCode = httpStatusCode;
    }

    protected MicFxException(string message, Exception innerException, string errorCode, ErrorCategory category = ErrorCategory.Technical, int httpStatusCode = 500)
        : base(message, innerException)
    {
        ErrorCode = errorCode;
        Category = category;
        HttpStatusCode = httpStatusCode;
    }

    /// <summary>
    /// Add detail information for debugging
    /// </summary>
    public MicFxException AddDetail(string key, object value)
    {
        if (!string.IsNullOrWhiteSpace(key) && !string.IsNullOrWhiteSpace(value.ToString()))
        {
            // Add error detail to exception
            Details[key] = value;
        }

        return this;
    }

    /// <summary>
    /// Adds multiple error details to the exception
    /// </summary>
    /// <param name="details">Dictionary of error details</param>
    /// <returns>Current instance for method chaining</returns>
    public MicFxException AddDetails(Dictionary<string, string> details)
    {
        if (details?.Count > 0)
        {
            foreach (var detail in details)
            {
                if (!string.IsNullOrWhiteSpace(detail.Key) && !string.IsNullOrWhiteSpace(detail.Value))
                {
                    // Add error detail to exception
                    Details[detail.Key] = detail.Value;
                }
            }
        }

        return this;
    }

    /// <summary>
    /// Set module context
    /// </summary>
    public MicFxException SetModule(string moduleName)
    {
        ModuleName = moduleName;
        return this;
    }
}

/// <summary>
/// Error category for classification
/// </summary>
public enum ErrorCategory
{
    /// <summary>
    /// Business logic errors (validation, business rules)
    /// </summary>
    Business,

    /// <summary>
    /// Technical errors (database, network, system)
    /// </summary>
    Technical,

    /// <summary>
    /// Validation errors (input validation, format errors)
    /// </summary>
    Validation,

    /// <summary>
    /// Authorization/Authentication errors
    /// </summary>
    Security,

    /// <summary>
    /// Configuration errors
    /// </summary>
    Configuration,

    /// <summary>
    /// Module-specific errors
    /// </summary>
    Module,

    /// <summary>
    /// External service errors
    /// </summary>
    External
}

/// <summary>
/// Business logic exception
/// </summary>
public class BusinessException : MicFxException
{
    public BusinessException(string message, string errorCode = "BUSINESS_ERROR")
        : base(message, errorCode, ErrorCategory.Business, 400)
    {
    }

    public BusinessException(string message, Exception innerException, string errorCode = "BUSINESS_ERROR")
        : base(message, innerException, errorCode, ErrorCategory.Business, 400)
    {
    }
}

/// <summary>
/// Validation exception
/// </summary>
public class ValidationException : MicFxException
{
    public List<ValidationError> ValidationErrors { get; set; } = new();

    public ValidationException(string message, string errorCode = "VALIDATION_ERROR")
        : base(message, errorCode, ErrorCategory.Validation, 400)
    {
    }

    public ValidationException(string message, List<ValidationError> validationErrors, string errorCode = "VALIDATION_ERROR")
        : base(message, errorCode, ErrorCategory.Validation, 400)
    {
        ValidationErrors = validationErrors;
    }

    public ValidationException AddValidationError(string field, string message)
    {
        ValidationErrors.Add(new ValidationError { Field = field, Message = message });
        return this;
    }
}

/// <summary>
/// Module-specific exception
/// </summary>
public class ModuleException : MicFxException
{
    public ModuleException(string message, string moduleName, string errorCode = "MODULE_ERROR")
        : base(message, errorCode, ErrorCategory.Module, 500)
    {
        ModuleName = moduleName;
    }

    public ModuleException(string message, Exception innerException, string moduleName, string errorCode = "MODULE_ERROR")
        : base(message, innerException, errorCode, ErrorCategory.Module, 500)
    {
        ModuleName = moduleName;
    }
}

/// <summary>
/// Configuration exception
/// </summary>
public class ConfigurationException : MicFxException
{
    public new string ModuleName { get; }
    public string SectionName { get; }

    public ConfigurationException(string moduleName, string sectionName, string message, string errorCode = "CONFIGURATION_ERROR")
        : base(message, errorCode, ErrorCategory.Configuration, 500)
    {
        ModuleName = moduleName;
        SectionName = sectionName;
        base.ModuleName = moduleName;
        AddDetail("ModuleName", moduleName);
        AddDetail("SectionName", sectionName);
    }

    public ConfigurationException(string moduleName, string sectionName, string message, Exception innerException, string errorCode = "CONFIGURATION_ERROR")
        : base(message, innerException, errorCode, ErrorCategory.Configuration, 500)
    {
        ModuleName = moduleName;
        SectionName = sectionName;
        base.ModuleName = moduleName;
        AddDetail("ModuleName", moduleName);
        AddDetail("SectionName", sectionName);
    }
}

/// <summary>
/// Configuration validation exception
/// </summary>
public class ConfigurationValidationException : ConfigurationException
{
    public IEnumerable<string> ValidationErrors { get; }

    public ConfigurationValidationException(string moduleName, string sectionName, IEnumerable<string> validationErrors)
        : base(moduleName, sectionName, $"Configuration validation failed for module '{moduleName}' in section '{sectionName}'", "CONFIGURATION_VALIDATION_ERROR")
    {
        ValidationErrors = validationErrors;

        // Add error details to exception
        foreach (var error in validationErrors)
        {
            AddDetail("ValidationError", error);
        }
    }

    public ConfigurationValidationException(string moduleName, string sectionName, string message, IEnumerable<string> validationErrors)
        : base(moduleName, sectionName, message, "CONFIGURATION_VALIDATION_ERROR")
    {
        ValidationErrors = validationErrors;

        // Add error details to exception
        foreach (var error in validationErrors)
        {
            AddDetail("ValidationError", error);
        }
    }
}

/// <summary>
/// Security exception
/// </summary>
public class SecurityException : MicFxException
{
    public SecurityException(string message, string errorCode = "SECURITY_ERROR")
        : base(message, errorCode, ErrorCategory.Security, 401)
    {
    }

    public SecurityException(string message, Exception innerException, string errorCode = "SECURITY_ERROR")
        : base(message, innerException, errorCode, ErrorCategory.Security, 401)
    {
    }
}

/// <summary>
/// Validation error detail
/// </summary>
public class ValidationError
{
    public string Field { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public object? AttemptedValue { get; set; }
}