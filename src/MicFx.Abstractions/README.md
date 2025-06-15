# 🔗 MicFx.Abstractions - Contract Layer

## 🎯 **Peran dalam Arsitektur**

**MicFx.Abstractions** adalah **Contract Layer** dalam arsitektur MicFx Framework yang berfungsi sebagai:

- **Interface Definition**: Mendefinisikan kontrak/interface untuk layanan framework
- **Dependency Inversion**: Memungkinkan loose coupling antara modules dan implementasi
- **Clean Architecture**: Memisahkan kontrak dari implementasi konkret
- **Module Safety**: Menyediakan interfaces yang aman digunakan oleh modules

## 🏗️ **Prinsip Design**

### **1. Interface-First Design**
```csharp
// ✅ Modules hanya bergantung pada interface
public class YourService
{
    private readonly ICacheService _cache;
    private readonly IStructuredLogger<YourService> _logger;
    
    public YourService(ICacheService cache, IStructuredLogger<YourService> logger)
    {
        _cache = cache;
        _logger = logger;
    }
}
```

### **2. Implementation Agnostic**
- Modules tidak perlu tahu implementasi konkret
- Infrastructure layer menyediakan implementasi actual
- Mudah testing dengan mock implementations
- Fleksibilitas untuk mengganti implementasi

### **3. Fail-Fast Pattern**
```csharp
// Default implementations throw NotImplementedException
// Memastikan Infrastructure layer terdaftar dengan benar
internal class DefaultCacheService : ICacheService
{
    public Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class
    {
        throw new NotImplementedException("Caching implementation not registered...");
    }
}
```

---

## 📂 **Struktur Folder**

```
📦 MicFx.Abstractions/
├── 📂 Caching/                      # 💾 Cache interfaces
│   └── ICacheService.cs                 # Distributed cache operations
├── 📂 Extensions/                   # 🔧 DI Extensions  
│   └── ServiceCollectionExtensions.cs  # Service registration helpers
├── 📂 Logging/                      # 📝 Logging interfaces
│   └── IStructuredLogger.cs            # Structured logging contracts
├── 📂 Security/                     # 🔐 Security interfaces
│   └── ISecurityService.cs             # Security operations contracts
└── MicFx.Abstractions.csproj       # 📦 Project configuration
```

---

## 🔧 **Komponen Utama**

### **1. Caching (💾)**

#### **ICacheService Interface**
```csharp
public interface ICacheService
{
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class;
    Task SetAsync<T>(string key, T value, TimeSpan expiration, CancellationToken cancellationToken = default) where T : class;
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);
    Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan expiration, CancellationToken cancellationToken = default) where T : class;
}
```

**Fitur:**
- ✅ **Async Operations**: Semua operasi asynchronous
- ✅ **Generic Support**: Strongly-typed cache operations
- ✅ **Pattern Removal**: Bulk removal dengan pattern matching
- ✅ **Get-or-Set**: Factory pattern untuk lazy loading
- ✅ **Cache Options**: Flexible expiration dan priority control

**Contoh Penggunaan:**
```csharp
// Di module Anda
public class ProductService
{
    private readonly ICacheService _cache;
    
    public async Task<Product> GetProductAsync(int id)
    {
        return await _cache.GetOrSetAsync(
            key: $"product:{id}",
            factory: () => LoadProductFromDatabase(id),
            expiration: TimeSpan.FromMinutes(30)
        );
    }
}
```

### **2. Logging (📝)**

#### **IStructuredLogger Interface**
```csharp
public interface IStructuredLogger
{
    void LogBusinessOperation(string operation, object? properties = null, string? message = null);
    void LogPerformance(string operation, double duration, object? properties = null);
    void LogSecurity(string securityEvent, string? userId = null, object? properties = null, string? message = null);
    IDisposable BeginTimedOperation(string operation, object? properties = null);
}
```

**Fitur:**
- ✅ **Business Operations**: Logging operasi bisnis dengan context
- ✅ **Performance Tracking**: Built-in performance monitoring
- ✅ **Security Events**: Audit trail untuk security events
- ✅ **Timed Operations**: Automatic performance logging
- ✅ **Structured Properties**: Rich context untuk monitoring

**Contoh Penggunaan:**
```csharp
public class OrderService
{
    private readonly IStructuredLogger<OrderService> _logger;
    
    public async Task ProcessOrderAsync(Order order)
    {
        using var timer = _logger.BeginTimedOperation("ProcessOrder", new { OrderId = order.Id });
        
        _logger.LogBusinessOperation("OrderProcessing", new 
        { 
            OrderId = order.Id, 
            CustomerId = order.CustomerId,
            Amount = order.Total 
        });
        
        // Process order logic...
    }
}
```

### **3. Security (🔐)**

#### **ISecurityService Interface**
```csharp
public interface ISecurityService
{
    Task<TokenValidationResult> ValidateTokenAsync(string token, CancellationToken cancellationToken = default);
    Task<AuthorizationResult> CheckPermissionsAsync(string userId, string[] permissions, CancellationToken cancellationToken = default);
    Task LogSecurityEventAsync(SecurityEvent securityEvent, CancellationToken cancellationToken = default);
    Task<string> EncryptAsync(string data, string? keyId = null);
    string GenerateHash(string data, string? salt = null);
}
```

**Fitur:**
- ✅ **Token Validation**: JWT dan custom token validation
- ✅ **Permission Checking**: Role-based authorization
- ✅ **Security Audit**: Comprehensive security event logging
- ✅ **Encryption/Decryption**: Data protection operations
- ✅ **Hashing**: Secure password hashing dan verification

**Contoh Penggunaan:**
```csharp
public class UserController : ControllerBase
{
    private readonly ISecurityService _security;
    
    public async Task<IActionResult> GetUserData(string userId)
    {
        var authResult = await _security.CheckPermissionsAsync(
            userId, 
            new[] { "users.read", "data.access" }
        );
        
        if (!authResult.IsAuthorized)
        {
            await _security.LogSecurityEventAsync(new SecurityEvent
            {
                EventType = SecurityEventType.Authorization,
                UserId = userId,
                Result = SecurityEventResult.Failure,
                Resource = "UserData"
            });
            
            return Forbid();
        }
        
        // Return user data...
    }
}
```

### **4. Extensions (🔧)**

#### **ServiceCollectionExtensions**
```csharp
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMicFxAbstractions(this IServiceCollection services)
    {
        services.AddMicFxLoggingAbstractions();
        services.AddMicFxCachingAbstractions();
        services.AddMicFxSecurityAbstractions();
        return services;
    }
}
```

**Fitur:**
- ✅ **Easy Registration**: One-liner untuk register semua abstractions
- ✅ **Modular Registration**: Register abstraction secara individual
- ✅ **Default Implementations**: Fail-fast default implementations
- ✅ **Module Safety**: Aman digunakan oleh modules

---

## 🚀 **Cara Penggunaan**

### **1. Dalam Module**

```csharp
// Startup.cs dalam module
public class Startup : ModuleStartupBase
{
    protected override void ConfigureModuleServices(IServiceCollection services)
    {
        // Register your services yang membutuhkan abstractions
        services.AddScoped<IYourService, YourService>();
        
        // Abstractions sudah otomatis tersedia melalui DI
        // Tidak perlu manual registration di module
    }
}

// Service dalam module
public class YourService : IYourService
{
    private readonly ICacheService _cache;
    private readonly IStructuredLogger<YourService> _logger;
    private readonly ISecurityService _security;
    
    public YourService(
        ICacheService cache,
        IStructuredLogger<YourService> logger,
        ISecurityService security)
    {
        _cache = cache;
        _logger = logger;
        _security = security;
    }
    
    public async Task DoSomethingAsync()
    {
        using var timer = _logger.BeginTimedOperation("DoSomething");
        
        var data = await _cache.GetOrSetAsync(
            "some-key",
            () => LoadExpensiveData(),
            TimeSpan.FromMinutes(15)
        );
        
        _logger.LogBusinessOperation("DataProcessed", new { DataId = data.Id });
    }
}
```

### **2. Testing dengan Mocks**

```csharp
[Test]
public async Task Should_Process_Data_Successfully()
{
    // Arrange
    var mockCache = new Mock<ICacheService>();
    var mockLogger = new Mock<IStructuredLogger<YourService>>();
    var mockSecurity = new Mock<ISecurityService>();
    
    mockCache.Setup(x => x.GetOrSetAsync<Data>(
        It.IsAny<string>(),
        It.IsAny<Func<Task<Data>>>(),
        It.IsAny<TimeSpan>(),
        It.IsAny<CancellationToken>()))
        .ReturnsAsync(new Data { Id = 1 });
    
    var service = new YourService(mockCache.Object, mockLogger.Object, mockSecurity.Object);
    
    // Act
    await service.DoSomethingAsync();
    
    // Assert
    mockLogger.Verify(x => x.LogBusinessOperation(
        "DataProcessed", 
        It.IsAny<object>(), 
        It.IsAny<string>()), Times.Once);
}
```

---

## 🔄 **Integrasi dengan Framework**

### **1. Registration Flow**

```
Program.cs
    ├── AddMicFxAbstractions()          # Register interfaces dengan default impl
    ├── AddMicFxInfrastructure()        # Replace dengan real implementations  
    └── AddMicFxModules()               # Modules menggunakan interfaces
```

### **2. Dependency Chain**

```
MicFx.Web (Host)
    ├── MicFx.Infrastructure (Implementations)
    │   └── MicFx.Abstractions (Contracts)
    │       └── MicFx.SharedKernel (Common Types)
    └── MicFx.Modules.* (Consumers)
        └── MicFx.Abstractions (Contracts)
```

---

## ⚠️ **Important Notes**

### **1. Implementation Registration**
```csharp
// ❌ JANGAN lakukan ini di module
services.AddSingleton<ICacheService, SomeConcreteImplementation>();

// ✅ Infrastructure layer yang handle implementasi
// Module hanya menggunakan interface melalui constructor injection
```

### **2. Error Handling**
```csharp
// Jika melihat NotImplementedException:
// "Caching implementation not registered. Please ensure MicFx.Infrastructure is properly configured."

// Pastikan di Program.cs:
builder.Services.AddMicFxAbstractions();      // Register interfaces
builder.Services.AddMicFxInfrastructure();    // Register implementations
```

### **3. Thread Safety**
- Semua interfaces di-design untuk thread-safe usage
- Async operations menggunakan CancellationToken
- Implementasi di Infrastructure layer yang handle thread safety

---

## 🎯 **Best Practices**

### **1. Module Development**
```csharp
// ✅ Constructor injection dengan interfaces
public class ProductService
{
    public ProductService(ICacheService cache, IStructuredLogger<ProductService> logger) { }
}

// ❌ Jangan menggunakan static dependencies atau service locator
public class ProductService
{
    public void DoSomething()
    {
        var cache = ServiceLocator.Get<ICacheService>(); // JANGAN!
    }
}
```

### **2. Logging Best Practices**
```csharp
// ✅ Structured logging dengan context
_logger.LogBusinessOperation("ProductCreated", new 
{ 
    ProductId = product.Id, 
    CategoryId = product.CategoryId,
    CreatedAt = DateTime.UtcNow 
});

// ❌ Unstructured logging
_logger.LogWithContext(LogLevel.Information, "Product created with ID: " + product.Id);
```

### **3. Caching Best Practices**
```csharp
// ✅ Consistent key naming
private const string CACHE_KEY_PATTERN = "product:{0}";
var key = string.Format(CACHE_KEY_PATTERN, productId);

// ✅ Appropriate expiration times
await _cache.SetAsync(key, product, TimeSpan.FromHours(1)); // Short-lived data
await _cache.SetAsync(key, staticData, TimeSpan.FromDays(1)); // Static data
```

---

## 🔗 **Dependencies**

```xml
<PackageReference Include="Microsoft.Extensions.Logging.Abstractions" Version="8.0.0" />
<PackageReference Include="Microsoft.Extensions.DependencyInjection.Abstractions" Version="8.0.0" />
<PackageReference Include="Microsoft.Extensions.Configuration.Abstractions" Version="8.0.0" />
<ProjectReference Include="../MicFx.SharedKernel/MicFx.SharedKernel.csproj" />
```

**Peran Dependencies:**
- **Microsoft.Extensions.*** - ASP.NET Core abstractions untuk DI dan configuration
- **MicFx.SharedKernel** - Common types dan base classes yang digunakan interfaces

---

*Folder ini adalah tulang punggung contract dalam MicFx Framework. Semua modules berinteraksi dengan framework melalui interfaces yang didefinisikan di sini, memastikan loose coupling dan maintainability yang tinggi.* 