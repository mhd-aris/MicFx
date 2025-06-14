# 🌐 MicFx.Mvc.Web - Host Application

## 🎯 **Overview**

MicFx.Mvc.Web adalah **host application** untuk MicFx Framework - sebuah modular ASP.NET Core framework yang mengimplementasikan clean architecture dengan zero-configuration development. Aplikasi ini berfungsi sebagai entry point dan orchestrator untuk semua module dalam ecosystem MicFx.

## 🏗️ **Architecture**

### **Layer Structure**
```
📦 MicFx.Mvc.Web (Host Application)
├── 🚀 Program.cs                 # Application entry point & configuration
├── ⚙️ appsettings.json          # Configuration management
├── 🎛️ Admin/                    # Admin panel system
│   ├── Services/                # Admin navigation & module discovery
│   ├── Extensions/              # DI extensions for admin features
│   └── Navigation/              # Navigation system components
├── 🏢 Areas/                    # MVC Areas (Admin interface)
│   └── Admin/                   # Admin area controllers & views
├── 🔧 Infrastructure/           # View resolution & framework infrastructure
├── 📄 Views/                    # Shared views & layouts
└── 🌐 wwwroot/                  # Static assets & resources
```

### **Responsibilities**
- **Module Orchestration**: Auto-discovery dan lifecycle management untuk semua modules
- **Infrastructure Setup**: Konfigurasi logging, exception handling, dan middleware pipeline
- **Admin Interface**: Comprehensive admin panel dengan real-time monitoring
- **View Resolution**: Sophisticated view location expansion untuk modular views
- **Security Management**: Authentication, authorization, dan security policies

## 🚀 **Quick Start**

### **Prerequisites**
- .NET 8.0 SDK atau lebih baru
- Visual Studio 2022 atau VS Code
- SQL Server (LocalDB atau full instance)

### **Running the Application**

1. **Clone dan Setup**
   ```bash
   git clone <repository-url>
   cd src/MicFx.Mvc.Web
   ```

2. **Configure Database** (Optional)
   ```bash
   # Edit appsettings.json connection string if needed
   # Default menggunakan LocalDB
   ```

3. **Run Application**
   ```bash
   dotnet run
   ```

4. **Access Application**
   - **Main App**: `http://localhost:5000`
   - **Admin Panel**: `http://localhost:5000/admin`
   - **API Docs**: `http://localhost:5000/api/docs` (Development only)
   - **Health Checks**: `http://localhost:5000/health`

## 🎛️ **Admin Panel Features**

### **Dashboard** (`/admin`)
- **Real-time System Information**: Clock, uptime, memory usage
- **Module Statistics**: Loaded modules, navigation contributors
- **Quick Actions**: Access to diagnostics, logs, dan configuration
- **Navigation Discovery**: Auto-generated navigation dari semua modules

### **Diagnostics** (`/admin/diagnostics`)
- **Module Scanner**: Comprehensive module information
- **Navigation Testing**: Role-based navigation testing
- **System Health**: Real-time health monitoring
- **Performance Metrics**: Response times dan system performance

### **Available Endpoints**
```
🏠 Main Dashboard:        /admin
🔍 System Diagnostics:    /admin/diagnostics
🧪 Role Testing:          /admin/diagnostics/test-roles
🩺 Health Checks:         /health
📚 API Documentation:     /swagger (dev only)
```

## ⚙️ **Configuration**

### **appsettings.json Structure**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=micfx;Trusted_Connection=true;"
  },
  "MicFx": {
    "ConfigurationManagement": {
      "AutoRegisterConfigurations": true,
      "EnableConfigurationMonitoring": true,
      "ValidateOnStartup": true
    },
    "Modules": {
      "HelloWorld": {
        "DefaultGreeting": "Hello from MicFx!",
        "MaxGreetings": 25,
        "EnableLogging": true
      }
    },
    "Auth": {
      "DefaultRoles": ["SuperAdmin", "Admin", "User"],
      "Cookie": {
        "ExpireTimeSpanHours": 2,
        "CookieName": "MicFx.Auth"
      }
    }
  },
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information"
    },
    "WriteTo": [
      { "Name": "Console" },
      { "Name": "File", "Args": { "path": "logs/micfx-.log" } }
    ]
  }
}
```

### **Environment-Specific Configuration**
- **Development**: Debug logging, request logging, detailed errors
- **Production**: Optimized logging, minimal error exposure, performance focused

### **Security Configuration**
```bash
# Use User Secrets for sensitive data (Development)
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "your-connection-string"
dotnet user-secrets set "MicFx:Auth:DefaultAdmin:Password" "your-admin-password"

# Use Environment Variables for Production
export MICFX_ConnectionStrings__DefaultConnection="your-connection-string"
export MICFX_Auth__DefaultAdmin__Password="your-admin-password"
```

## 🧩 **Module Integration**

### **How Modules are Loaded**
1. **Auto-Discovery**: Framework scans `MicFx.Modules.*` assemblies
2. **Dependency Resolution**: Automatic dependency resolution dan registration
3. **Lifecycle Management**: Complete module lifecycle dengan initialization hooks
4. **View Resolution**: Modular view resolution dengan intelligent path detection

### **Supported Module Patterns**
```
📦 Module Structure:
├── 📂 Api/                    # JSON API Controllers → /api/{module-name}/*
├── 📂 Controllers/            # MVC Controllers → /{module-name}/*
├── 📂 Areas/Admin/            # Admin Controllers → /admin/{module-name}/*
├── 📂 Views/                  # Razor Views
├── 📂 Services/               # Business Logic
├── Manifest.cs                # Module Metadata
└── Startup.cs                 # Module Configuration
```

### **Available Modules**
- **MicFx.Modules.HelloWorld**: Demo module dengan comprehensive examples
- **MicFx.Modules.Auth**: Authentication dan authorization system

## 🔧 **Development Guide**

### **Project Structure**
```
src/MicFx.Mvc.Web/
├── Program.cs                 # ✅ Application entry point
├── appsettings.json          # ✅ Main configuration
├── Admin/                    # ✅ Admin panel system
│   ├── Services/             # Navigation discovery, module scanner
│   └── Extensions/           # DI registration extensions
├── Areas/Admin/              # ✅ Admin interface
│   ├── Controllers/          # Dashboard, Diagnostics controllers
│   └── Views/                # Admin views dan layouts
├── Infrastructure/           # ✅ Framework infrastructure
│   └── MicFxViewLocationExpander.cs # Sophisticated view resolution
└── Views/                    # ✅ Shared views
    └── Shared/               # Global layouts dan components
```

### **Key Components**

#### **1. Program.cs - Application Bootstrap**
- Serilog configuration dengan environment-aware settings
- Module discovery dan lifecycle management
- Middleware pipeline configuration
- Security policies dan authorization setup

#### **2. Admin System**
- **AdminNavDiscoveryService**: Auto-discovery navigation dari modules
- **AdminModuleScanner**: Comprehensive module information scanning
- **Dashboard & Diagnostics**: Real-time monitoring dan debugging tools

#### **3. View Resolution**
- **MicFxViewLocationExpander**: Sophisticated view resolution untuk modular views
- Support untuk multiple patterns (MVC, Admin, API)
- Backward compatibility dengan legacy patterns

### **Adding New Features**

#### **1. Adding Admin Navigation**
```csharp
// In your module
public class YourModuleAdminNavContributor : IAdminNavContributor
{
    public Task<IEnumerable<AdminNavItem>> GetNavigationItemsAsync(AdminNavContext context)
    {
        return Task.FromResult<IEnumerable<AdminNavItem>>(new[]
        {
            new AdminNavItem
            {
                Title = "Your Module Admin",
                Url = "/admin/your-module",
                Icon = "settings",
                Category = "Management",
                Order = 100
            }
        });
    }
}
```

#### **2. Adding Health Checks**
```csharp
// In Program.cs
builder.Services.AddHealthChecks()
    .AddCheck<YourCustomHealthCheck>("your-check");
```

#### **3. Adding Custom Configuration**
```csharp
// In appsettings.json
{
  "MicFx": {
    "Modules": {
      "YourModule": {
        "YourSetting": "value"
      }
    }
  }
}
```

## 🚨 **Troubleshooting**

### **Common Issues**

| Problem | Cause | Solution |
|---------|-------|----------|
| Module tidak load | Project reference missing | Add project reference di .csproj |
| Admin navigation kosong | Navigation contributor not registered | Register IAdminNavContributor dalam module startup |
| View tidak ditemukan | View location tidak sesuai convention | Check view location dan naming convention |
| Database connection error | Connection string salah | Verify connection string di configuration |
| Authorization error | Role tidak sesuai | Check user roles dan policy requirements |

### **Debugging Tips**

#### **1. Enable Debug Logging**
```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Debug",
      "Override": {
        "MicFx": "Debug"
      }
    }
  }
}
```

#### **2. Check Module Loading**
```bash
# Check logs untuk module loading
grep "Module.*loaded" logs/micfx-*.log
```

#### **3. Test Health Endpoints**
```bash
# Check application health
curl http://localhost:5000/health

# Check module-specific health
curl http://localhost:5000/health/modules
```

#### **4. Admin Panel Diagnostics**
- Access `/admin/diagnostics` untuk system information
- Use `/admin/diagnostics/test-roles` untuk testing authorization

## 📊 **Performance Considerations**

### **Production Optimizations**
- **Request Logging**: Disabled di production untuk performance
- **Minimum Log Level**: Set ke Warning atau Error
- **View Compilation**: Runtime compilation disabled di production
- **Exception Details**: Minimized untuk security

### **Memory Management**
- **Module Caching**: Navigation items cached untuk 15 minutes
- **View Caching**: Views cached setelah first compilation
- **Health Check Caching**: Results cached untuk mengurangi overhead

## 🛡️ **Security**

### **Authentication & Authorization**
- **Cookie Authentication**: Configured dengan secure defaults
- **Role-Based Access**: Admin area requires Admin atau SuperAdmin role
- **Anti-Forgery**: Enabled untuk semua POST operations
- **HTTPS**: Enforced di production environments

### **Security Best Practices**
- **Sensitive Data**: Never store passwords atau secrets di appsettings.json
- **Connection Strings**: Use User Secrets (dev) atau Environment Variables (prod)
- **Error Handling**: Environment-aware error detail exposure
- **Logging**: No sensitive data dalam logs

## 🤝 **Contributing**

### **Development Setup**
1. Fork repository
2. Create feature branch
3. Setup development environment
4. Run tests
5. Submit pull request

### **Code Standards**
- Follow existing code style dan conventions
- Add XML documentation untuk public APIs
- Include unit tests untuk new features
- Update documentation jika diperlukan

### **Testing**
```bash
# Run tests
dotnet test

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"
```

## 📞 **Support**

### **Resources**
- **Documentation**: See `/docs` folder untuk comprehensive documentation
- **Examples**: Check `MicFx.Modules.HelloWorld` untuk implementation examples
- **Health Monitoring**: Use `/health` endpoints untuk application status

### **Getting Help**
- Check troubleshooting section di atas
- Review application logs di `logs/` folder  
- Use admin panel diagnostics untuk debugging
- Check existing issues dalam repository

---

## 🏷️ **Project Info**

- **Framework**: ASP.NET Core 8.0
- **Architecture**: Clean Architecture dengan Modular Monolith
- **UI Framework**: Tailwind CSS
- **Logging**: Serilog dengan structured logging
- **Testing**: xUnit dengan comprehensive coverage

---

*MicFx.Mvc.Web adalah host application yang robust dan production-ready untuk framework modular MicFx. Dengan comprehensive admin panel, sophisticated module system, dan excellent developer experience.* 