# 🛡️ MicFx Framework - Exception Handling

## 🎯 **Overview**

Exception Handling dalam MicFx Framework menyediakan sistem penanganan error yang komprehensif dengan structured responses, automatic logging, dan environment-aware error detail exposure.

---

## 🏗️ **Architecture**

### **Exception Flow**
```
Application Exception → Global Middleware → Exception Classification → Structured Response → Logging
```

---

## 🎭 **Exception Types**

### **Base Exception Class**
```csharp
using MicFx.SharedKernel.Common.Exceptions;

public abstract class MicFxException : Exception
{
    public string ErrorCode { get; }
    public Dictionary<string, object> Details { get; }
    public int HttpStatusCode { get; protected set; }

    protected MicFxException(string message, string errorCode, int httpStatusCode = 500) 
        : base(message)
    {
        ErrorCode = errorCode;
        HttpStatusCode = httpStatusCode;
        Details = new Dictionary<string, object>();
    }

    public MicFxException AddDetail(string key, object value)
    {
        Details[key] = value;
        return this;
    }
}
```

### **Specific Exception Types**
```csharp
// Business Logic Exceptions (HTTP 400)
public class BusinessException : MicFxException
{
    public BusinessException(string message, string errorCode) 
        : base(message, errorCode, 400) { }
}

// Validation Exceptions (HTTP 400)
public class ValidationException : MicFxException
{
    public List<ValidationError> ValidationErrors { get; }

    public ValidationException(string message, List<ValidationError> errors) 
        : base(message, "VALIDATION_FAILED", 400)
    {
        ValidationErrors = errors;
        AddDetail("ValidationErrors", errors);
    }
}

// Security Exceptions (HTTP 401/403)
public class SecurityException : MicFxException
{
    public SecurityException(string message, string errorCode) 
        : base(message, errorCode, 401) { }
}

// Not Found Exceptions (HTTP 404)
public class NotFoundException : MicFxException
{
    public NotFoundException(string resource, object identifier) 
        : base($"{resource} with identifier '{identifier}' was not found", "RESOURCE_NOT_FOUND", 404)
    {
        AddDetail("Resource", resource);
        AddDetail("Identifier", identifier);
    }
}
```

---

## 📋 **Standard Response Format**

### **ApiResponse Structure**
```csharp
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<string> Errors { get; set; } = new();
    public Dictionary<string, object> Details { get; set; } = new();
    public string TraceId { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public static ApiResponse<T> Ok(T data, string message = "Success")
    {
        return new ApiResponse<T>
        {
            Success = true,
            Data = data,
            Message = message
        };
    }

    public static ApiResponse<T> Error(string message, string? errorCode = null)
    {
        var response = new ApiResponse<T>
        {
            Success = false,
            Message = message,
            Errors = new List<string> { message }
        };

        if (!string.IsNullOrEmpty(errorCode))
        {
            response.Details["ErrorCode"] = errorCode;
        }

        return response;
    }
}
```

---

## 🔧 **Usage Patterns**

### **Controller Usage**
```csharp
[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<User>>> GetUser(int id)
    {
        if (id <= 0)
        {
            throw new ValidationException("Invalid user ID", new List<ValidationError>
            {
                new ValidationError { PropertyName = "id", ErrorMessage = "Must be greater than 0" }
            });
        }

        var user = await _userService.GetUserAsync(id);
        
        if (user == null)
        {
            throw new NotFoundException("User", id);
        }

        return Ok(ApiResponse<User>.Ok(user, "User retrieved successfully"));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<User>>> CreateUser([FromBody] CreateUserRequest request)
    {
        if (await _userService.EmailExistsAsync(request.Email))
        {
            throw new BusinessException("Email address is already in use", "EMAIL_ALREADY_EXISTS")
                .AddDetail("Email", request.Email);
        }

        var user = await _userService.CreateUserAsync(request);
        return Ok(ApiResponse<User>.Ok(user, "User created successfully"));
    }
}
```

---

## 🌍 **Environment-Aware Error Handling**

### **Development Environment**
```json
{
  "success": false,
  "message": "User with identifier '999' was not found",
  "errors": ["User with identifier '999' was not found"],
  "details": {
    "errorCode": "RESOURCE_NOT_FOUND",
    "resource": "User",
    "identifier": 999,
    "stackTrace": "at UserService.GetUserAsync...",
    "exceptionType": "NotFoundException"
  },
  "traceId": "abc-123-def"
}
```

### **Production Environment**
```json
{
  "success": false,
  "message": "User with identifier '999' was not found",
  "errors": ["User with identifier '999' was not found"],
  "details": {
    "errorCode": "RESOURCE_NOT_FOUND",
    "resource": "User",
    "identifier": 999
  },
  "traceId": "abc-123-def"
}
```

---

## 💡 **Best Practices**

### **Exception Design Guidelines**
1. **Use Specific Exceptions**: Create specific exception types for different scenarios
2. **Include Context**: Always add relevant context information
3. **Chain Exceptions**: Preserve original exceptions when wrapping
4. **Log Appropriately**: Different log levels for different exception types
5. **Environment Awareness**: Hide sensitive information in production

### **Good vs Bad Examples**
```csharp
// ❌ Bad - Generic exception
throw new Exception("Something went wrong");

// ✅ Good - Specific typed exception with context
throw new BusinessException("Email address is already in use", "EMAIL_ALREADY_EXISTS")
    .AddDetail("Email", request.Email);

// ❌ Bad - No context for errors
_logger.LogError(ex, "An error occurred");

// ✅ Good - Error with context
_logger.LogError(ex, "Failed to process order {OrderId} for user {UserId}", 
    orderId, userId);
```

---

## 🚨 **Troubleshooting**

### **Common Exception Issues**

| Problem | Cause | Solution |
|---------|-------|----------|
| Generic error messages | Not using typed exceptions | Use specific MicFx exception types |
| Missing context | Not adding details | Use AddDetail() method |
| Poor logging | Wrong log levels | Use appropriate log levels per exception type |
| Information disclosure | Exposing internal details | Check environment-aware response generation |

---

*Exception Handling menyediakan error management yang robust dan user-friendly untuk aplikasi MicFx.*
