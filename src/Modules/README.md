# 🧩 MicFx.Modules - Modular System Architecture

## 🎯 **Peran dalam Arsitektur**

**MicFx.Modules** adalah **Modular System Layer** dalam arsitektur MicFx Framework yang berfungsi sebagai:

- **Module Container**: Tempat semua modules aplikasi dengan struktur yang konsisten
- **Feature Isolation**: Setiap module adalah unit fungsional yang independent
- **Plugin Architecture**: Sistem plugin yang mendukung hot-reload dan dynamic loading
- **Business Logic Encapsulation**: Setiap module mengenkapsulasi business logic spesifik
- **Scalable Development**: Memungkinkan development parallel oleh multiple teams
- **Deployment Flexibility**: Module dapat di-deploy secara independent

## 🏗️ **Prinsip Design**

### **1. Module Independence**
```csharp
// ✅ Setiap module adalah unit yang independent
public class AuthModule : ModuleStartupBase
{
    public override IModuleManifest Manifest { get; } = new AuthManifest();
    
    protected override void ConfigureModuleServices(IServiceCollection services)
    {
        // Module-specific services
        services.AddScoped<IAuthService, AuthService>();
    }
}
```

### **2. Convention-Based Structure**
```
MicFx.Modules.{ModuleName}/
├── Api/                    # API Controllers (auto-routed)
├── Areas/Admin/           # Admin area controllers & views
├── Controllers/           # MVC Controllers
├── Views/                 # Razor views
├── Domain/               # Entities, DTOs, Exceptions
├── Services/             # Business logic services
├── Data/                 # DbContext, Migrations
├── Manifest.cs           # Module metadata
├── Startup.cs            # Module configuration
└── README.md             # Module documentation
```

### **3. Auto-Discovery Pattern**
```csharp
// Framework otomatis menemukan dan load modules
[assembly: MicFxModule(typeof(AuthStartup))]

// Auto-routing berdasarkan folder structure
// Api/AuthController.cs → /api/auth/*
// Areas/Admin/Controllers/UserController.cs → /admin/auth/users/*
```

## 📁 **Struktur Modules**

### **🔐 MicFx.Modules.Auth - Authentication & Authorization**
```
MicFx.Modules.Auth/
├── 📁 Api/                          # REST API endpoints
│   └── AuthController.cs            # /api/auth/* endpoints
├── 📁 Areas/Admin/                  # Admin management interface
│   ├── Controllers/
│   │   ├── DashboardController.cs   # Auth dashboard
│   │   ├── UserManagementController.cs
│   │   └── RoleManagementController.cs
│   └── Views/                       # Admin UI views
├── 📁 Controllers/                  # MVC Controllers
│   └── AuthController.cs            # /auth/* pages
├── 📁 Domain/                       # Business entities
│   ├── Entities/                    # User, Role, Permission
│   ├── DTOs/                        # Data transfer objects
│   └── Exceptions/                  # Auth-specific exceptions
├── 📁 Services/                     # Business logic
│   ├── AuthService.cs               # Core auth operations
│   ├── AuthDatabaseInitializer.cs   # DB setup
│   └── AuthHealthCheck.cs           # Health monitoring
├── 📁 Data/                         # Data access
│   ├── AuthDbContext.cs             # Entity Framework context
│   └── Migrations/                  # Database migrations
└── 📄 Configuration files
    ├── Manifest.cs                  # Module metadata
    └── Startup.cs                   # Module bootstrap
```

**Capabilities:**
- 🔐 **Authentication**: Login/logout, session management
- 👥 **User Management**: CRUD operations, profile management
- 🛡️ **Authorization**: Role-based access control (RBAC)
- 🔑 **Permission System**: Granular permission management
- 📊 **Admin Interface**: Complete admin dashboard
- 🏥 **Health Monitoring**: Database and service health checks

### **👋 MicFx.Modules.HelloWorld - Demo & Learning Module**
```
MicFx.Modules.HelloWorld/
├── 📁 Api/                          # REST API examples
│   ├── HelloWorldController.cs      # Basic API patterns
│   ├── DemoController.cs            # Routing & conventions
│   ├── ExceptionDemoController.cs   # Error handling examples
│   └── LoggingDemoController.cs     # Logging patterns
├── 📁 Areas/Admin/                  # Admin integration example
│   └── Controllers/
│       └── HelloWorldController.cs  # Admin dashboard example
├── 📁 Controllers/                  # MVC examples
│   └── HelloWorldController.cs      # Page controllers
├── 📁 Domain/                       # Domain modeling
│   └── HelloWorldEntities.cs        # Entity examples
├── 📁 Services/                     # Service layer
│   └── HelloWorldService.cs         # Business logic example
└── 📄 Module files
    ├── Manifest.cs                  # Module definition
    └── Startup.cs                   # Service registration
```

**Learning Features:**
- 🎯 **API Patterns**: RESTful API best practices
- 🔄 **Auto-Routing**: Convention-based routing examples
- ⚠️ **Exception Handling**: Comprehensive error scenarios
- 📝 **Logging**: Structured logging demonstrations
- 🎨 **Admin Integration**: Admin panel contribution
- 📊 **Health Checks**: Module health monitoring

## 🚀 **Module Development Workflow**

### **1. Module Creation**
```bash
# Struktur module baru
mkdir src/Modules/MicFx.Modules.YourModule
cd src/Modules/MicFx.Modules.YourModule

# File wajib
touch Manifest.cs Startup.cs README.md
mkdir Api Controllers Domain Services
```

### **2. Manifest Definition**
```csharp
public class YourModuleManifest : ModuleManifestBase
{
    public override string Name => "YourModule";
    public override string Version => "1.0.0";
    public override string Description => "Module description";
    public override string[] Dependencies => new[] { "Auth" }; // Optional
    public override int Priority => 100; // Loading order
    public override bool IsCritical => false; // System critical?
}
```

### **3. Startup Configuration**
```csharp
public class YourModuleStartup : ModuleStartupBase
{
    public override IModuleManifest Manifest { get; } = new YourModuleManifest();
    
    protected override void ConfigureModuleServices(IServiceCollection services)
    {
        // Register module services
        services.AddScoped<IYourService, YourService>();
        
        // Add DbContext if needed
        services.AddDbContext<YourDbContext>(options => 
            options.UseSqlServer(connectionString));
    }
}
```

### **4. Auto-Discovery Registration**
```csharp
// Di AssemblyInfo.cs atau Startup.cs
[assembly: MicFxModule(typeof(YourModuleStartup))]
```

## 🔄 **Module Lifecycle**

### **1. Discovery Phase**
```
Framework Startup
    ↓
Scan Assemblies for [MicFxModule]
    ↓
Load Module Manifests
    ↓
Validate Dependencies
    ↓
Sort by Priority
```

### **2. Registration Phase**
```
ConfigureModuleServices()
    ↓
Register Services to DI Container
    ↓
Configure Module-specific Settings
    ↓
Setup Database Contexts
```

### **3. Configuration Phase**
```
ConfigureModuleEndpoints()
    ↓
Register Routes & Endpoints
    ↓
Setup Middleware Pipeline
    ↓
Configure Admin Navigation
```

### **4. Runtime Phase**
```
Module Health Checks
    ↓
Hot Reload Support (if enabled)
    ↓
Dynamic Service Resolution
    ↓
Request Processing
```

## 🎯 **Module Patterns**

### **1. API-First Module**
```csharp
// Fokus pada REST API
[ApiController]
[Route("api/your-module")]
public class YourModuleApiController : ControllerBase
{
    [HttpGet("data")]
    public async Task<ApiResponse<Data>> GetData() { }
}
```

### **2. Full-Stack Module**
```csharp
// API + MVC + Admin
public class YourModuleController : Controller  // MVC
public class YourModuleApiController : ControllerBase  // API
public class AdminYourModuleController : Controller  // Admin
```

### **3. Service-Only Module**
```csharp
// Hanya services tanpa controllers
public class BackgroundServiceModule : ModuleStartupBase
{
    protected override void ConfigureModuleServices(IServiceCollection services)
    {
        services.AddHostedService<YourBackgroundService>();
    }
}
```

## 🔧 **Module Integration**

### **1. Cross-Module Communication**
```csharp
// Via Dependency Injection
public class YourService
{
    private readonly IAuthService _authService; // From Auth module
    
    public YourService(IAuthService authService)
    {
        _authService = authService;
    }
}
```

### **2. Admin Navigation Contribution**
```csharp
public class YourModuleAdminNavContributor : IAdminNavContributor
{
    public IEnumerable<AdminNavItem> GetNavItems()
    {
        return new[]
        {
            new AdminNavItem
            {
                Title = "Your Module",
                Url = "/admin/your-module",
                Icon = "module-icon",
                Order = 50
            }
        };
    }
}
```

### **3. Health Check Integration**
```csharp
public class YourModuleHealthCheck : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context, 
        CancellationToken cancellationToken = default)
    {
        // Check module health
        return HealthCheckResult.Healthy("Module is running");
    }
}
```

## 📋 **Best Practices**

### **✅ DO**
- Gunakan naming convention `MicFx.Modules.{ModuleName}`
- Implementasikan proper error handling dengan custom exceptions
- Sediakan comprehensive logging dengan structured data
- Buat unit tests untuk business logic
- Dokumentasikan API endpoints dengan Swagger annotations
- Implementasikan health checks untuk monitoring

### **❌ DON'T**
- Jangan buat tight coupling antar modules
- Jangan hardcode configuration values
- Jangan skip validation pada input data
- Jangan expose internal implementation details
- Jangan lupa handle database migrations
- Jangan abaikan security considerations

## 🔍 **Module Discovery & Loading**

Framework menggunakan **Convention over Configuration** untuk:

1. **Auto-Discovery**: Scan assemblies untuk `[MicFxModule]` attribute
2. **Auto-Routing**: Route berdasarkan folder dan class names
3. **Auto-Registration**: Services di-register otomatis ke DI container
4. **Auto-Configuration**: Database contexts dan middleware di-setup otomatis

## 🎨 **Admin Integration**

Setiap module dapat berkontribusi ke admin panel:

- **Navigation Items**: Via `IAdminNavContributor`
- **Dashboard Widgets**: Via admin area controllers
- **Management Pages**: Via admin area views
- **Health Status**: Via `IHealthCheck` implementation

## 🔄 **Hot Reload Support**

Modules yang mendukung hot reload:
- Dapat di-reload tanpa restart aplikasi
- Configuration changes applied dynamically
- Service registrations updated on-the-fly
- Ideal untuk development environment

---

**MicFx.Modules** menyediakan arsitektur modular yang powerful dan fleksibel, memungkinkan development aplikasi enterprise yang scalable dengan separation of concerns yang jelas dan maintainability yang tinggi. 