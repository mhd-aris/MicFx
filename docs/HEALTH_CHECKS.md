# 🏥 MicFx Framework - Health Checks System

## 🎯 **Overview**

Health Checks System dalam MicFx Framework menyediakan monitoring dan diagnostics untuk semua komponen aplikasi, including module-specific health checks, dependency monitoring, dan automated alerting.

---

## 🏗️ **Architecture**

### **Health Check Flow**
```
Health Check Endpoint → Health Check Services → Module Health Checks → Aggregated Response
```

### **Health Check Types**
```
📊 System Health Checks
├── 🔧 Application Health        → Application startup dan configuration
├── 🗄️ Database Health           → Database connectivity dan performance
├── 🌐 External Services         → API dependencies dan third-party services
├── 📦 Module Health             → Module-specific health checks
└── 🔄 Background Services       → Background task dan scheduled jobs
```

---

## 🔧 **Implementation**

### **Basic Health Check Setup**
```csharp
// Program.cs atau Startup.cs
builder.Services.AddHealthChecks()
    .AddCheck("self", () => HealthCheckResult.Healthy())
    .AddDbContext<ApplicationDbContext>()
    .AddUrlGroup(new Uri("https://api.external-service.com/health"), "external-api")
    .AddMemoryHealthCheck("memory", failureThreshold: 1024 * 1024 * 1024) // 1GB
    .AddDiskStorageHealthCheck(options =>
    {
        options.AddDriveByPath("C:\\", "System Drive", 2048); // 2GB free space required
    });

// Configure health check endpoint
app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = WriteResponse,
    ResultStatusCodes =
    {
        [HealthStatus.Healthy] = StatusCodes.Status200OK,
        [HealthStatus.Degraded] = StatusCodes.Status200OK,
        [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable
    }
});

// Detailed health check endpoint
app.MapHealthChecks("/health/detailed", new HealthCheckOptions
{
    ResponseWriter = WriteDetailedResponse,
    Predicate = _ => true
});
```

### **Module-Specific Health Checks**
```csharp
// HelloWorld Module Health Check
public class HelloWorldHealthCheck : IHealthCheck
{
    private readonly IHelloWorldService _helloWorldService;
    private readonly ILogger<HelloWorldHealthCheck> _logger;

    public HelloWorldHealthCheck(
        IHelloWorldService helloWorldService,
        ILogger<HelloWorldHealthCheck> logger)
    {
        _helloWorldService = helloWorldService;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Check service availability
            var isServiceHealthy = await _helloWorldService.IsHealthyAsync(cancellationToken);
            
            if (!isServiceHealthy)
            {
                return HealthCheckResult.Unhealthy("HelloWorld service is not responding");
            }

            // Check configuration
            var config = await _helloWorldService.GetConfigurationHealthAsync(cancellationToken);
            
            var healthData = new Dictionary<string, object>
            {
                ["LastCheck"] = DateTime.UtcNow,
                ["ServiceStatus"] = "Healthy",
                ["Configuration"] = config,
                ["Version"] = "1.0.0"
            };

            // Performance check
            var stopwatch = Stopwatch.StartNew();
            await _helloWorldService.PerformHealthCheckAsync(cancellationToken);
            stopwatch.Stop();

            healthData["ResponseTime"] = stopwatch.ElapsedMilliseconds;

            if (stopwatch.ElapsedMilliseconds > 1000) // 1 second threshold
            {
                return HealthCheckResult.Degraded("HelloWorld service is responding slowly", 
                    data: healthData);
            }

            return HealthCheckResult.Healthy("HelloWorld service is healthy", healthData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "HelloWorld health check failed");
            return HealthCheckResult.Unhealthy("HelloWorld health check failed", ex, 
                new Dictionary<string, object>
                {
                    ["Error"] = ex.Message,
                    ["LastCheck"] = DateTime.UtcNow
                });
        }
    }
}
```

### **Database Health Check**
```csharp
public class DatabaseHealthCheck : IHealthCheck
{
    private readonly IDbConnection _connection;
    private readonly ILogger<DatabaseHealthCheck> _logger;

    public DatabaseHealthCheck(IDbConnection connection, ILogger<DatabaseHealthCheck> logger)
    {
        _connection = connection;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var stopwatch = Stopwatch.StartNew();
            
            // Test basic connectivity
            await _connection.OpenAsync(cancellationToken);
            
            // Test query execution
            var result = await _connection.QuerySingleAsync<int>("SELECT 1");
            
            stopwatch.Stop();

            var healthData = new Dictionary<string, object>
            {
                ["ConnectionState"] = _connection.State.ToString(),
                ["ResponseTime"] = stopwatch.ElapsedMilliseconds,
                ["LastCheck"] = DateTime.UtcNow,
                ["QueryResult"] = result
            };

            if (stopwatch.ElapsedMilliseconds > 500) // 500ms threshold
            {
                return HealthCheckResult.Degraded("Database is responding slowly", 
                    data: healthData);
            }

            return HealthCheckResult.Healthy("Database is healthy", healthData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database health check failed");
            return HealthCheckResult.Unhealthy("Database connection failed", ex);
        }
        finally
        {
            if (_connection.State == ConnectionState.Open)
            {
                await _connection.CloseAsync();
            }
        }
    }
}
```

---

## 📊 **Health Check Response Format**

### **Standard Response**
```json
{
  "status": "Healthy",
  "totalDuration": "00:00:01.234",
  "entries": {
    "self": {
      "status": "Healthy",
      "duration": "00:00:00.001",
      "data": {}
    },
    "database": {
      "status": "Healthy",
      "duration": "00:00:00.045",
      "data": {
        "connectionState": "Open",
        "responseTime": 45,
        "lastCheck": "2024-01-15T10:30:00.123Z"
      }
    },
    "hello-world": {
      "status": "Healthy",
      "duration": "00:00:00.123",
      "data": {
        "serviceStatus": "Healthy",
        "responseTime": 123,
        "version": "1.0.0",
        "lastCheck": "2024-01-15T10:30:00.123Z"
      }
    },
    "external-api": {
      "status": "Degraded",
      "duration": "00:00:01.500",
      "data": {
        "responseTime": 1500,
        "message": "External API is responding slowly"
      }
    }
  }
}
```

### **Unhealthy Response**
```json
{
  "status": "Unhealthy",
  "totalDuration": "00:00:02.345",
  "entries": {
    "database": {
      "status": "Unhealthy",
      "duration": "00:00:02.000",
      "exception": "Connection timeout",
      "data": {
        "error": "Unable to connect to database",
        "lastCheck": "2024-01-15T10:30:00.123Z"
      }
    }
  }
}
```

---

## 🔧 **Module Registration**

### **Module Health Check Registration**
```csharp
// Module Startup.cs
public class HelloWorldStartup : ModuleStartupBase
{
    protected override void ConfigureModuleServices(IServiceCollection services)
    {
        // Register module health check
        services.AddHealthChecks()
            .AddCheck<HelloWorldHealthCheck>("hello-world")
            .AddCheck<HelloWorldDatabaseHealthCheck>("hello-world-database")
            .AddCheck<HelloWorldExternalServiceHealthCheck>("hello-world-external-service");

        // Register health check dependencies
        services.AddScoped<IHelloWorldHealthService, HelloWorldHealthService>();
    }
}
```

### **Conditional Health Checks**
```csharp
protected override void ConfigureModuleServices(IServiceCollection services)
{
    var healthChecksBuilder = services.AddHealthChecks();

    // Always add basic module health check
    healthChecksBuilder.AddCheck<HelloWorldHealthCheck>("hello-world");

    // Conditionally add database health check
    if (Configuration.GetConnectionString("HelloWorldDatabase") != null)
    {
        healthChecksBuilder.AddCheck<HelloWorldDatabaseHealthCheck>("hello-world-database");
    }

    // Conditionally add external service health check
    if (Configuration.GetValue<bool>("HelloWorld:EnableExternalService"))
    {
        healthChecksBuilder.AddCheck<HelloWorldExternalServiceHealthCheck>("hello-world-external");
    }
}
```

---

## 🎛️ **Configuration**

### **Health Check Configuration**
```json
{
  "MicFx": {
    "HealthChecks": {
      "EnableDetailedErrors": true,
      "EnableHealthChecksUI": true,
      "CheckTimeoutSeconds": 30,
      "CacheDurationSeconds": 30,
      "HealthCheckEndpoint": "/health",
      "DetailedHealthCheckEndpoint": "/health/detailed"
    },
    "Modules": {
      "HelloWorld": {
        "HealthCheck": {
          "Enabled": true,
          "TimeoutSeconds": 10,
          "PerformanceThresholdMs": 1000,
          "EnableExternalServiceCheck": true,
          "ExternalServiceUrl": "https://api.external-service.com/health"
        }
      }
    }
  }
}
```

### **Health Checks UI Configuration**
```json
{
  "HealthChecksUI": {
    "HealthChecks": [
      {
        "Name": "MicFx Application",
        "Uri": "https://localhost:5001/health"
      }
    ],
    "EvaluationTimeInSeconds": 10,
    "MinimumSecondsBetweenFailureNotifications": 60
  }
}
```

---

## 📈 **Monitoring & Alerting**

### **Health Check Metrics**
```csharp
public class HealthCheckMetricsCollector
{
    private readonly IMetricsCollector _metricsCollector;
    private readonly ILogger<HealthCheckMetricsCollector> _logger;

    public HealthCheckMetricsCollector(
        IMetricsCollector metricsCollector,
        ILogger<HealthCheckMetricsCollector> logger)
    {
        _metricsCollector = metricsCollector;
        _logger = logger;
    }

    public async Task CollectHealthCheckMetrics(HealthReport report)
    {
        // Overall health status
        _metricsCollector.RecordGauge("health_status", 
            report.Status == HealthStatus.Healthy ? 1 : 0,
            new { status = report.Status.ToString() });

        // Individual health check metrics
        foreach (var entry in report.Entries)
        {
            _metricsCollector.RecordGauge("health_check_status",
                entry.Value.Status == HealthStatus.Healthy ? 1 : 0,
                new { check_name = entry.Key, status = entry.Value.Status.ToString() });

            _metricsCollector.RecordHistogram("health_check_duration",
                entry.Value.Duration.TotalMilliseconds,
                new { check_name = entry.Key });
        }

        // Total duration
        _metricsCollector.RecordHistogram("health_check_total_duration",
            report.TotalDuration.TotalMilliseconds);

        _logger.LogInformation("Health check metrics collected: {Status} in {Duration}ms",
            report.Status, report.TotalDuration.TotalMilliseconds);
    }
}
```

### **Alerting Service**
```csharp
public class HealthCheckAlertingService
{
    private readonly INotificationService _notificationService;
    private readonly ILogger<HealthCheckAlertingService> _logger;
    private readonly Dictionary<string, DateTime> _lastAlertTimes = new();

    public HealthCheckAlertingService(
        INotificationService notificationService,
        ILogger<HealthCheckAlertingService> logger)
    {
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task ProcessHealthCheckResult(HealthReport report)
    {
        // Check overall health
        if (report.Status == HealthStatus.Unhealthy)
        {
            await SendAlertIfNeeded("overall_health", 
                $"Application health check failed: {report.Status}",
                AlertLevel.Critical);
        }

        // Check individual components
        foreach (var entry in report.Entries)
        {
            if (entry.Value.Status == HealthStatus.Unhealthy)
            {
                await SendAlertIfNeeded(entry.Key,
                    $"Health check failed for {entry.Key}: {entry.Value.Description}",
                    AlertLevel.High);
            }
            else if (entry.Value.Status == HealthStatus.Degraded)
            {
                await SendAlertIfNeeded(entry.Key,
                    $"Health check degraded for {entry.Key}: {entry.Value.Description}",
                    AlertLevel.Medium);
            }
        }
    }

    private async Task SendAlertIfNeeded(string checkName, string message, AlertLevel level)
    {
        var now = DateTime.UtcNow;
        var alertKey = $"{checkName}_{level}";

        // Rate limiting - don't send alerts too frequently
        if (_lastAlertTimes.TryGetValue(alertKey, out var lastAlert) && 
            now.Subtract(lastAlert).TotalMinutes < 5)
        {
            return;
        }

        _lastAlertTimes[alertKey] = now;

        await _notificationService.SendAlertAsync(new Alert
        {
            Title = $"Health Check Alert - {checkName}",
            Message = message,
            Level = level,
            Timestamp = now,
            Source = "HealthCheckSystem"
        });

        _logger.LogWarning("Health check alert sent for {CheckName}: {Message}", 
            checkName, message);
    }
}
```

---

## 💡 **Best Practices**

### **Health Check Design Guidelines**
1. **Fast Execution**: Keep health checks lightweight and fast
2. **Meaningful Checks**: Test actual functionality, not just connectivity
3. **Proper Timeouts**: Set appropriate timeouts for external dependencies
4. **Descriptive Messages**: Provide clear error messages and context
5. **Graceful Degradation**: Use Degraded status for non-critical issues

### **Performance Considerations**
```csharp
// ✅ Good - Lightweight health check
public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken)
{
    using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(5));
    using var combined = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeout.Token);
    
    try
    {
        // Quick connectivity test
        await _connection.OpenAsync(combined.Token);
        return HealthCheckResult.Healthy();
    }
    catch (OperationCanceledException) when (timeout.Token.IsCancellationRequested)
    {
        return HealthCheckResult.Degraded("Health check timed out");
    }
}

// ❌ Bad - Heavy health check
public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken)
{
    // Don't do this - expensive operations
    var allUsers = await _userService.GetAllUsersAsync(); // Could be millions of records
    var processedData = await _dataService.ProcessAllDataAsync(); // Heavy computation
    
    return HealthCheckResult.Healthy();
}
```

---

## 🚨 **Troubleshooting**

### **Common Health Check Issues**

| Problem | Cause | Solution |
|---------|-------|----------|
| Health check timeouts | Long-running operations | Implement proper timeouts and lightweight checks |
| False positives | Overly sensitive checks | Add proper thresholds and degraded states |
| Missing dependencies | Service not registered | Ensure all dependencies are registered in DI |
| Slow responses | Heavy operations in health checks | Use async operations and optimize check logic |

### **Debugging Health Checks**
```csharp
// Enable detailed health check logging
builder.Services.AddLogging(config =>
{
    config.AddConsole();
    config.SetMinimumLevel(LogLevel.Debug);
});

// Add health check logging
builder.Services.AddHealthChecks()
    .AddCheck("test", () =>
    {
        var logger = LoggerFactory.Create(b => b.AddConsole()).CreateLogger("HealthCheck");
        logger.LogInformation("Executing test health check");
        return HealthCheckResult.Healthy("Test completed");
    });
```

---

*Health Checks System menyediakan monitoring dan observability yang komprehensif untuk aplikasi MicFx.*
