# 📚 MicFx Framework - Documentation

## 🎯 **Overview**

MicFx adalah framework modular berbasis ASP.NET Core 8 yang dirancang untuk pengembangan aplikasi enterprise dengan prinsip clean architecture dan zero-configuration development.

---

## 📖 **Dokumentasi Lengkap**

Dokumentasi terperinci untuk setiap fitur tersedia dalam file terpisah:

### **🧩 Core Framework**
- **[Module System](MODULE_SYSTEM.md)** - Auto-discovery, lifecycle management, dan module organization
- **[Configuration Management](CONFIGURATION_MANAGEMENT.md)** - Type-safe configuration dengan validation dan hot-reload
- **[Exception Handling](EXCEPTION_HANDLING.md)** - Structured error handling dengan environment-aware responses
- **[Architecture Overview](ARCHITECTURE.md)** - Clean architecture dan design patterns

### **🛠️ Infrastructure & Monitoring**
- **[Structured Logging](STRUCTURED_LOGGING.md)** - Serilog implementation dengan module context enrichment
- **[Health Checks](HEALTH_CHECKS.md)** - Comprehensive health monitoring dan alerting system
- **[Smart Routing](SMART_ROUTING.md)** - Convention-based routing dengan auto-discovery
- **[Admin Panel](ADMIN_PANEL.md)** - Modern admin interface dengan auto-discovery navigation

### **🔧 Development Tools**
- **[Swagger Integration](SWAGGER_INTEGRATION.md)** - API documentation generation dengan module organization

### **📚 Quick Start Guides**
- **[Admin Panel Quick Start](ADMIN_QUICK_START.md)** - Panduan cepat menggunakan admin interface

---

## 🏗️ **Architecture**

Framework ini menggunakan layered architecture dengan separation of concerns yang jelas:

```
📦 MicFx Framework
├── 🌐 MicFx.Web           # Host Application
├── ⚙️ MicFx.Core              # Framework Core  
├── 🏗️ MicFx.Infrastructure   # Infrastructure Implementation
├── 📋 MicFx.Abstractions      # Contracts & Interfaces
├── 📦 MicFx.SharedKernel      # Shared Components
└── 🧩 Modules/                # Application Modules
    ├── MicFx.Modules.HelloWorld
    └── MicFx.Modules.Auth
```

---

## 🚀 **Key Features**

### **Module System**
- ✅ **Auto-Discovery**: Automatic module detection dan registration
- ✅ **Lifecycle Management**: Complete module lifecycle dengan hooks
- ✅ **Dependency Resolution**: Automatic dependency resolution
- ✅ **Health Checks**: Built-in health monitoring

### **Smart Routing**
- ✅ **Folder-Based Routing**: API controllers di `Api/` folder, MVC di `Controllers/`
- ✅ **Auto Kebab-Case**: `HelloWorld` → `/hello-world` secara otomatis
- ✅ **Multiple Patterns**: Support API, MVC, dan Admin routing

### **Infrastructure**
- ✅ **Structured Logging**: Serilog dengan automatic module context
- ✅ **Configuration Management**: Centralized configuration dengan validation
- ✅ **Exception Handling**: Global exception middleware dengan structured responses
- ✅ **View Resolution**: Modular view resolution system

### **Admin Panel**
- ✅ **Auto-Discovery Navigation**: Automatic module admin interface detection
- ✅ **Real-time Dashboard**: Live system metrics dan module information
- ✅ **Role-Based Access**: Permission-based navigation filtering
- ✅ **Diagnostics System**: Comprehensive system monitoring dan debugging
- ✅ **Modern UI**: Responsive admin interface dengan Tailwind CSS

---

## 🛠️ **Quick Start**

### **1. Struktur Module**
```
📦 MicFx.Modules.YourModule/
├── 📂 Api/                    # JSON API Controllers
│   └── YourModuleController.cs # → /api/your-module/*
├── 📂 Controllers/            # MVC Controllers  
│   └── YourModuleController.cs # → /your-module/*
├── 📂 Areas/                  # Area-based organization
│   └── Admin/                 # Admin area (Recommended)
│       ├── Controllers/       # Admin controllers
│       │   └── YourModuleController.cs # → /admin/your-module/*
│       └── Views/             # Admin views
│           └── YourModule/
├── 📂 Views/                  # Razor views
├── 📂 Services/               # Business logic
├── Manifest.cs                # Module metadata
└── Startup.cs                 # Module configuration
```

**⚠️ Breaking Change:** Admin controllers sekarang menggunakan **Areas/Admin** pattern daripada suffix `AdminController`. Legacy pattern masih didukung untuk backward compatibility.

### **2. Membuat Module Baru**

**a. Buat Project**
```bash
mkdir src/Modules/MicFx.Modules.YourModule
cd src/Modules/MicFx.Modules.YourModule
```

**b. Project File**
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>
  
  <ItemGroup>
    <ProjectReference Include="../../MicFx.SharedKernel/MicFx.SharedKernel.csproj" />
    <ProjectReference Include="../../MicFx.Abstractions/MicFx.Abstractions.csproj" />
  </ItemGroup>
</Project>
```

**c. Manifest**
```csharp
public class Manifest : ModuleManifestBase
{
    public override string Name => "YourModule";
    public override string Version => "1.0.0";
    public override string Description => "Your module description";
    public override string Author => "Your Name";
}
```

**d. Startup**
```csharp
public class Startup : ModuleStartupBase
{
    public override IModuleManifest Manifest { get; } = new Manifest();
    
    protected override void ConfigureModuleServices(IServiceCollection services)
    {
        services.AddScoped<IYourService, YourService>();
    }
}
```

### **3. Membuat Controllers**

**API Controller**
```csharp
// File: Api/YourModuleController.cs
namespace MicFx.Modules.YourModule.Api;

[ApiController]
public class YourModuleController : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ApiResponse<string>>> GetData()
    {
        // AUTO-ROUTE: GET /api/your-module/get-data
        return Ok(ApiResponse<string>.Ok("Hello from API"));
    }
}
```

**MVC Controller**
```csharp
// File: Controllers/YourModuleController.cs  
namespace MicFx.Modules.YourModule.Controllers;

public class YourModuleController : Controller
{
    public IActionResult Index()
    {
        // AUTO-ROUTE: GET /your-module
        return View();
    }
}
```

**Admin Controller (Areas/Admin Pattern - Recommended)**
```csharp
// File: Areas/Admin/Controllers/YourModuleController.cs
namespace MicFx.Modules.YourModule.Areas.Admin.Controllers;

[Area("Admin")]
public class YourModuleController : Controller
{
    public IActionResult Index()
    {
        // AUTO-ROUTE: GET /admin/your-module
        return View();
    }

    public IActionResult Settings()
    {
        // AUTO-ROUTE: GET /admin/your-module/settings
        return View();
    }
}
```

**Legacy Admin Controller (Deprecated)**
```csharp
// File: Controllers/YourModuleAdminController.cs (Not recommended)
namespace MicFx.Modules.YourModule.Controllers;

public class YourModuleAdminController : Controller
{
    public IActionResult Index()
    {
        // AUTO-ROUTE: GET /admin/your-module (Legacy)
        return View();
    }
}
```

---

## ⚙️ **Configuration**

### **appsettings.json**
```json
{
  "MicFx": {
    "ConfigurationManagement": {
      "AutoRegisterConfigurations": true,
      "EnableConfigurationMonitoring": true,
      "ValidateOnStartup": true
    },
    "Modules": {
      "YourModule": {
        "Setting1": "value1",
        "Setting2": 123
      }
    }
  },
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information"
    },
    "WriteTo": [
      { "Name": "Console" },
      { 
        "Name": "File", 
        "Args": { "path": "logs/micfx-.log" }
      }
    ]
  }
}
```

### **Module Configuration**
```csharp
public class YourModuleConfig
{
    [Required]
    public string Setting1 { get; set; } = string.Empty;
    
    [Range(1, 100)]
    public int Setting2 { get; set; } = 10;
}

public class YourModuleConfigService : ModuleConfigurationBase<YourModuleConfig>
{
    public override string ModuleName => "YourModule";
    public override string SectionName => "MicFx:Modules:YourModule";
}
```

---

## 🧪 **Testing & Development**

### **Available Endpoints**
- **Admin Panel**: `/admin` (main dashboard), `/admin/diagnostics` (system info)
- **Health Checks**: `/health`, `/health/modules`
- **API Documentation**: `/api/docs` (development only)
- **HelloWorld Demo**: `/hello-world/*`, `/api/hello-world/*`, `/admin/hello-world/*`

### **Logging**
Framework menyediakan structured logging dengan automatic module context:

```csharp
public class YourService
{
    private readonly IStructuredLogger<YourService> _logger;

    public YourService(IStructuredLogger<YourService> logger)
    {
        _logger = logger;
    }

    public async Task DoSomething()
    {
        _logger.LogBusinessOperation("DoSomething", 
            new { Parameter = "value" }, 
            "Operation started");
            
        using var timer = _logger.BeginTimedOperation("DatabaseQuery");
        // Your logic here
    }
}
```

### **Exception Handling**
Framework menangani semua exception secara otomatis:

```csharp
[HttpGet]
public async Task<ActionResult<ApiResponse<User>>> GetUser(int id)
{
    var user = await _userService.GetUserAsync(id);
    
    if (user == null)
    {
        // Akan otomatis dikonversi ke structured response
        throw new BusinessException("User not found", "USER_NOT_FOUND")
            .AddDetail("UserId", id);
    }

    return Ok(ApiResponse<User>.Ok(user));
}
```

---

## 🚀 **Production**

### **Environment Configuration**
- **Development**: Detailed logging, hot reload, verbose errors
- **Production**: Optimized logging, minimal error exposure, performance focused

### **Monitoring**
Framework kompatibel dengan:
- **ELK Stack**: Structured JSON logs
- **Seq**: Centralized logging
- **Application Insights**: APM integration
- **Custom monitoring tools**: Extensible architecture

### **Security**
- Environment-aware error responses
- No sensitive data dalam logs
- Complete audit trail
- Module isolation

---

## 📊 **Module Examples**

### **MicFx.Modules.HelloWorld**
Proof of concept module yang mendemonstrasikan:
- API dan MVC endpoints (`/api/hello-world/*`, `/hello-world/*`)
- Admin interface (`/admin/hello-world/*`)
- Configuration management
- Structured logging
- Exception handling
- Navigation contributor implementation

### **MicFx.Modules.Auth** 
Authentication module dengan:
- User authentication dan authorization
- Role-based access control
- Session management
- Admin interface untuk user management
- Security features dan audit logging

---

## 🔧 **Development Tips**

### **Routing Conventions**
- API controllers: Folder `Api/` → `/api/{module-name}/*`
- MVC controllers: Folder `Controllers/` → `/{module-name}/*`
- Admin controllers: `*AdminController` → `/admin/{module-name}/*`

### **Naming Conventions**
- Modules: `MicFx.Modules.{ModuleName}`
- Namespaces: `MicFx.Modules.{ModuleName}.{Folder}`
- URLs: Auto-converted ke kebab-case

### **Best Practices**
1. Gunakan folder structure untuk organization
2. Implement proper dependency injection
3. Leverage structured logging
4. Handle exceptions dengan typed exceptions
5. Configure module dengan strongly-typed classes

---

## 🆘 **Troubleshooting**

### **Common Issues**
| Issue | Solution |
|-------|----------|
| Module tidak load | Check project references dan namespace |
| Route tidak work | Verify controller location dan naming |
| Configuration error | Check appsettings.json structure |
| View tidak found | Verify view location dan naming |

### **Debugging**
1. Check application logs untuk detailed errors
2. Use `/health` endpoints untuk module status  
3. Verify `/swagger` untuk API documentation
4. Check module registration di startup logs

---

## 📞 **Support**

- **Issues**: Report dengan detail reproduction steps
- **Documentation**: Lihat file-file di folder `/docs`
- **Examples**: Check `MicFx.Modules.HelloWorld` untuk reference
- **Health Checks**: Monitor via `/health/modules` endpoint

---

*Framework ini terus berkembang. Dokumentasi akan diupdate sesuai dengan fitur terbaru.* 