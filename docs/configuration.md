# ⚙️ Configuration Guide

## 📋 **Overview**

MicFx Framework menggunakan sistem konfigurasi ASP.NET Core yang fleksibel dengan tambahan convention-based configuration untuk modules. Framework mendukung multiple configuration sources dan environment-specific settings.

## 🏗️ **Configuration Architecture**

```mermaid
graph TB
    A[appsettings.json] --> E[Configuration Manager]
    B[appsettings.{Environment}.json] --> E
    C[Environment Variables] --> E
    D[Command Line Args] --> E
    
    E --> F[Framework Configuration]
    E --> G[Module Configurations]
    E --> H[Infrastructure Settings]
    
    F --> I[Core Settings]
    F --> J[Logging Settings]
    F --> K[Security Settings]
    
    G --> L[Auth Module Config]
    G --> M[Custom Module Config]
```

## 📁 **Configuration Files Structure**

```
src/MicFx.Web/
├── appsettings.json                    # Base configuration
├── appsettings.Development.json        # Development overrides
├── appsettings.Staging.json           # Staging overrides
├── appsettings.Production.json        # Production overrides
└── appsettings.Local.json             # Local developer overrides (gitignored)
```

## 🔧 **Base Configuration (appsettings.json)**

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore": "Warning",
      "MicFx": "Information"
    }
  },
  "AllowedHosts": "*",
  
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=MicFxDb;Trusted_Connection=true;MultipleActiveResultSets=true;",
    "RedisConnection": "localhost:6379"
  },
  
  "MicFx": {
    "Framework": {
      "Name": "MicFx Framework",
      "Version": "1.0.0",
      "Environment": "Development",
      "EnableDetailedErrors": true,
      "EnableSwagger": true,
      "EnableHealthChecks": true,
      "EnableMetrics": true
    },
    
    "Modules": {
      "AutoDiscovery": true,
      "LoadOrder": "Priority",
      "EnableHotReload": true,
      "ModulePaths": [
        "src/Modules"
      ]
    },
    
    "Security": {
      "EnableCors": true,
      "AllowedOrigins": ["http://localhost:3000", "https://localhost:3001"],
      "EnableRateLimiting": true,
      "EnableRequestValidation": true
    },
    
    "Performance": {
      "EnableResponseCaching": true,
      "EnableResponseCompression": true,
      "EnableOutputCaching": true,
      "CacheDefaultExpiration": "00:05:00"
    }
  },
  
  "Serilog": {
    "Using": ["Serilog.Sinks.Console", "Serilog.Sinks.File"],
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "System": "Warning",
        "MicFx": "Debug"
      }
    },
    "WriteTo": [
      {
        "Name": "Console",
        "Args": {
          "outputTemplate": "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}"
        }
      },
      {
        "Name": "File",
        "Args": {
          "path": "logs/micfx-.log",
          "rollingInterval": "Day",
          "retainedFileCountLimit": 30,
          "outputTemplate": "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}"
        }
      }
    ],
    "Enrich": ["FromLogContext", "WithMachineName", "WithThreadId"]
  }
}
```

## 🌍 **Environment-Specific Configurations**

### **Development Environment**
```json
// appsettings.Development.json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "MicFx": "Trace"
    }
  },
  
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=MicFxDb_Dev;Trusted_Connection=true;MultipleActiveResultSets=true;"
  },
  
  "MicFx": {
    "Framework": {
      "Environment": "Development",
      "EnableDetailedErrors": true,
      "EnableSwagger": true,
      "EnableDeveloperExceptionPage": true
    },
    
    "Modules": {
      "EnableHotReload": true,
      "LoadTimeout": "00:02:00"
    },
    
    "Security": {
      "EnableCors": true,
      "AllowedOrigins": ["*"],
      "EnableRateLimiting": false
    }
  },
  
  "Serilog": {
    "MinimumLevel": {
      "Default": "Debug",
      "Override": {
        "MicFx": "Trace"
      }
    }
  }
}
```

### **Production Environment**
```json
// appsettings.Production.json
{
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "MicFx": "Information"
    }
  },
  
  "MicFx": {
    "Framework": {
      "Environment": "Production",
      "EnableDetailedErrors": false,
      "EnableSwagger": false,
      "EnableDeveloperExceptionPage": false
    },
    
    "Modules": {
      "EnableHotReload": false,
      "LoadTimeout": "00:01:00"
    },
    
    "Security": {
      "EnableCors": true,
      "AllowedOrigins": ["https://yourdomain.com"],
      "EnableRateLimiting": true,
      "RateLimitRequests": 100,
      "RateLimitWindow": "00:01:00"
    },
    
    "Performance": {
      "EnableResponseCaching": true,
      "EnableResponseCompression": true,
      "CacheDefaultExpiration": "00:15:00"
    }
  },
  
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information"
    },
    "WriteTo": [
      {
        "Name": "File",
        "Args": {
          "path": "/var/log/micfx/micfx-.log",
          "rollingInterval": "Day",
          "retainedFileCountLimit": 90
        }
      }
    ]
  }
}
```

## 🔐 **Module Configurations**

### **Auth Module Configuration**
```json
{
  "MicFx": {
    "Modules": {
      "Auth": {
        "Password": {
          "RequireDigit": true,
          "RequiredLength": 8,
          "RequireNonAlphanumeric": true,
          "RequireUppercase": true,
          "RequireLowercase": true,
          "RequiredUniqueChars": 1
        },
        
        "Lockout": {
          "DefaultLockoutTimeSpan": "00:05:00",
          "MaxFailedAccessAttempts": 5,
          "AllowedForNewUsers": true
        },
        
        "Cookie": {
          "LoginPath": "/auth/login",
          "LogoutPath": "/auth/logout",
          "AccessDeniedPath": "/auth/access-denied",
          "ExpireTimeSpan": "02:00:00",
          "SlidingExpiration": true,
          "CookieName": "MicFx.Auth"
        },
        
        "DefaultRoles": ["User"],
        "AdminRoles": ["Admin", "SuperAdmin"],
        "UserManagementRoles": ["Admin", "SuperAdmin", "UserManager"],
        
        "RoleHierarchy": {
          "SuperAdmin": 1,
          "Admin": 10,
          "UserManager": 20,
          "User": 100
        },
        
        "DefaultAdmin": {
          "Email": "admin@micfx.dev",
          "Password": "Admin123!",
          "FirstName": "System",
          "LastName": "Administrator",
          "Department": "IT",
          "JobTitle": "System Administrator"
        },
        
        "RoutePrefix": "auth"
      }
    }
  }
}
```

### **Custom Module Configuration**
```json
{
  "MicFx": {
    "Modules": {
      "Blog": {
        "PostsPerPage": 10,
        "EnableComments": true,
        "EnableTags": true,
        "MaxPostLength": 10000,
        "AllowedFileTypes": [".jpg", ".png", ".gif"],
        "MaxFileSize": 5242880,
        "CacheExpiration": "00:30:00"
      }
    }
  }
}
```

## 🌐 **Environment Variables**

### **Connection Strings**
```bash
# Database
export ConnectionStrings__DefaultConnection="Server=prod-server;Database=MicFxDb;User Id=app_user;Password=secure_password;TrustServerCertificate=true;"

# Redis
export ConnectionStrings__RedisConnection="redis-server:6379"

# Storage
export ConnectionStrings__StorageConnection="DefaultEndpointsProtocol=https;AccountName=storage;AccountKey=key;"
```

### **Framework Settings**
```bash
# Environment
export ASPNETCORE_ENVIRONMENT="Production"
export MicFx__Framework__Environment="Production"

# Security
export MicFx__Security__EnableCors="true"
export MicFx__Security__AllowedOrigins__0="https://yourdomain.com"

# Performance
export MicFx__Performance__EnableResponseCaching="true"
export MicFx__Performance__CacheDefaultExpiration="00:15:00"
```

### **Module Settings**
```bash
# Auth Module
export MicFx__Modules__Auth__DefaultAdmin__Email="admin@yourcompany.com"
export MicFx__Modules__Auth__DefaultAdmin__Password="YourSecurePassword123!"

# Custom Module
export MicFx__Modules__Blog__PostsPerPage="20"
export MicFx__Modules__Blog__EnableComments="false"
```

## 🔧 **Configuration in Code**

### **Reading Configuration**
```csharp
// In Startup.cs or Program.cs
public void ConfigureServices(IServiceCollection services)
{
    // Bind configuration section
    var authConfig = Configuration.GetSection("MicFx:Modules:Auth").Get<AuthConfig>();
    services.AddSingleton(authConfig);
    
    // Configure options pattern
    services.Configure<BlogConfig>(Configuration.GetSection("MicFx:Modules:Blog"));
}

// In service class
public class BlogService
{
    private readonly BlogConfig _config;
    
    public BlogService(IOptions<BlogConfig> config)
    {
        _config = config.Value;
    }
    
    public async Task<IEnumerable<Post>> GetPostsAsync()
    {
        var pageSize = _config.PostsPerPage;
        // Use configuration...
    }
}
```

### **Configuration Classes**
```csharp
// Configuration model
public class BlogConfig
{
    public int PostsPerPage { get; set; } = 10;
    public bool EnableComments { get; set; } = true;
    public bool EnableTags { get; set; } = true;
    public int MaxPostLength { get; set; } = 10000;
    public string[] AllowedFileTypes { get; set; } = Array.Empty<string>();
    public long MaxFileSize { get; set; } = 5242880; // 5MB
    public TimeSpan CacheExpiration { get; set; } = TimeSpan.FromMinutes(30);
}

// Validation attributes
public class BlogConfigValidation : IValidateOptions<BlogConfig>
{
    public ValidateOptionsResult Validate(string name, BlogConfig options)
    {
        var errors = new List<string>();
        
        if (options.PostsPerPage <= 0)
            errors.Add("PostsPerPage must be greater than 0");
            
        if (options.MaxPostLength <= 0)
            errors.Add("MaxPostLength must be greater than 0");
        
        return errors.Any() 
            ? ValidateOptionsResult.Fail(errors)
            : ValidateOptionsResult.Success;
    }
}
```

## 🗄️ **Database Configuration**

### **Entity Framework Configuration**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=MicFxDb;Trusted_Connection=true;MultipleActiveResultSets=true;",
    "AuthConnection": "Server=(localdb)\\MSSQLLocalDB;Database=MicFxAuth;Trusted_Connection=true;",
    "LoggingConnection": "Server=(localdb)\\MSSQLLocalDB;Database=MicFxLogs;Trusted_Connection=true;"
  },
  
  "EntityFramework": {
    "DefaultTimeout": "00:00:30",
    "EnableSensitiveDataLogging": false,
    "EnableDetailedErrors": false,
    "MaxRetryCount": 3,
    "MaxRetryDelay": "00:00:30"
  }
}
```

### **Database Provider Configuration**
```csharp
// In module startup
protected override void ConfigureModuleServices(IServiceCollection services)
{
    var connectionString = Configuration.GetConnectionString("DefaultConnection");
    
    // SQL Server
    services.AddDbContext<YourDbContext>(options =>
        options.UseSqlServer(connectionString, sqlOptions =>
        {
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 3,
                maxRetryDelay: TimeSpan.FromSeconds(30),
                errorNumbersToAdd: null);
        }));
    
    // PostgreSQL
    services.AddDbContext<YourDbContext>(options =>
        options.UseNpgsql(connectionString));
    
    // SQLite (for testing)
    services.AddDbContext<YourDbContext>(options =>
        options.UseSqlite(connectionString));
}
```

## 📝 **Logging Configuration**

### **Serilog Configuration**
```json
{
  "Serilog": {
    "Using": [
      "Serilog.Sinks.Console",
      "Serilog.Sinks.File",
      "Serilog.Sinks.Seq",
      "Serilog.Sinks.ApplicationInsights"
    ],
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "System": "Warning",
        "Microsoft.EntityFrameworkCore": "Warning",
        "MicFx": "Debug"
      }
    },
    "WriteTo": [
      {
        "Name": "Console",
        "Args": {
          "theme": "Serilog.Sinks.SystemConsole.Themes.AnsiConsoleTheme::Code, Serilog.Sinks.Console",
          "outputTemplate": "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}"
        }
      },
      {
        "Name": "File",
        "Args": {
          "path": "logs/micfx-.log",
          "rollingInterval": "Day",
          "retainedFileCountLimit": 30,
          "fileSizeLimitBytes": 104857600,
          "outputTemplate": "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}"
        }
      },
      {
        "Name": "Seq",
        "Args": {
          "serverUrl": "http://localhost:5341"
        }
      }
    ],
    "Enrich": [
      "FromLogContext",
      "WithMachineName",
      "WithThreadId",
      "WithEnvironmentUserName"
    ],
    "Properties": {
      "Application": "MicFx Framework",
      "Environment": "Development"
    }
  }
}
```

### **Structured Logging Configuration**
```csharp
// Custom enricher
public class ModuleEnricher : ILogEventEnricher
{
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        var moduleProperty = propertyFactory.CreateProperty("Module", "Unknown");
        logEvent.AddPropertyIfAbsent(moduleProperty);
    }
}

// Registration
services.AddSingleton<ILogEventEnricher, ModuleEnricher>();
```

## 🔒 **Security Configuration**

### **CORS Configuration**
```json
{
  "MicFx": {
    "Security": {
      "Cors": {
        "EnableCors": true,
        "PolicyName": "MicFxCorsPolicy",
        "AllowedOrigins": [
          "https://yourdomain.com",
          "https://admin.yourdomain.com"
        ],
        "AllowedMethods": ["GET", "POST", "PUT", "DELETE"],
        "AllowedHeaders": ["*"],
        "AllowCredentials": true,
        "MaxAge": 3600
      }
    }
  }
}
```

### **Rate Limiting Configuration**
```json
{
  "MicFx": {
    "Security": {
      "RateLimit": {
        "EnableRateLimiting": true,
        "GlobalPolicy": {
          "PermitLimit": 100,
          "Window": "00:01:00",
          "QueueLimit": 10
        },
        "Policies": {
          "ApiPolicy": {
            "PermitLimit": 1000,
            "Window": "00:01:00"
          },
          "AuthPolicy": {
            "PermitLimit": 10,
            "Window": "00:01:00"
          }
        }
      }
    }
  }
}
```

## 🚀 **Performance Configuration**

### **Caching Configuration**
```json
{
  "MicFx": {
    "Performance": {
      "Caching": {
        "EnableMemoryCache": true,
        "EnableDistributedCache": true,
        "DefaultExpiration": "00:15:00",
        "MaxSize": 104857600,
        "CompactionPercentage": 0.2,
        
        "Redis": {
          "Configuration": "localhost:6379",
          "InstanceName": "MicFx"
        },
        
        "Policies": {
          "ShortTerm": "00:05:00",
          "MediumTerm": "00:30:00",
          "LongTerm": "02:00:00"
        }
      },
      
      "Compression": {
        "EnableResponseCompression": true,
        "Providers": ["Gzip", "Brotli"],
        "Level": "Optimal"
      }
    }
  }
}
```

## 🏥 **Health Checks Configuration**

```json
{
  "MicFx": {
    "HealthChecks": {
      "EnableHealthChecks": true,
      "Endpoint": "/health",
      "DetailedEndpoint": "/health/detailed",
      "Timeout": "00:00:30",
      
      "Checks": {
        "Database": {
          "Enabled": true,
          "ConnectionString": "DefaultConnection",
          "Timeout": "00:00:10"
        },
        "Redis": {
          "Enabled": true,
          "ConnectionString": "RedisConnection",
          "Timeout": "00:00:05"
        },
        "ExternalApi": {
          "Enabled": false,
          "Url": "https://api.external.com/health",
          "Timeout": "00:00:15"
        }
      }
    }
  }
}
```

## 🔧 **Configuration Validation**

### **Startup Validation**
```csharp
public void ConfigureServices(IServiceCollection services)
{
    // Validate configuration on startup
    services.AddOptions<AuthConfig>()
        .Bind(Configuration.GetSection("MicFx:Modules:Auth"))
        .ValidateDataAnnotations()
        .ValidateOnStart();
    
    // Custom validation
    services.AddSingleton<IValidateOptions<AuthConfig>, AuthConfigValidation>();
}
```

### **Configuration Model with Validation**
```csharp
public class AuthConfig
{
    [Required]
    [EmailAddress]
    public string DefaultAdminEmail { get; set; } = string.Empty;
    
    [Required]
    [MinLength(8)]
    public string DefaultAdminPassword { get; set; } = string.Empty;
    
    [Range(1, 100)]
    public int MaxFailedAccessAttempts { get; set; } = 5;
    
    [Range(1, 1440)] // 1 minute to 24 hours
    public int LockoutTimeSpanMinutes { get; set; } = 5;
}
```

## 🔄 **Configuration Hot Reload**

```csharp
// Enable configuration hot reload
public void ConfigureServices(IServiceCollection services)
{
    services.Configure<BlogConfig>(Configuration.GetSection("MicFx:Modules:Blog"));
    
    // Monitor configuration changes
    services.AddSingleton<IOptionsMonitor<BlogConfig>>();
}

// Use in service
public class BlogService
{
    private readonly IOptionsMonitor<BlogConfig> _configMonitor;
    
    public BlogService(IOptionsMonitor<BlogConfig> configMonitor)
    {
        _configMonitor = configMonitor;
        
        // React to configuration changes
        _configMonitor.OnChange(config =>
        {
            // Handle configuration change
            Logger.LogInformation("Blog configuration updated");
        });
    }
}
```

## 📚 **Best Practices**

### **✅ DO**
- Use environment-specific configuration files
- Validate configuration on startup
- Use strongly-typed configuration classes
- Store secrets in secure configuration providers
- Use configuration hot reload for non-critical settings
- Document all configuration options

### **❌ DON'T**
- Store secrets in appsettings.json files
- Use magic strings for configuration keys
- Skip configuration validation
- Hardcode configuration values in code
- Ignore configuration errors
- Use complex nested configuration structures

## 🔐 **Secrets Management**

### **Development (User Secrets)**
```bash
# Initialize user secrets
dotnet user-secrets init --project src/MicFx.Web

# Set secrets
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=dev-server;Database=MicFxDb;User Id=dev_user;Password=dev_password;" --project src/MicFx.Web
dotnet user-secrets set "MicFx:Modules:Auth:DefaultAdmin:Password" "SecurePassword123!" --project src/MicFx.Web
```

### **Production (Azure Key Vault)**
```csharp
// In Program.cs
public static IHostBuilder CreateHostBuilder(string[] args) =>
    Host.CreateDefaultBuilder(args)
        .ConfigureAppConfiguration((context, config) =>
        {
            if (context.HostingEnvironment.IsProduction())
            {
                var builtConfig = config.Build();
                config.AddAzureKeyVault(
                    builtConfig["KeyVault:Vault"],
                    builtConfig["KeyVault:ClientId"],
                    builtConfig["KeyVault:ClientSecret"]);
            }
        });
```

---

**Configuration complete! Your MicFx Framework is now properly configured for all environments.** ⚙️ 