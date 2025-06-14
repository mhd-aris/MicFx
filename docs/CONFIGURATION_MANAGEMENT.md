# ⚙️ MicFx Framework - Configuration Management

## 🎯 **Overview**

Configuration Management dalam MicFx Framework menyediakan sistem konfigurasi terpusat dengan type-safety, validation, dan hot-reload capabilities untuk semua module.

---

## 🏗️ **Architecture**

### **Configuration Flow**
```
appsettings.json → ConfigurationManager → ModuleConfiguration → Validation → DI Container → Runtime Usage
```

---

## 📄 **Configuration Structure**

### **appsettings.json Structure**
```json
{
  "MicFx": {
    "ConfigurationManagement": {
      "AutoRegisterConfigurations": true,
      "EnableConfigurationMonitoring": true,
      "EnableChangeNotifications": true,
      "ValidateOnStartup": true,
      "ThrowOnValidationFailure": true,
      "MonitoringIntervalMs": 30000
    },
    "Modules": {
      "HelloWorld": {
        "DefaultGreeting": "Hello from MicFx!",
        "MaxGreetings": 25,
        "EnableLogging": true,
        "ApiSettings": {
          "BaseUrl": "https://api.example.com",
          "ApiKey": "your-api-key",
          "TimeoutSeconds": 30,
          "RetryCount": 3
        },
        "FeatureFlags": {
          "EnableAdvancedFeatures": true,
          "EnableCaching": false,
          "MaxConcurrentUsers": 100
        }
      }
    }
  }
}
```

---

## 🔧 **Module Configuration Implementation**

### **Configuration Class Definition**
```csharp
using System.ComponentModel.DataAnnotations;

namespace MicFx.Modules.YourModule.Configuration;

public class YourModuleConfig
{
    [Required(ErrorMessage = "Module name is required")]
    [StringLength(50, MinimumLength = 3)]
    public string ModuleName { get; set; } = "YourModule";

    [Range(1, 1000)]
    public int MaxItems { get; set; } = 100;

    public bool EnableFeature { get; set; } = true;

    public ApiConfig ApiSettings { get; set; } = new();

    public Dictionary<string, object> FeatureFlags { get; set; } = new();
}

public class ApiConfig
{
    [Required]
    [Url(ErrorMessage = "Valid URL is required")]
    public string BaseUrl { get; set; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 10)]
    public string ApiKey { get; set; } = string.Empty;

    [Range(5, 300)]
    public int TimeoutSeconds { get; set; } = 30;

    [Range(0, 10)]
    public int RetryCount { get; set; } = 3;
}
```

### **Configuration Service Implementation**
```csharp
using MicFx.Core.Configuration;
using Microsoft.Extensions.Options;

namespace MicFx.Modules.YourModule.Configuration;

public class YourModuleConfigService : ModuleConfigurationBase<YourModuleConfig>
{
    public override string ModuleName => "YourModule";
    public override string SectionName => "MicFx:Modules:YourModule";

    public YourModuleConfigService(
        IOptionsMonitor<YourModuleConfig> options,
        ILogger<YourModuleConfigService> logger)
        : base(options, logger)
    {
    }

    protected override ValidationResult ValidateConfiguration(YourModuleConfig config)
    {
        var errors = new List<string>();

        if (config.MaxItems > 500 && !config.EnableFeature)
        {
            errors.Add("MaxItems cannot exceed 500 when EnableFeature is false");
        }

        if (!Uri.TryCreate(config.ApiSettings.BaseUrl, UriKind.Absolute, out _))
        {
            errors.Add("Invalid API BaseUrl format");
        }

        return errors.Any() 
            ? ValidationResult.Failed(errors.ToArray())
            : ValidationResult.Success();
    }

    public bool IsFeatureEnabled(string featureName)
    {
        return Current.FeatureFlags.TryGetValue(featureName, out var value) && 
               value is bool enabled && enabled;
    }

    public T GetFeatureValue<T>(string featureName, T defaultValue)
    {
        if (Current.FeatureFlags.TryGetValue(featureName, out var value) && value is T typedValue)
        {
            return typedValue;
        }
        return defaultValue;
    }
}
```

---

## 🔄 **Configuration Registration**

### **Module Startup Registration**
```csharp
using MicFx.Core.Modularity;

namespace MicFx.Modules.YourModule;

public class Startup : ModuleStartupBase
{
    protected override void ConfigureModuleServices(IServiceCollection services)
    {
        // Register configuration
        services.Configure<YourModuleConfig>(
            Configuration.GetSection("MicFx:Modules:YourModule"));

        // Register configuration service
        services.AddSingleton<YourModuleConfigService>();

        // Configuration-dependent services
        services.AddHttpClient<ApiClient>((serviceProvider, client) =>
        {
            var config = serviceProvider.GetRequiredService<YourModuleConfigService>();
            client.BaseAddress = new Uri(config.Current.ApiSettings.BaseUrl);
            client.Timeout = TimeSpan.FromSeconds(config.Current.ApiSettings.TimeoutSeconds);
        });
    }

    protected override void ValidateConfiguration()
    {
        var configSection = Configuration.GetSection("MicFx:Modules:YourModule");
        var config = configSection.Get<YourModuleConfig>();

        if (config == null)
        {
            throw new ConfigurationException($"Configuration section not found");
        }

        var context = new ValidationContext(config);
        var results = new List<ValidationResult>();
        
        if (!Validator.TryValidateObject(config, context, results, true))
        {
            var errors = results.Select(r => r.ErrorMessage).ToArray();
            throw new ConfigurationException($"Configuration validation failed: {string.Join(", ", errors)}");
        }
    }
}
```

---

## 🌍 **Environment-Specific Configuration**

### **Environment Configuration Files**
```json
// appsettings.Development.json
{
  "MicFx": {
    "Modules": {
      "YourModule": {
        "MaxItems": 50,
        "ApiSettings": {
          "BaseUrl": "https://dev-api.example.com"
        },
        "FeatureFlags": {
          "EnableDebugMode": true
        }
      }
    }
  }
}

// appsettings.Production.json
{
  "MicFx": {
    "Modules": {
      "YourModule": {
        "MaxItems": 1000,
        "ApiSettings": {
          "BaseUrl": "https://api.example.com"
        },
        "FeatureFlags": {
          "EnableCaching": true
        }
      }
    }
  }
}
```

---

## 🔐 **Secure Configuration**

### **Secrets Management**
```csharp
// For development - use User Secrets
// dotnet user-secrets init
// dotnet user-secrets set "MicFx:Modules:YourModule:ApiSettings:ApiKey" "dev-api-key"

public class SecureConfigurationService
{
    private readonly IConfiguration _configuration;

    public SecureConfigurationService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GetSecureValue(string key)
    {
        var sources = new[]
        {
            $"KeyVault:{key}",      // Azure Key Vault
            $"Secrets:{key}",       // User Secrets (dev)
            key                     // Environment variables
        };

        foreach (var source in sources)
        {
            var value = _configuration[source];
            if (!string.IsNullOrEmpty(value))
            {
                return value;
            }
        }

        return string.Empty;
    }
}
```

---

## 📊 **Configuration Health Checks**

### **Configuration Health Check**
```csharp
using Microsoft.Extensions.Diagnostics.HealthChecks;

public class ConfigurationHealthCheck : IHealthCheck
{
    private readonly YourModuleConfigService _configService;

    public ConfigurationHealthCheck(YourModuleConfigService configService)
    {
        _configService = configService;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var config = _configService.Current;
            var healthData = new Dictionary<string, object>
            {
                ["ModuleName"] = config.ModuleName,
                ["MaxItems"] = config.MaxItems,
                ["EnableFeature"] = config.EnableFeature
            };

            if (string.IsNullOrEmpty(config.ApiSettings.ApiKey))
            {
                return HealthCheckResult.Degraded("API Key not configured", data: healthData);
            }

            return HealthCheckResult.Healthy("Configuration is valid", healthData);
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Configuration health check failed", ex);
        }
    }
}
```

---

## 💡 **Best Practices**

### **Configuration Design Guidelines**
1. **Type Safety**: Always use strongly-typed configuration classes
2. **Validation**: Implement comprehensive validation with data annotations
3. **Default Values**: Provide sensible defaults for all settings
4. **Environment Awareness**: Support different values per environment
5. **Security**: Never store secrets in plain text configuration files

### **Performance Considerations**
```csharp
// Use IOptionsSnapshot for scoped access
public class YourService
{
    private readonly IOptionsSnapshot<YourModuleConfig> _config;

    public YourService(IOptionsSnapshot<YourModuleConfig> config)
    {
        _config = config; // Fresh value per request
    }
}

// Use IOptionsMonitor for singleton services
public class SingletonService
{
    private readonly IOptionsMonitor<YourModuleConfig> _config;

    public SingletonService(IOptionsMonitor<YourModuleConfig> config)
    {
        _config = config; // Supports change notifications
    }
}
```

---

## 🚨 **Troubleshooting**

### **Common Configuration Issues**

| Problem | Cause | Solution |
|---------|-------|----------|
| Configuration not loaded | Wrong section name | Verify section name matches |
| Validation errors | Invalid values | Check validation attributes |
| Hot reload not working | Monitoring disabled | Enable configuration monitoring |
| Secrets not accessible | Wrong secrets setup | Verify secrets configuration |

---

*Configuration Management memberikan fondasi yang solid untuk aplikasi yang dapat dikonfigurasi dan maintainable.*
