# 🧩 MicFx Framework - Module System

## 🎯 **Overview**

Module System adalah inti dari MicFx Framework yang memungkinkan pengembangan aplikasi modular dengan auto-discovery dan lifecycle management yang komprehensif.

---

## 🏗️ **Module Architecture**

### **Struktur Standard Module**
```
📦 MicFx.Modules.{ModuleName}/
├── 📂 Api/                          # API Controllers (REST endpoints)
│   ├── {ModuleName}Controller.cs    # → /api/{module-name}/*
│   └── {Feature}Controller.cs       # → /api/{module-name}/{feature}/*
├── 📂 Controllers/                  # MVC Controllers (Web pages)
│   └── {ModuleName}Controller.cs    # → /{module-name}/*
├── 📂 Areas/                        # Area-based organization
│   └── Admin/                       # Admin area
│       ├── Controllers/             # Admin controllers
│       │   └── {ModuleName}Controller.cs # → /admin/{module-name}/*
│       └── Views/                   # Admin views
│           └── {ModuleName}/
├── 📂 Views/                        # Razor views
│   ├── {ModuleName}/
│   └── Shared/
├── 📂 ViewModels/                   # View models untuk MVC
├── 📂 Services/                     # Business logic services
│   ├── I{ModuleName}Service.cs      # Service interfaces
│   └── {ModuleName}Service.cs       # Service implementations
├── 📂 Domain/                       # Domain entities & value objects
│   ├── Entities/
│   ├── ValueObjects/
│   └── {ModuleName}Entities.cs
├── 📂 Infrastructure/               # Module-specific infrastructure
├── Manifest.cs                      # Module metadata & configuration
├── Startup.cs                       # Module startup & DI configuration
├── README.md                        # Module documentation
└── {ModuleName}.csproj             # Project file
```

**Legacy Structure (Deprecated):**
```
📦 MicFx.Modules.{ModuleName}/
├── 📂 Controllers/                  # MVC Controllers
│   ├── {ModuleName}Controller.cs    # → /{module-name}/*
│   └── {ModuleName}AdminController.cs # → /admin/{module-name}/* (Legacy)
└── ... (other folders)
```

---

## 📋 **Module Manifest**

### **Manifest Implementation**
```csharp
using MicFx.Core.Modularity;

namespace MicFx.Modules.YourModule;

public class Manifest : ModuleManifestBase
{
    // ✅ Required Properties
    public override string Name => "YourModule";
    public override string Version => "1.0.0";
    public override string Description => "Detailed description of your module functionality";
    public override string Author => "Your Name or Team";

    // 🏷️ Categorization & Discovery
    public override string[] Tags => new[]
    {
        "business-logic",      // Business domain
        "api-endpoints",       // Provides API
        "web-interface",       // Has web UI
        "data-processing",     // Data operations
        "integration"          // External integrations
    };

    // 🔗 Dependencies Management
    public override string[] Dependencies => new[]
    {
        "MicFx.Modules.Auth"   // Hard dependencies
    };

    public override string[] OptionalDependencies => new[]
    {
        "MicFx.Modules.Notifications"  // Soft dependencies
    };

    // ⚙️ Module Configuration
    public override int Priority => 100;                    // Loading priority (higher = earlier)
    public override bool IsCritical => false;               // System critical module
    public override bool SupportsHotReload => true;         // Hot reload support
    public override int StartupTimeoutSeconds => 30;        // Startup timeout
    public override string MinimumFrameworkVersion => "1.0.0";

    // 🛠️ Capabilities Declaration
    public override string[] Capabilities => new[]
    {
        "user-management",
        "api-endpoints",
        "background-processing",
        "file-upload",
        "email-notifications"
    };
}
```

### **Advanced Manifest Features**
```csharp
public class AdvancedManifest : ModuleManifestBase
{
    // 🌍 Environment-specific configuration
    public override bool IsEnabledInEnvironment(string environment)
    {
        return environment switch
        {
            "Development" => true,
            "Staging" => true,
            "Production" => true,
            "Testing" => false,    // Disabled in testing
            _ => false
        };
    }

    // 🔧 Feature flags
    public override Dictionary<string, object> GetFeatureFlags()
    {
        return new Dictionary<string, object>
        {
            ["EnableAdvancedFeatures"] = true,
            ["MaxConcurrentUsers"] = 1000,
            ["EnableCaching"] = true
        };
    }

    // 📊 Health check configuration
    public override TimeSpan HealthCheckInterval => TimeSpan.FromMinutes(5);
    public override bool EnableHealthChecks => true;
}
```

---

## 🚀 **Module Startup**

### **Basic Startup Implementation**
```csharp
using MicFx.Core.Modularity;
using Microsoft.Extensions.DependencyInjection;

namespace MicFx.Modules.YourModule;

public class Startup : ModuleStartupBase
{
    public override IModuleManifest Manifest { get; } = new Manifest();

    // 🔧 Service Registration
    protected override void ConfigureModuleServices(IServiceCollection services)
    {
        // Business services
        services.AddScoped<IYourModuleService, YourModuleService>();
        services.AddScoped<IDataProcessor, DataProcessor>();
        
        // Background services
        services.AddHostedService<BackgroundTaskService>();
        
        // External integrations
        services.AddHttpClient<ExternalApiClient>();
        
        // Module-specific options
        services.Configure<YourModuleOptions>(Configuration.GetSection("MicFx:Modules:YourModule"));
    }

    // 🔄 Module initialization
    protected override async Task InitializeModuleAsync()
    {
        Logger.LogInformation("Initializing {ModuleName} module", Manifest.Name);
        
        // Initialize database
        await InitializeDatabaseAsync();
        
        // Setup background tasks
        await SetupBackgroundTasksAsync();
        
        // Validate external dependencies
        await ValidateExternalDependenciesAsync();
    }

    // 🛑 Module cleanup
    protected override async Task ShutdownModuleAsync()
    {
        Logger.LogInformation("Shutting down {ModuleName} module", Manifest.Name);
        
        // Cleanup resources
        await CleanupResourcesAsync();
        
        // Stop background tasks
        await StopBackgroundTasksAsync();
    }

    // 🔧 Private helper methods
    private async Task InitializeDatabaseAsync()
    {
        // Database initialization logic
        using var scope = ServiceProvider.CreateScope();
        var dbService = scope.ServiceProvider.GetRequiredService<IDatabaseService>();
        await dbService.InitializeAsync();
    }
}
```

### **Advanced Startup Features**
```csharp
public class AdvancedStartup : ModuleStartupBase
{
    // 🎛️ Configuration validation
    protected override void ValidateConfiguration()
    {
        var config = Configuration.GetSection("MicFx:Modules:YourModule").Get<YourModuleConfig>();
        
        if (string.IsNullOrEmpty(config?.ApiKey))
        {
            throw new ConfigurationException("API Key is required for YourModule");
        }

        if (config.MaxRetries < 1 || config.MaxRetries > 10)
        {
            throw new ConfigurationException("MaxRetries must be between 1 and 10");
        }
    }

    // 🔗 Dependency validation
    protected override async Task ValidateDependenciesAsync()
    {
        // Check required modules
        if (!IsModuleLoaded("MicFx.Modules.Auth"))
        {
            throw new ModuleDependencyException("Auth module is required");
        }

        // Check external services
        var httpClient = ServiceProvider.GetRequiredService<HttpClient>();
        try
        {
            var response = await httpClient.GetAsync("https://api.external-service.com/health");
            if (!response.IsSuccessStatusCode)
            {
                Logger.LogWarning("External service is not available");
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to validate external service");
        }
    }

    // 📊 Health check registration
    protected override void ConfigureHealthChecks(IServiceCollection services)
    {
        services.AddHealthChecks()
            .AddCheck<YourModuleHealthCheck>("your-module")
            .AddCheck<DatabaseHealthCheck>("your-module-database")
            .AddCheck<ExternalServiceHealthCheck>("external-service");
    }
}
```

---

## 🔄 **Module Lifecycle**

### **Lifecycle Stages**
```
1. Discovery    → Framework scans assemblies for modules
2. Registration → Services registered in DI container
3. Validation   → Dependencies & configuration validated
4. Initialization → Module startup code executed
5. Runtime      → Module is active and serving requests
6. Shutdown     → Cleanup and resource disposal
```

### **Lifecycle Events**
```csharp
public class YourModuleStartup : ModuleStartupBase
{
    // 🔍 Pre-registration hooks
    protected override void OnDiscovered()
    {
        Logger.LogDebug("Module {ModuleName} discovered", Manifest.Name);
    }

    // 📝 Pre-initialization hooks
    protected override void OnRegistering()
    {
        Logger.LogDebug("Registering services for {ModuleName}", Manifest.Name);
    }

    // ✅ Post-initialization hooks
    protected override void OnInitialized()
    {
        Logger.LogInformation("{ModuleName} module initialized successfully", Manifest.Name);
        
        // Post-initialization tasks
        NotifyOtherModules();
        StartPerformanceMonitoring();
    }

    // ❌ Error handling
    protected override void OnInitializationFailed(Exception exception)
    {
        Logger.LogError(exception, "Failed to initialize {ModuleName}", Manifest.Name);
        
        // Cleanup partial initialization
        CleanupPartialInitialization();
    }

    // 🛑 Shutdown hooks
    protected override void OnShuttingDown()
    {
        Logger.LogInformation("Shutting down {ModuleName} module", Manifest.Name);
        
        // Graceful shutdown
        NotifyShutdown();
        WaitForActiveRequests();
    }
}
```

---

## 🔧 **Dependency Management**

### **Dependency Declaration**
```csharp
public class Manifest : ModuleManifestBase
{
    // Hard dependencies (required)
    public override string[] Dependencies => new[]
    {
        "MicFx.Modules.Auth",          // Authentication required
        "MicFx.Modules.Database"       // Database access required
    };

    // Soft dependencies (optional)
    public override string[] OptionalDependencies => new[]
    {
        "MicFx.Modules.Notifications", // Enhanced with notifications
        "MicFx.Modules.Caching"        // Performance enhancement
    };

    // Version constraints
    public override Dictionary<string, string> DependencyVersions => new()
    {
        ["MicFx.Modules.Auth"] = ">=1.0.0",
        ["MicFx.Modules.Database"] = "^2.0.0"
    };
}
```

### **Dependency Injection Integration**
```csharp
protected override void ConfigureModuleServices(IServiceCollection services)
{
    // Check for optional dependencies
    if (IsModuleAvailable("MicFx.Modules.Notifications"))
    {
        services.AddScoped<INotificationSender, EnhancedNotificationSender>();
    }
    else
    {
        services.AddScoped<INotificationSender, BasicNotificationSender>();
    }

    // Conditional service registration
    if (IsModuleAvailable("MicFx.Modules.Caching"))
    {
        services.Decorate<IDataService, CachedDataService>();
    }
}
```

---

## 🔍 **Auto-Discovery System**

### **Discovery Process**
```csharp
// Framework automatically discovers:
// 1. Controllers ending with "Controller"
// 2. API controllers in "Api" folder
// 3. MVC controllers in "Controllers" folder
// 4. Admin controllers ending with "AdminController"
// 5. Services implementing interfaces
// 6. Health checks implementing IHealthCheck
```

### **Discovery Configuration**
```csharp
[assembly: ModuleDiscovery(
    EnableAutoDiscovery = true,
    ScanSubdirectories = true,
    IncludePatterns = new[] { "*.Controller", "*.Service" },
    ExcludePatterns = new[] { "*.Test*", "*.Mock*" }
)]
```

---

## 📊 **Health Checks**

### **Module Health Check Implementation**
```csharp
using Microsoft.Extensions.Diagnostics.HealthChecks;

public class YourModuleHealthCheck : IHealthCheck
{
    private readonly IYourModuleService _service;
    private readonly ILogger<YourModuleHealthCheck> _logger;

    public YourModuleHealthCheck(
        IYourModuleService service,
        ILogger<YourModuleHealthCheck> logger)
    {
        _service = service;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Check service health
            var isHealthy = await _service.IsHealthyAsync(cancellationToken);
            
            if (!isHealthy)
            {
                return HealthCheckResult.Unhealthy("Service is not responding");
            }

            // Check dependencies
            var dependencyHealth = await CheckDependenciesAsync(cancellationToken);
            
            return HealthCheckResult.Healthy("Module is healthy", new Dictionary<string, object>
            {
                ["LastCheck"] = DateTime.UtcNow,
                ["Dependencies"] = dependencyHealth,
                ["Version"] = "1.0.0"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Health check failed for YourModule");
            return HealthCheckResult.Unhealthy("Health check failed", ex);
        }
    }

    private async Task<Dictionary<string, string>> CheckDependenciesAsync(CancellationToken cancellationToken)
    {
        var results = new Dictionary<string, string>();

        // Check database
        try
        {
            await _service.TestDatabaseConnectionAsync(cancellationToken);
            results["Database"] = "Healthy";
        }
        catch
        {
            results["Database"] = "Unhealthy";
        }

        // Check external APIs
        try
        {
            await _service.TestExternalApiAsync(cancellationToken);
            results["ExternalAPI"] = "Healthy";
        }
        catch
        {
            results["ExternalAPI"] = "Unhealthy";
        }

        return results;
    }
}
```

---

## 🔧 **Best Practices**

### **Module Design Principles**
1. **Single Responsibility**: Each module should have one clear purpose
2. **Loose Coupling**: Minimize dependencies between modules
3. **High Cohesion**: Related functionality should be in the same module
4. **Interface Segregation**: Use specific interfaces for different concerns
5. **Dependency Inversion**: Depend on abstractions, not concretions

### **Naming Conventions**
```csharp
// Module Names
MicFx.Modules.{BusinessDomain}      // e.g., MicFx.Modules.UserManagement
MicFx.Modules.{Feature}             // e.g., MicFx.Modules.Authentication

// Namespaces
MicFx.Modules.{ModuleName}.Api      // API controllers
MicFx.Modules.{ModuleName}.Controllers // MVC controllers
MicFx.Modules.{ModuleName}.Services    // Business services
MicFx.Modules.{ModuleName}.Domain      // Domain entities

// File Names
{ModuleName}Controller.cs           // Main controller
{ModuleName}Service.cs             // Main service
{ModuleName}Entities.cs            // Domain entities
{ModuleName}HealthCheck.cs         // Health check
```

### **Performance Optimization**
```csharp
// Lazy loading for expensive services
services.AddScoped<Lazy<IExpensiveService>>(provider =>
    new Lazy<IExpensiveService>(() => provider.GetRequiredService<IExpensiveService>()));

// Connection pooling for external services
services.AddHttpClient<ExternalApiClient>()
    .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
    {
        MaxConnectionsPerServer = 10
    });

// Caching for frequently accessed data
services.AddMemoryCache();
services.Decorate<IDataService, CachedDataService>();
```

---

## 🚨 **Troubleshooting**

### **Common Issues**

| Problem | Cause | Solution |
|---------|-------|----------|
| Module not discovered | Wrong namespace or assembly not referenced | Check namespace and project references |
| Dependency injection fails | Service not registered or circular dependency | Review service registration and dependencies |
| Module fails to start | Configuration error or missing dependency | Check configuration and dependency availability |
| Routes not working | Controller not in correct folder | Verify controller location matches conventions |
| Health check fails | Service unavailable or configuration issue | Check service health and configuration |

### **Debugging Module Issues**
```csharp
// Enable detailed module logging
"Serilog": {
  "MinimumLevel": {
    "Default": "Information",
    "Override": {
      "MicFx.Core.Modularity": "Debug",
      "MicFx.Modules.YourModule": "Debug"
    }
  }
}

// Check module registration logs
[12:34:56 INF] Discovering modules in assembly MicFx.Modules.YourModule
[12:34:57 INF] Found module: YourModule v1.0.0
[12:34:58 INF] Registering services for module: YourModule
[12:34:59 INF] Module YourModule initialized successfully
```

---

## 📈 **Advanced Features**

### **Simplified Module Lifecycle**
```csharp
public class Manifest : ModuleManifestBase
{
    // Hot reload removed for simplicity - deploy new version instead
    public override bool IsCritical => false; // Only essential property remains
}
```

### **Module Communication (Simplified)**
```csharp
// Use direct service dependencies instead of complex event bus
public class YourModuleService
{
    private readonly ILogger<YourModuleService> _logger;
    private readonly IOtherModuleService _otherModuleService; // Direct DI injection

    public async Task ProcessDataAsync(Data data)
    {
        // Process data
        await ProcessAsync(data);
        
        // Simple direct call instead of event bus
        await _otherModuleService.HandleDataProcessedAsync(data.Id);
        
        _logger.LogInformation("Data {DataId} processed successfully", data.Id);
    }
}
```

---

*Module System adalah fondasi untuk membangun aplikasi modular yang scalable dan maintainable dengan MicFx Framework.* 