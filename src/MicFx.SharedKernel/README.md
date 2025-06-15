# 🔄 MicFx.SharedKernel - Shared Components Layer

## 🎯 **Peran dalam Arsitektur**

**MicFx.SharedKernel** adalah **Shared Components Layer** dalam arsitektur MicFx Framework yang berfungsi sebagai:

- **Common Contracts**: Interface dan base classes yang digunakan di seluruh framework
- **Shared Utilities**: Common utilities dan helper classes untuk semua layers
- **Exception Framework**: Structured exception handling dengan categorization
- **Modularity Foundation**: Core interfaces untuk module system
- **Cross-Layer Communication**: Shared models dan contracts antar layers
- **Framework Primitives**: Basic building blocks untuk framework operations

## 🏗️ **Prinsip Design**

### **1. Dependency-Free Design**
```csharp
// ✅ SharedKernel tidak bergantung pada layer lain
// Hanya menggunakan .NET BCL dan ASP.NET Core primitives
public interface IModuleManifest
{
    string Name { get; }
    string Version { get; }
    string[] Dependencies { get; }
}
```

### **2. Contract-First Approach**
```csharp
// ✅ Define contracts yang akan diimplementasi di layer lain
public interface IMicFxModule
{
    void RegisterServices(IServiceCollection services);
    void MapEndpoints(IEndpointRouteBuilder endpoints);
}
```

### **3. Structured Error Handling**
```csharp
// ✅ Consistent error handling dengan categorization
public class BusinessException : MicFxException
{
    public BusinessException(string message, string errorCode = "BUSINESS_ERROR")
        : base(message, errorCode, ErrorCategory.Business, 400)
    {
    }
}
```

## 📁 **Struktur Komponen**

### **🔧 Common/** (Core Utilities)
```
Common/
├── ApiResponse.cs                    # Standard API response format
├── IConfigurationManager.cs         # Configuration management interface
├── IModuleConfiguration.cs          # Module configuration contracts
└── Exceptions/
    └── MicFxException.cs            # Exception framework dengan categorization
```

**Peran**: Common utilities dan shared contracts
- **ApiResponse<T>**: Standardized response format untuk consistency
- **Configuration Interfaces**: Contracts untuk module configuration management
- **Exception Framework**: Structured exception handling dengan categories
- **Validation Support**: Built-in validation dengan Data Annotations

### **🔌 Interfaces/** (Cross-Layer Contracts)
```
Interfaces/
└── IAdminNavContributor.cs          # Admin navigation contribution interface
```

**Peran**: Interface contracts untuk cross-layer communication
- **Admin Integration**: Interface untuk modules contribute ke admin panel
- **Navigation System**: Structured navigation item management
- **Role-Based Access**: Built-in role-based navigation filtering

### **🏗️ Modularity/** (Module System Foundation)
```
Modularity/
├── IMicFxModule.cs                  # Core module interface
├── IModuleManifest.cs               # Module metadata interface
├── ModuleInfo.cs                    # Module information model
└── ModuleState.cs                   # Module lifecycle management
```

**Peran**: Foundation untuk module system
- **Module Contracts**: Core interfaces untuk module implementation
- **Lifecycle Management**: Module state dan lifecycle events
- **Dependency Management**: Module dependency resolution contracts
- **Health Monitoring**: Module health check interfaces
- **Hot Reload Support**: Runtime module reload capabilities

## 🎯 **Key Components Deep Dive**

### **📋 ApiResponse Framework**
```csharp
// Standard response format untuk consistency
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public T? Data { get; set; }
    public IEnumerable<string>? Errors { get; set; }
    public DateTimeOffset Timestamp { get; set; }
    public string? TraceId { get; set; }
    public string? Source { get; set; }
}

// Factory methods untuk common scenarios
var success = ApiResponse<User>.Ok(user, "User retrieved successfully");
var error = ApiResponse<User>.Error("User not found", new[] { "Invalid user ID" });
```

**Features**:
- **Consistent Structure**: Same format untuk semua API responses
- **Error Details**: Structured error information
- **Tracing Support**: Built-in trace ID untuk debugging
- **Module Context**: Source module identification
- **Factory Methods**: Easy creation untuk common scenarios

### **🚨 Exception Framework**
```csharp
// Base exception dengan rich context
public abstract class MicFxException : Exception
{
    public string ErrorCode { get; set; }
    public string? ModuleName { get; set; }
    public ErrorCategory Category { get; set; }
    public int HttpStatusCode { get; set; }
    public Dictionary<string, object> Details { get; set; }
}

// Specialized exceptions
public class BusinessException : MicFxException { }
public class ValidationException : MicFxException { }
public class ConfigurationException : MicFxException { }
public class SecurityException : MicFxException { }
```

**Categories**:
- **Business**: Business logic violations (400)
- **Validation**: Input validation errors (400)
- **Technical**: System/infrastructure errors (500)
- **Security**: Authentication/authorization errors (401/403)
- **Configuration**: Configuration-related errors (500)
- **Module**: Module-specific errors (500)

### **🔧 Module System Foundation**
```csharp
// Core module interface
public interface IMicFxModule
{
    void RegisterServices(IServiceCollection services);
    void MapEndpoints(IEndpointRouteBuilder endpoints);
}

// Module manifest dengan rich metadata
public interface IModuleManifest
{
    string Name { get; }
    string Version { get; }
    string[] Dependencies { get; }
    string[] OptionalDependencies { get; }
    int Priority { get; }
    bool IsCritical { get; }
    bool SupportsHotReload { get; }
}

// Module lifecycle management
public enum ModuleState
{
    NotLoaded, Loading, Loaded, Starting, Started,
    Stopping, Stopped, Error, Reloading
}
```

**Capabilities**:
- **Dependency Management**: Required dan optional dependencies
- **Lifecycle Events**: Complete module lifecycle hooks
- **Health Monitoring**: Built-in health check interfaces
- **Hot Reload**: Runtime module reload support
- **Priority System**: Module loading order management

### **⚙️ Configuration Management**
```csharp
// Configuration interface dengan validation
public interface IModuleConfiguration<T> : IModuleConfiguration where T : class
{
    T Value { get; set; }
    ValidationResult ValidateValue(T value);
}

// Configuration manager interface
public interface IMicFxConfigurationManager
{
    void RegisterModuleConfiguration<T>(IModuleConfiguration<T> configuration);
    IModuleConfiguration<T>? GetModuleConfiguration<T>();
    ValidationResult ValidateAllConfigurations();
    Task ReloadConfigurationsAsync();
}
```

**Features**:
- **Type-Safe Configuration**: Strongly-typed configuration classes
- **Validation Support**: Built-in validation dengan Data Annotations
- **Hot Reload**: Runtime configuration reload
- **Change Notifications**: Configuration change events

## 🔄 **Integration Patterns**

### **1. Module Implementation Pattern**
```csharp
// Module implementation menggunakan SharedKernel contracts
public class HelloWorldModule : IMicFxModule
{
    public void RegisterServices(IServiceCollection services)
    {
        services.AddScoped<IHelloWorldService, HelloWorldService>();
    }
    
    public void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/hello", async (IHelloWorldService service) =>
        {
            try
            {
                var result = await service.GetGreetingAsync();
                return Results.Ok(ApiResponse<string>.Ok(result));
            }
            catch (BusinessException ex)
            {
                return Results.BadRequest(ApiResponse<string>.Error(ex.Message));
            }
        });
    }
}
```

### **2. Exception Handling Pattern**
```csharp
// Service layer menggunakan structured exceptions
public class HelloWorldService : IHelloWorldService
{
    public async Task<string> GetGreetingAsync(string? name = null)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ValidationException("Name is required")
                .AddDetail("Field", "name")
                .SetModule("HelloWorld");
        }
        
        if (name.Length > 50)
        {
            throw new BusinessException("Name too long", "NAME_TOO_LONG")
                .AddDetail("MaxLength", 50)
                .AddDetail("ActualLength", name.Length)
                .SetModule("HelloWorld");
        }
        
        return $"Hello, {name}!";
    }
}
```

### **3. Configuration Pattern**
```csharp
// Module configuration implementation
public class HelloWorldConfiguration : IModuleConfiguration<HelloWorldSettings>
{
    public string ModuleName => "HelloWorld";
    public string SectionName => "HelloWorld";
    public bool IsRequired => true;
    public HelloWorldSettings Value { get; set; } = new();
    
    public ValidationResult Validate()
    {
        return ValidateValue(Value);
    }
    
    public ValidationResult ValidateValue(HelloWorldSettings value)
    {
        var context = new ValidationContext(value);
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(value, context, results, true);
        
        return results.Count == 0 
            ? ValidationResult.Success! 
            : new ValidationResult(string.Join(", ", results.Select(r => r.ErrorMessage)));
    }
}
```

## 🚀 **Usage Examples**

### **API Response Usage**
```csharp
// Controller action dengan standard response
[HttpGet("{id}")]
public async Task<ActionResult<ApiResponse<User>>> GetUser(int id)
{
    try
    {
        var user = await _userService.GetByIdAsync(id);
        return Ok(ApiResponse<User>.Ok(user, "User retrieved successfully"));
    }
    catch (BusinessException ex)
    {
        return BadRequest(ApiResponse<User>.Error(ex.Message, ex.Details.Values.Select(v => v.ToString())));
    }
    catch (Exception ex)
    {
        return StatusCode(500, ApiResponse<User>.Error(ex, includeStackTrace: _environment.IsDevelopment()));
    }
}
```

### **Admin Navigation Contribution**
```csharp
// Module contributing to admin navigation
public class HelloWorldAdminNavContributor : IAdminNavContributor
{
    public IEnumerable<AdminNavItem> GetNavItems()
    {
        return new[]
        {
            new AdminNavItem
            {
                Title = "Hello World",
                Url = "/admin/helloworld",
                Icon = "fas fa-globe",
                Category = "Modules",
                Order = 100,
                RequiredRoles = new[] { "Admin", "HelloWorldManager" }
            }
        };
    }
}
```

## 🔗 **Dependencies**

### **Framework Dependencies**
```xml
<ItemGroup>
  <FrameworkReference Include="Microsoft.AspNetCore.App" />
</ItemGroup>
```

**No External Dependencies**: SharedKernel hanya bergantung pada .NET BCL dan ASP.NET Core framework references untuk menjaga independence.

## 📈 **Performance Considerations**

- **Lightweight Design**: Minimal overhead untuk shared components
- **Lazy Initialization**: Components loaded on-demand
- **Memory Efficient**: Minimal memory footprint
- **Exception Performance**: Structured exceptions dengan minimal overhead
- **Configuration Caching**: Efficient configuration access patterns

## 🎯 **Best Practices**

### **✅ DO**
- Use `ApiResponse<T>` untuk consistent API responses
- Implement proper exception categorization dengan `MicFxException`
- Follow module contracts dari `IMicFxModule`
- Use structured configuration dengan validation
- Implement proper lifecycle management untuk modules

### **❌ DON'T**
- Jangan add external dependencies ke SharedKernel
- Jangan couple SharedKernel dengan specific implementations
- Jangan ignore exception categorization
- Jangan skip configuration validation
- Jangan bypass module lifecycle events

---

> **💡 Tip**: SharedKernel adalah foundation dari seluruh framework. Semua komponen di sini harus **dependency-free** dan **implementation-agnostic** untuk memastikan reusability dan maintainability di seluruh layers. 