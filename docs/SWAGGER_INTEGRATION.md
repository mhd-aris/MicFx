# 📚 MicFx Framework - Swagger Integration

## 🎯 **Overview**

Swagger Integration dalam MicFx Framework menyediakan automatic API documentation generation dengan module-aware organization, comprehensive endpoint documentation, dan interactive API testing interface.

---

## 🏗️ **Architecture**

### **Documentation Flow**
```
Controllers → Swagger Generation → Module Organization → API Documentation → Interactive UI
```

### **Documentation Structure**
```
📖 Swagger Documentation
├── 🏠 General Information      → API metadata dan authentication
├── 📦 Module Sections          → Organized by module
│   ├── HelloWorld API          → Module-specific endpoints
│   ├── Auth API                → Authentication endpoints
│   └── User Management API     → User-related endpoints
├── 🔧 Schemas                  → Data models dan DTOs
└── 🛡️ Security Definitions    → Authentication schemes
```

---

## 🔧 **Configuration**

### **Basic Swagger Setup**
```csharp
// Program.cs
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "MicFx API",
        Version = "v1.0.0",
        Description = "MicFx Framework API Documentation",
        Contact = new OpenApiContact
        {
            Name = "MicFx Team",
            Email = "team@micfx.com",
            Url = new Uri("https://github.com/micfx/micfx")
        },
        License = new OpenApiLicense
        {
            Name = "MIT",
            Url = new Uri("https://opensource.org/licenses/MIT")
        }
    });

    // Include XML comments
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }

    // Module-specific documentation
    c.TagActionsBy(api =>
    {
        var controllerName = api.ActionDescriptor.RouteValues["controller"];
        var moduleName = ExtractModuleName(api.ActionDescriptor);
        return new[] { $"{moduleName} - {controllerName}" };
    });

    // Security definitions
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "Enter JWT Bearer token"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Configure Swagger UI
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "MicFx API v1");
        c.RoutePrefix = "swagger";
        c.DocumentTitle = "MicFx API Documentation";
        c.DefaultModelsExpandDepth(2);
        c.DefaultModelRendering(ModelRendering.Model);
        c.EnableDeepLinking();
        c.EnableFilter();
        c.EnableTryItOutByDefault();
    });
}
```

### **Module-Specific Documentation**
```csharp
/// <summary>
/// HelloWorld API Controller
/// Provides greeting and demonstration endpoints for the HelloWorld module
/// </summary>
[ApiController]
[Route("api/hello-world")]
[Tags("HelloWorld")]
[Produces("application/json")]
public class HelloWorldController : ControllerBase
{
    /// <summary>
    /// Get a greeting message
    /// </summary>
    /// <remarks>
    /// Returns a simple greeting message. This endpoint demonstrates basic API functionality.
    /// 
    /// Sample request:
    /// 
    ///     GET /api/hello-world/greet
    ///     
    /// Sample response:
    /// 
    ///     {
    ///       "success": true,
    ///       "data": "Hello from MicFx!",
    ///       "message": "Greeting retrieved successfully"
    ///     }
    /// </remarks>
    /// <returns>A greeting message</returns>
    /// <response code="200">Greeting retrieved successfully</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("greet")]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<string>>> GetGreeting()
    {
        var greeting = await _helloWorldService.GetGreetingAsync();
        return Ok(ApiResponse<string>.Ok(greeting, "Greeting retrieved successfully"));
    }

    /// <summary>
    /// Create a personalized greeting
    /// </summary>
    /// <remarks>
    /// Creates a personalized greeting message for the specified user.
    /// 
    /// Sample request:
    /// 
    ///     POST /api/hello-world/greet
    ///     {
    ///       "name": "John Doe",
    ///       "language": "en",
    ///       "includeTime": true
    ///     }
    /// </remarks>
    /// <param name="request">Greeting request parameters</param>
    /// <returns>Personalized greeting message</returns>
    /// <response code="200">Greeting created successfully</response>
    /// <response code="400">Invalid request parameters</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("greet")]
    [ProducesResponseType(typeof(ApiResponse<GreetingResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<GreetingResponse>>> CreateGreeting(
        [FromBody] CreateGreetingRequest request)
    {
        var greeting = await _helloWorldService.CreateGreetingAsync(request);
        return Ok(ApiResponse<GreetingResponse>.Ok(greeting, "Greeting created successfully"));
    }

    /// <summary>
    /// Get greeting by ID
    /// </summary>
    /// <remarks>
    /// Retrieves a specific greeting by its unique identifier.
    /// </remarks>
    /// <param name="id">Greeting unique identifier</param>
    /// <returns>Greeting details</returns>
    /// <response code="200">Greeting found</response>
    /// <response code="404">Greeting not found</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("greet/{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<GreetingResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<GreetingResponse>>> GetGreeting(
        [FromRoute] int id)
    {
        var greeting = await _helloWorldService.GetGreetingAsync(id);
        
        if (greeting == null)
        {
            throw new NotFoundException("Greeting", id);
        }

        return Ok(ApiResponse<GreetingResponse>.Ok(greeting, "Greeting retrieved successfully"));
    }
}
```

---

## 📋 **Data Models Documentation**

### **Request/Response Models**
```csharp
/// <summary>
/// Request model for creating a personalized greeting
/// </summary>
public class CreateGreetingRequest
{
    /// <summary>
    /// Name of the person to greet
    /// </summary>
    /// <example>John Doe</example>
    [Required(ErrorMessage = "Name is required")]
    [StringLength(100, MinimumLength = 1, ErrorMessage = "Name must be between 1 and 100 characters")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Language code for the greeting (ISO 639-1)
    /// </summary>
    /// <example>en</example>
    [StringLength(2, MinimumLength = 2, ErrorMessage = "Language must be a 2-character ISO code")]
    public string Language { get; set; } = "en";

    /// <summary>
    /// Whether to include current time in the greeting
    /// </summary>
    /// <example>true</example>
    public bool IncludeTime { get; set; } = false;

    /// <summary>
    /// Custom message to include in the greeting
    /// </summary>
    /// <example>Hope you have a great day!</example>
    [StringLength(500, ErrorMessage = "Custom message cannot exceed 500 characters")]
    public string? CustomMessage { get; set; }
}

/// <summary>
/// Response model containing greeting information
/// </summary>
public class GreetingResponse
{
    /// <summary>
    /// Unique identifier for the greeting
    /// </summary>
    /// <example>123</example>
    public int Id { get; set; }

    /// <summary>
    /// The generated greeting message
    /// </summary>
    /// <example>Hello, John Doe! Hope you have a great day!</example>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Language used for the greeting
    /// </summary>
    /// <example>en</example>
    public string Language { get; set; } = string.Empty;

    /// <summary>
    /// Timestamp when the greeting was created
    /// </summary>
    /// <example>2024-01-15T10:30:00.123Z</example>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Name of the person being greeted
    /// </summary>
    /// <example>John Doe</example>
    public string PersonName { get; set; } = string.Empty;

    /// <summary>
    /// Whether time was included in the greeting
    /// </summary>
    /// <example>true</example>
    public bool IncludedTime { get; set; }
}

/// <summary>
/// Standard API response wrapper
/// </summary>
/// <typeparam name="T">Type of data being returned</typeparam>
public class ApiResponse<T>
{
    /// <summary>
    /// Indicates whether the operation was successful
    /// </summary>
    /// <example>true</example>
    public bool Success { get; set; }

    /// <summary>
    /// The actual data payload
    /// </summary>
    public T? Data { get; set; }

    /// <summary>
    /// Human-readable message describing the result
    /// </summary>
    /// <example>Operation completed successfully</example>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// List of error messages if operation failed
    /// </summary>
    public List<string> Errors { get; set; } = new();

    /// <summary>
    /// Additional details about the operation
    /// </summary>
    public Dictionary<string, object> Details { get; set; } = new();

    /// <summary>
    /// Unique identifier for tracing the request
    /// </summary>
    /// <example>abc-123-def-456</example>
    public string TraceId { get; set; } = string.Empty;

    /// <summary>
    /// Timestamp when the response was generated
    /// </summary>
    /// <example>2024-01-15T10:30:00.123Z</example>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
```

---

## 🔐 **Security Documentation**

### **Authentication Schemes**
```csharp
builder.Services.AddSwaggerGen(c =>
{
    // JWT Bearer Authentication
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "Enter JWT Bearer token",
        In = ParameterLocation.Header,
        Name = "Authorization"
    });

    // API Key Authentication
    c.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.ApiKey,
        In = ParameterLocation.Header,
        Name = "X-API-Key",
        Description = "Enter API Key"
    });

    // OAuth2 Authentication
    c.AddSecurityDefinition("OAuth2", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.OAuth2,
        Flows = new OpenApiOAuthFlows
        {
            AuthorizationCode = new OpenApiOAuthFlow
            {
                AuthorizationUrl = new Uri("https://auth.micfx.com/oauth/authorize"),
                TokenUrl = new Uri("https://auth.micfx.com/oauth/token"),
                Scopes = new Dictionary<string, string>
                {
                    ["read"] = "Read access to resources",
                    ["write"] = "Write access to resources",
                    ["admin"] = "Administrative access"
                }
            }
        }
    });
});
```

### **Protected Endpoint Documentation**
```csharp
/// <summary>
/// Get user profile information
/// </summary>
/// <remarks>
/// Retrieves the profile information for the authenticated user.
/// Requires valid JWT token in Authorization header.
/// </remarks>
/// <returns>User profile information</returns>
/// <response code="200">Profile retrieved successfully</response>
/// <response code="401">Unauthorized - Invalid or missing token</response>
/// <response code="403">Forbidden - Insufficient permissions</response>
[HttpGet("profile")]
[Authorize]
[ProducesResponseType(typeof(ApiResponse<UserProfile>), StatusCodes.Status200OK)]
[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
public async Task<ActionResult<ApiResponse<UserProfile>>> GetProfile()
{
    var userId = User.GetUserId();
    var profile = await _userService.GetProfileAsync(userId);
    return Ok(ApiResponse<UserProfile>.Ok(profile));
}

/// <summary>
/// Administrative endpoint - Get all users
/// </summary>
/// <remarks>
/// Retrieves all users in the system. Requires admin role.
/// </remarks>
/// <returns>List of all users</returns>
/// <response code="200">Users retrieved successfully</response>
/// <response code="401">Unauthorized</response>
/// <response code="403">Forbidden - Admin role required</response>
[HttpGet("users")]
[Authorize(Roles = "Admin")]
[ProducesResponseType(typeof(ApiResponse<List<User>>), StatusCodes.Status200OK)]
[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
public async Task<ActionResult<ApiResponse<List<User>>>> GetAllUsers()
{
    var users = await _userService.GetAllUsersAsync();
    return Ok(ApiResponse<List<User>>.Ok(users));
}
```

---

## 🏷️ **Module Organization**

### **Tag-Based Organization**
```csharp
// Automatic module tagging
public class ModuleTagOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var actionDescriptor = context.ApiDescription.ActionDescriptor;
        var moduleName = ExtractModuleName(actionDescriptor);
        var controllerName = actionDescriptor.RouteValues["controller"];

        // Add module-based tags
        operation.Tags = new List<OpenApiTag>
        {
            new OpenApiTag 
            { 
                Name = $"{moduleName}",
                Description = $"Endpoints for {moduleName} module"
            }
        };

        // Add controller-specific tag
        if (!string.IsNullOrEmpty(controllerName))
        {
            operation.Tags.Add(new OpenApiTag
            {
                Name = $"{moduleName} - {controllerName}",
                Description = $"{controllerName} operations in {moduleName} module"
            });
        }
    }

    private string ExtractModuleName(ActionDescriptor actionDescriptor)
    {
        var namespaceParts = actionDescriptor.RouteValues["controller"]?.Split('.');
        if (namespaceParts?.Length > 2 && namespaceParts[1] == "Modules")
        {
            return namespaceParts[2].Replace("MicFx.Modules.", "");
        }
        return "General";
    }
}

// Register the filter
builder.Services.AddSwaggerGen(c =>
{
    c.OperationFilter<ModuleTagOperationFilter>();
});
```

---

## 🎨 **Custom Swagger UI**

### **UI Customization**
```csharp
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "MicFx API v1");
    c.RoutePrefix = "swagger";
    
    // Customization options
    c.DocumentTitle = "MicFx API Documentation";
    c.DefaultModelsExpandDepth(2);
    c.DefaultModelRendering(ModelRendering.Model);
    c.EnableDeepLinking();
    c.EnableFilter();
    c.EnableTryItOutByDefault();
    c.DisplayRequestDuration();
    c.DocExpansion(DocExpansion.None);
    
    // Custom CSS and JavaScript
    c.InjectStylesheet("/swagger-ui/custom.css");
    c.InjectJavascript("/swagger-ui/custom.js");
    
    // OAuth configuration
    c.OAuthClientId("micfx-swagger-ui");
    c.OAuthAppName("MicFx API");
    c.OAuthUseBasicAuthenticationWithAccessCodeGrant();
});
```

### **Custom Styling**
```css
/* wwwroot/swagger-ui/custom.css */
.swagger-ui .topbar {
    background-color: #2c3e50;
}

.swagger-ui .topbar .download-url-wrapper .download-url-button {
    background-color: #3498db;
}

.swagger-ui .info .title {
    color: #2c3e50;
}

.swagger-ui .scheme-container {
    background-color: #ecf0f1;
    box-shadow: 0 1px 2px 0 rgba(0,0,0,.15);
}

.swagger-ui .opblock.opblock-post {
    border-color: #27ae60;
}

.swagger-ui .opblock.opblock-get {
    border-color: #3498db;
}

.swagger-ui .opblock.opblock-put {
    border-color: #f39c12;
}

.swagger-ui .opblock.opblock-delete {
    border-color: #e74c3c;
}
```

---

## 📊 **API Versioning**

### **Multiple API Versions**
```csharp
builder.Services.AddSwaggerGen(c =>
{
    // Version 1
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "MicFx API",
        Version = "v1.0.0",
        Description = "MicFx Framework API - Version 1"
    });

    // Version 2
    c.SwaggerDoc("v2", new OpenApiInfo
    {
        Title = "MicFx API",
        Version = "v2.0.0",
        Description = "MicFx Framework API - Version 2"
    });

    // Version filtering
    c.DocInclusionPredicate((version, apiDesc) =>
    {
        if (!apiDesc.TryGetMethodInfo(out var methodInfo))
            return false;

        var versions = methodInfo.DeclaringType?
            .GetCustomAttributes(true)
            .OfType<ApiVersionAttribute>()
            .SelectMany(attr => attr.Versions);

        return versions?.Any(v => $"v{v}" == version) ?? false;
    });
});

app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "MicFx API v1");
    c.SwaggerEndpoint("/swagger/v2/swagger.json", "MicFx API v2");
});
```

---

## 💡 **Best Practices**

### **Documentation Guidelines**
1. **Comprehensive Comments**: Use XML documentation for all public APIs
2. **Example Values**: Provide realistic example data
3. **Error Responses**: Document all possible error responses
4. **Request/Response Models**: Use well-defined DTOs
5. **Security Information**: Clearly document authentication requirements

### **Good Documentation Examples**
```csharp
/// <summary>
/// Updates user information
/// </summary>
/// <remarks>
/// Updates the specified user's information. Only the authenticated user can update their own information,
/// or users with admin role can update any user.
/// 
/// The request must include a valid JWT token and the user ID must match the authenticated user
/// (unless the user has admin role).
/// 
/// Sample request:
/// 
///     PUT /api/users/123
///     {
///       "firstName": "John",
///       "lastName": "Doe",
///       "email": "john.doe@example.com",
///       "phoneNumber": "+1234567890"
///     }
/// 
/// Note: Email changes require email verification.
/// </remarks>
/// <param name="id">User ID to update</param>
/// <param name="request">User update information</param>
/// <returns>Updated user information</returns>
/// <response code="200">User updated successfully</response>
/// <response code="400">Invalid request data</response>
/// <response code="401">Unauthorized - Invalid or missing token</response>
/// <response code="403">Forbidden - Cannot update other users</response>
/// <response code="404">User not found</response>
/// <response code="409">Email already exists</response>
/// <response code="500">Internal server error</response>
[HttpPut("{id:int}")]
[Authorize]
[ProducesResponseType(typeof(ApiResponse<UserResponse>), StatusCodes.Status200OK)]
[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
public async Task<ActionResult<ApiResponse<UserResponse>>> UpdateUser(
    [FromRoute] int id,
    [FromBody] UpdateUserRequest request)
{
    // Implementation
}
```

---

## 🚨 **Troubleshooting**

### **Common Swagger Issues**

| Problem | Cause | Solution |
|---------|-------|----------|
| Missing XML comments | XML documentation not enabled | Enable XML documentation in project settings |
| Incomplete schemas | Missing [Required] attributes | Add proper validation attributes |
| Security not showing | Security schemes not configured | Configure security definitions |
| Wrong response types | Incorrect ProducesResponseType | Fix response type attributes |

### **XML Documentation Setup**
```xml
<!-- In .csproj file -->
<PropertyGroup>
  <GenerateDocumentationFile>true</GenerateDocumentationFile>
  <DocumentationFile>bin\$(Configuration)\$(TargetFramework)\$(AssemblyName).xml</DocumentationFile>
  <NoWarn>$(NoWarn);1591</NoWarn> <!-- Suppress missing XML comment warnings -->
</PropertyGroup>
```

---

*Swagger Integration menyediakan API documentation yang komprehensif dan interactive untuk pengembangan dan testing API MicFx.*
