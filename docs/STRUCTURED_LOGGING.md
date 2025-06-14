# 📝 MicFx Framework - Structured Logging

## 🎯 **Overview**

Structured Logging dalam MicFx Framework menggunakan Serilog untuk menyediakan logging yang kaya, terstruktur, dan dapat dianalisis dengan mudah. Framework secara otomatis menambahkan context module dan mendukung berbagai output format.

---

## 🏗️ **Architecture**

### **Logging Flow**
```
Application Event → Serilog Logger → Enrichers → Sinks → Output (Console/File/External)
```

### **Components**
```
┌─────────────────────────────────────┐
│        Application Code             │ ← Business logic logging
├─────────────────────────────────────┤
│        MicFx Logging Abstractions   │ ← IStructuredLogger<T>
├─────────────────────────────────────┤
│        Serilog Implementation       │ ← Core logging engine
├─────────────────────────────────────┤
│        Custom Enrichers             │ ← Module context, correlation
├─────────────────────────────────────┤
│        Output Sinks                 │ ← Console, File, External systems
└─────────────────────────────────────┘
```

---

## ⚙️ **Configuration**

### **appsettings.json Configuration**
```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "System": "Warning",
        "Microsoft.Hosting.Lifetime": "Information",
        "MicFx": "Debug"
      }
    },
    "WriteTo": [
      {
        "Name": "Console",
        "Args": {
          "theme": "Serilog.Sinks.SystemConsole.Themes.AnsiConsoleTheme::Code, Serilog.Sinks.Console",
          "outputTemplate": "[{Timestamp:HH:mm:ss} {Level:u3}] {Module} {Message:lj}{NewLine}{Exception}"
        }
      },
      {
        "Name": "File",
        "Args": {
          "path": "logs/micfx-.log",
          "rollingInterval": "Day",
          "retainedFileCountLimit": 7,
          "buffered": true,
          "flushToDiskInterval": "00:00:01",
          "outputTemplate": "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Module} {Message:lj} {Properties:j}{NewLine}{Exception}"
        }
      }
    ],
    "Enrich": [
      "FromLogContext",
      "WithMachineName",
      "WithProcessId",
      "WithThreadId"
    ],
    "Properties": {
      "Application": "MicFx"
    }
  }
}
```

### **Environment-Specific Configuration**
```json
// appsettings.Development.json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Debug"
    },
    "WriteTo": [
      {
        "Name": "Console",
        "Args": {
          "theme": "Serilog.Sinks.SystemConsole.Themes.AnsiConsoleTheme::Code, Serilog.Sinks.Console"
        }
      }
    ]
  }
}

// appsettings.Production.json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Warning"
    },
    "WriteTo": [
      {
        "Name": "File",
        "Args": {
          "path": "logs/micfx-.log",
          "formatter": "Serilog.Formatting.Compact.CompactJsonFormatter, Serilog.Formatting.Compact"
        }
      },
      {
        "Name": "Seq",
        "Args": {
          "serverUrl": "http://localhost:5341"
        }
      }
    ]
  }
}
```

---

## 🔧 **Implementation**

### **IStructuredLogger Interface**
```csharp
using MicFx.Abstractions.Logging;

namespace MicFx.Modules.YourModule.Services;

public class YourService
{
    private readonly IStructuredLogger<YourService> _logger;

    public YourService(IStructuredLogger<YourService> logger)
    {
        _logger = logger;
    }

    public async Task ProcessDataAsync(string dataId)
    {
        // Basic structured logging
        _logger.LogInformation("Processing data {DataId}", dataId);

        // Business operation logging
        _logger.LogBusinessOperation("ProcessData", 
            new { DataId = dataId, Timestamp = DateTime.UtcNow }, 
            "Starting data processing");

        try
        {
            // Performance tracking
            using var timer = _logger.BeginTimedOperation("DatabaseQuery", 
                new { DataId = dataId });

            await ProcessInternalAsync(dataId);

            // Success logging
            _logger.LogBusinessSuccess("ProcessData", 
                new { DataId = dataId, ProcessedAt = DateTime.UtcNow }, 
                "Data processed successfully");
        }
        catch (Exception ex)
        {
            // Error logging with context
            _logger.LogBusinessError("ProcessData", ex, 
                new { DataId = dataId, FailedAt = DateTime.UtcNow }, 
                "Failed to process data");
            throw;
        }
    }

    public async Task SecuritySensitiveOperationAsync(string userId, string action)
    {
        // Security audit logging
        _logger.LogSecurity("UserAction", userId, 
            new 
            { 
                Action = action,
                IPAddress = GetClientIPAddress(),
                UserAgent = GetUserAgent(),
                Timestamp = DateTime.UtcNow
            }, 
            $"User performed {action}");
    }
}
```

### **Advanced Logging Patterns**
```csharp
public class AdvancedLoggingService
{
    private readonly IStructuredLogger<AdvancedLoggingService> _logger;

    public AdvancedLoggingService(IStructuredLogger<AdvancedLoggingService> logger)
    {
        _logger = logger;
    }

    // Correlation tracking
    public async Task ProcessWithCorrelationAsync(string orderId)
    {
        using var correlationScope = _logger.BeginCorrelationScope("OrderProcessing", orderId);
        
        _logger.LogInformation("Starting order processing {OrderId}", orderId);
        
        await Step1Async(orderId);
        await Step2Async(orderId);
        await Step3Async(orderId);
        
        _logger.LogInformation("Completed order processing {OrderId}", orderId);
    }

    // Performance monitoring
    public async Task MonitoredOperationAsync()
    {
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            using var performanceLogger = _logger.BeginTimedOperation("ComplexOperation", 
                new { ExpectedDurationMs = 5000 });

            // Simulate work
            await Task.Delay(2000);
            
            // Log intermediate metrics
            _logger.LogPerformanceMetric("IntermediateStep", stopwatch.ElapsedMilliseconds, 
                new { Step = "DataRetrieval" });

            await Task.Delay(1000);
            
            _logger.LogPerformanceMetric("CompletedStep", stopwatch.ElapsedMilliseconds, 
                new { Step = "DataProcessing" });
        }
        finally
        {
            stopwatch.Stop();
            _logger.LogPerformanceMetric("TotalOperation", stopwatch.ElapsedMilliseconds, 
                new { TotalSteps = 2 });
        }
    }

    // Structured data logging
    public async Task LogStructuredDataAsync(Order order)
    {
        _logger.LogInformation("Processing order {@Order}", order);
        
        // Log with structured properties
        _logger.LogInformation("Order summary: {OrderId}, {CustomerId}, {TotalAmount:C}, {ItemCount}", 
            order.Id, order.CustomerId, order.TotalAmount, order.Items.Count);

        // Log complex objects
        _logger.LogBusinessOperation("OrderValidation", 
            new 
            {
                Order = new 
                {
                    order.Id,
                    order.CustomerId,
                    order.TotalAmount,
                    ItemCount = order.Items.Count,
                    Categories = order.Items.Select(i => i.Category).Distinct().ToArray()
                },
                ValidationRules = new[] { "Amount", "Items", "Customer" }
            }, 
            "Validating order structure");
    }
}
```

---

## 🏷️ **Module Context Enrichment**

### **Automatic Module Detection**
```csharp
// Framework automatically adds module context to all logs
// Example output:
[12:34:56 INF] HelloWorld Processing greeting request for user "john.doe"
[12:34:57 DBG] Auth Validating JWT token
[12:34:58 WRN] UserManagement User not found in cache, querying database
```

### **Custom Context Enrichment**
```csharp
public class CustomEnricher : ILogEventEnricher
{
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        // Add custom properties
        logEvent.AddOrUpdateProperty(propertyFactory.CreateProperty("Environment", Environment.MachineName));
        logEvent.AddOrUpdateProperty(propertyFactory.CreateProperty("ApplicationVersion", GetApplicationVersion()));
        
        // Add user context if available
        if (TryGetCurrentUser(out var userId))
        {
            logEvent.AddOrUpdateProperty(propertyFactory.CreateProperty("UserId", userId));
        }

        // Add request context if in HTTP context
        if (TryGetHttpContext(out var httpContext))
        {
            logEvent.AddOrUpdateProperty(propertyFactory.CreateProperty("RequestId", httpContext.TraceIdentifier));
            logEvent.AddOrUpdateProperty(propertyFactory.CreateProperty("IPAddress", httpContext.Connection.RemoteIpAddress?.ToString()));
        }
    }
}
```

---

## 📊 **Log Analysis & Monitoring**

### **Structured Query Examples**
```sql
-- Seq queries
// Find all errors in the last hour
@Level = 'Error' and @Timestamp > Now() - 1h

// Find performance issues
has(ElapsedMs) and ElapsedMs > 5000

// Find specific module activity
Module = 'HelloWorld' and @Level = 'Information'

// Find security events
EventType = 'Security' and Action = 'Login'

// Correlation tracking
CorrelationId = 'specific-correlation-id'
```

### **ELK Stack Integration**
```json
// Logstash configuration
{
  "timestamp": "2024-01-15T10:30:00.123Z",
  "level": "Information",
  "module": "HelloWorld",
  "message": "Processing greeting request",
  "properties": {
    "UserId": "john.doe",
    "RequestId": "abc-123",
    "ElapsedMs": 45
  },
  "exception": null
}
```

---

## 🎯 **Best Practices**

### **Logging Guidelines**
1. **Use Structured Logging**: Always use structured properties instead of string interpolation
2. **Include Context**: Add relevant business context to all log entries
3. **Performance Aware**: Use performance tracking for critical operations
4. **Security Conscious**: Never log sensitive information (passwords, tokens, etc.)
5. **Consistent Naming**: Use consistent property names across modules

### **Good vs Bad Examples**
```csharp
// ❌ Bad - String interpolation, no structure
_logger.LogInformation($"User {userId} processed order {orderId} with amount {amount}");

// ✅ Good - Structured logging with context
_logger.LogInformation("User processed order {UserId} {OrderId} {Amount:C}", 
    userId, orderId, amount);

// ✅ Better - Business operation logging
_logger.LogBusinessOperation("OrderProcessing", 
    new { UserId = userId, OrderId = orderId, Amount = amount }, 
    "Order processed successfully");

// ❌ Bad - No context for errors
_logger.LogError(ex, "An error occurred");

// ✅ Good - Error with context
_logger.LogError(ex, "Failed to process order {OrderId} for user {UserId}", 
    orderId, userId);

// ✅ Better - Business error logging
_logger.LogBusinessError("OrderProcessing", ex, 
    new { OrderId = orderId, UserId = userId, Stage = "Payment" }, 
    "Order processing failed during payment");
```

### **Performance Considerations**
```csharp
// Use log levels appropriately
_logger.LogDebug("Detailed debugging info {Details}", details); // Development only
_logger.LogInformation("Business event {Event}", eventData);    // Important events
_logger.LogWarning("Recoverable issue {Issue}", issue);        // Problems that don't stop processing
_logger.LogError(ex, "Unrecoverable error {Error}", error);    // Failures that stop processing

// Avoid expensive operations in logging
// ❌ Bad
_logger.LogInformation("Processing items {Items}", items.Select(i => i.ToString()).ToArray());

// ✅ Good
_logger.LogInformation("Processing {ItemCount} items", items.Count);

// Use conditional logging for expensive operations
if (_logger.IsEnabled(LogLevel.Debug))
{
    var expensiveData = GenerateExpensiveDebuggingData();
    _logger.LogDebug("Debug data: {@Data}", expensiveData);
}
```

---

## 🔍 **Troubleshooting & Debugging**

### **Common Logging Issues**

| Problem | Cause | Solution |
|---------|-------|----------|
| Logs not appearing | Wrong log level | Check minimum log level configuration |
| No module context | Missing enricher | Verify MicFx enrichers are registered |
| Poor performance | Synchronous logging | Enable async logging sinks |
| Missing correlation | No correlation scope | Use correlation scopes for related operations |
| Large log files | No rotation | Configure rolling file policies |

### **Debugging Configuration**
```csharp
// Enable Serilog self-logging for troubleshooting
var loggerConfig = new LoggerConfiguration()
    .WriteTo.Debug() // Enable debug output
    .ReadFrom.Configuration(configuration);

// Check if specific loggers are working
Log.Information("Serilog is working");
Log.Warning("This is a test warning");
Log.Error("This is a test error");

// Verify enrichers are working
using (LogContext.PushProperty("TestProperty", "TestValue"))
{
    Log.Information("Testing enricher functionality");
}
```

### **Log Analysis Tools**
```bash
# Using grep for quick analysis
grep "ERROR" logs/micfx-*.log | head -10
grep "Module.*HelloWorld" logs/micfx-*.log

# Using jq for JSON logs
cat logs/micfx-*.log | jq 'select(.Level == "Error")'
cat logs/micfx-*.log | jq 'select(.Properties.ElapsedMs > 1000)'

# Performance analysis
cat logs/micfx-*.log | jq 'select(has("ElapsedMs")) | .ElapsedMs' | sort -n | tail -10
```

---

## 🚀 **Advanced Features**

### **Custom Sinks**
```csharp
public class DatabaseSink : ILogEventSink
{
    private readonly IDbConnection _connection;

    public DatabaseSink(IDbConnection connection)
    {
        _connection = connection;
    }

    public void Emit(LogEvent logEvent)
    {
        var logEntry = new
        {
            Timestamp = logEvent.Timestamp,
            Level = logEvent.Level.ToString(),
            Message = logEvent.RenderMessage(),
            Exception = logEvent.Exception?.ToString(),
            Properties = logEvent.Properties.ToDictionary(p => p.Key, p => p.Value.ToString())
        };

        // Insert into database
        _connection.Execute("INSERT INTO Logs (...) VALUES (...)", logEntry);
    }
}

// Registration
builder.Services.AddSerilog((services, logger) =>
{
    logger.WriteTo.Sink(new DatabaseSink(services.GetRequiredService<IDbConnection>()));
});
```

### **Alerting Integration**
```csharp
public class AlertingSink : ILogEventSink
{
    private readonly IAlertingService _alertingService;

    public AlertingSink(IAlertingService alertingService)
    {
        _alertingService = alertingService;
    }

    public void Emit(LogEvent logEvent)
    {
        if (logEvent.Level >= LogEventLevel.Error)
        {
            var alert = new Alert
            {
                Level = logEvent.Level.ToString(),
                Message = logEvent.RenderMessage(),
                Timestamp = logEvent.Timestamp,
                Module = logEvent.Properties.TryGetValue("Module", out var module) 
                    ? module.ToString() : "Unknown"
            };

            _alertingService.SendAlertAsync(alert);
        }
    }
}
```

---

*Structured Logging menyediakan observability yang komprehensif untuk monitoring dan debugging aplikasi MicFx.* 