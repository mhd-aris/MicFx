# 🔧 MicFx.Infrastructure - Implementation Layer

## 🎯 **Peran dalam Arsitektur**

**MicFx.Infrastructure** adalah **Implementation Layer** dalam arsitektur MicFx Framework yang berfungsi sebagai:

- **Concrete Implementations**: Implementasi nyata dari interface yang didefinisikan di `MicFx.Abstractions`
- **External Dependencies**: Integrasi dengan third-party libraries (Serilog, Swagger, dll)
- **Infrastructure Services**: Layanan infrastruktur seperti logging, caching, security
- **Cross-cutting Concerns**: Implementasi aspek lintas aplikasi (monitoring, documentation)
- **Technology Bridge**: Jembatan antara framework dengan teknologi eksternal

## 🏗️ **Prinsip Design**

### **1. Interface Implementation Pattern**
```csharp
// ✅ Implementasi konkret dari abstraction
public class StructuredLoggerImplementation : IStructuredLogger
{
    private readonly ILogger _logger;
    
    public void LogBusinessOperation(string operation, object? properties = null)
    {
        // Serilog implementation dengan module context
        using (LogContext.PushProperty("Module", GetModuleName()))
        {
            _logger.LogInformation("🔄 {Operation}", operation);
        }
    }
}
```

### **2. Dependency Replacement Pattern**
```csharp
// ✅ Replace default implementations dengan real ones
public static IServiceCollection AddMicFxInfrastructure(this IServiceCollection services)
{
    // Replace throw-exception defaults dengan implementasi nyata
    services.Replace(ServiceDescriptor.Singleton<IStructuredLoggerFactory, StructuredLoggerFactory>());
    services.AddTransient(typeof(IStructuredLogger<>), typeof(StructuredLoggerImplementation<>));
    
    return services;
}
```

### **3. Configuration-Driven Pattern**
```csharp
// ✅ Konfigurasi fleksibel untuk berbagai environment
services.AddMicFxSerilog(configuration, environment, options =>
{
    options.MinimumLevel = LogEventLevel.Information;
    options.EnableConsoleSink = true;
    options.EnableFileSink = environment.IsProduction();
    options.EnableSeqSink = !string.IsNullOrEmpty(seqUrl);
});
```

## 📁 **Struktur Komponen**

### **📋 Extensions/**
```
Extensions/
├── InfrastructureServiceCollectionExtensions.cs    # DI registration untuk Infrastructure
```

**Peran**: Service registration dan configuration extensions
- **Registration Pattern**: Replace default implementations dengan real ones
- **Modular Setup**: Registrasi per layanan (logging, caching, security)
- **Chain Configuration**: Fluent API untuk easy configuration

### **📝 Logging/** (Primary Component)
```
Logging/
├── MicFxLogger.cs                      # Logger helper dengan module context
├── MicFxModuleEnricher.cs             # Serilog enricher untuk module info
├── SerilogExtensions.cs               # Serilog configuration & setup
└── StructuredLoggerImplementation.cs  # IStructuredLogger implementation
```

**Peran**: Structured logging implementation dengan Serilog
- **Module-Aware Logging**: Automatic module detection dan context
- **Structured Data**: JSON logging dengan searchable properties
- **Performance Monitoring**: Built-in timing dan metrics
- **Security Auditing**: Security event logging
- **Multi-Sink**: Console, File, Seq support

### **📚 Swagger/** (Documentation Component)
```
Swagger/
├── AutoResponseExampleFilter.cs       # Automatic response examples
└── SwaggerAutoDiscoveryExtensions.cs  # Auto-discovery Swagger setup
```

**Peran**: API documentation generation dan management
- **Auto-Discovery**: Automatic endpoint detection dari modules
- **Smart Grouping**: Group by module dan routing type (API/MVC/Admin)
- **Response Examples**: Automatic example generation
- **Multi-Route Support**: API, MVC, Admin endpoint documentation

## 🔄 **Integration Patterns**

### **1. Bootstrapping Pattern**
```csharp
// Program.cs - Infrastructure setup
var builder = WebApplication.CreateBuilder(args);

// 1. Add Infrastructure services
builder.Services.AddMicFxInfrastructure();

// 2. Add specific infrastructure components
builder.Services.AddMicFxSerilog(builder.Configuration, builder.Environment);
builder.Services.AddMicFxSwaggerInfrastructure();

var app = builder.Build();

// 3. Use Infrastructure middleware
app.UseMicFxSerilog();
app.UseMicFxSwaggerInfrastructure(app.Environment);
```

### **2. Module Integration Pattern**
```csharp
// Module menggunakan infrastructure services
public class HelloWorldService
{
    private readonly IStructuredLogger<HelloWorldService> _logger;
    
    public HelloWorldService(IStructuredLogger<HelloWorldService> logger)
    {
        _logger = logger; // Infrastructure implementation
    }
    
    public async Task<string> GetGreetingAsync()
    {
        using (_logger.BeginTimedOperation("GetGreeting"))
        {
            _logger.LogBusinessOperation("GenerateGreeting", new { User = "Anonymous" });
            return "Hello from MicFx!";
        }
    }
}
```

### **3. Configuration Integration Pattern**
```csharp
// appsettings.json
{
  "Serilog": {
    "MinimumLevel": "Information",
    "WriteTo": [
      { "Name": "Console" },
      { "Name": "File", "Args": { "path": "logs/micfx-.log" } }
    ]
  }
}
```

## 🎯 **Key Features**

### **🔍 Structured Logging**
- **Module Context**: Automatic module name detection
- **Correlation IDs**: Request tracing support  
- **Performance Metrics**: Built-in timing measurements
- **Security Auditing**: Security event logging
- **Multi-Environment**: Different output untuk dev/prod

### **📖 Auto-Discovery Documentation**
- **Smart Grouping**: Group endpoints by module dan type
- **Automatic Examples**: Generate response examples
- **Route Detection**: Auto-detect API/MVC/Admin routes
- **XML Comments**: Auto-include documentation dari assemblies

### **🔧 Service Integration**
- **Interface Replacement**: Replace default dengan implementations
- **Modular Registration**: Optional service registration
- **Configuration Driven**: Flexible configuration options

## 🚀 **Usage Examples**

### **Basic Infrastructure Setup**
```csharp
// Minimal infrastructure setup
builder.Services.AddMicFxInfrastructure();
```

### **Custom Logging Configuration**
```csharp
builder.Services.AddMicFxSerilog(configuration, environment, options =>
{
    options.MinimumLevel = LogEventLevel.Debug;
    options.EnableSeqSink = true;
    options.SeqServerUrl = "http://localhost:5341";
    
    // Custom sink
    options.AddCustomSink(config => 
        config.WriteTo.Elasticsearch("http://localhost:9200"));
});
```

### **Module Usage Pattern**
```csharp
// Di module service
public class YourModuleService
{
    private readonly IStructuredLogger<YourModuleService> _logger;
    
    public async Task ProcessAsync()
    {
        _logger.LogBusinessOperation("ProcessData", new { Count = 100 });
        
        using (_logger.BeginTimedOperation("DatabaseQuery"))
        {
            // Your business logic
        }
        
        _logger.LogSecurity("DataAccess", userId: "123");
    }
}
```

## 🔗 **Dependencies**

### **Core Dependencies**
- **MicFx.Abstractions**: Interface contracts
- **MicFx.SharedKernel**: Common utilities

### **External Dependencies**
```xml
<!-- Serilog Ecosystem -->
<PackageReference Include="Serilog.AspNetCore" Version="9.0.0" />
<PackageReference Include="Serilog.Sinks.Console" Version="6.0.0" />
<PackageReference Include="Serilog.Sinks.File" Version="6.0.0" />
<PackageReference Include="Serilog.Sinks.Seq" Version="8.0.0" />

<!-- Swagger Documentation -->
<PackageReference Include="Swashbuckle.AspNetCore" Version="9.0.1" />
```

## 📈 **Performance Considerations**

- **Lazy Loading**: Services loaded on-demand
- **Async Logging**: Non-blocking log operations
- **Buffered Writes**: Efficient file I/O
- **Memory Management**: Proper disposal patterns
- **Configuration Caching**: Minimize config reads

## 🎯 **Best Practices**

### **✅ DO**
- Replace default implementations dengan `services.Replace()`
- Use structured logging properties untuk searchability
- Configure different sinks untuk different environments
- Implement proper error handling dalam infrastructure services

### **❌ DON'T**
- Jangan couple infrastructure dengan business logic
- Jangan hardcode configuration values
- Jangan ignore performance implications dari logging
- Jangan expose infrastructure details ke modules

---

> **💡 Tip**: Infrastructure layer ini berfungsi sebagai implementasi konkret yang dapat diganti tanpa mengubah business logic di modules. Selalu design dengan prinsip **Dependency Inversion** dan **Interface Segregation**. 