namespace MicFx.SharedKernel.Modularity;

/// <summary>
/// Simplified enum that describes the lifecycle status of a module
/// Removed complex states for simplicity and maintainability
/// </summary>
public enum ModuleState
{
    /// <summary>
    /// Module has not been loaded into memory
    /// </summary>
    NotLoaded = 0,

    /// <summary>
    /// Module is in the loading process
    /// </summary>
    Loading = 1,

    /// <summary>
    /// Module has been loaded and is ready for use
    /// </summary>
    Loaded = 2,

    /// <summary>
    /// Module is in an error state
    /// </summary>
    Error = 3
}

/// <summary>
/// Simplified interface for modules that support basic lifecycle events
/// Reduced from 8 hooks to 2 essential hooks
/// </summary>
public interface IModuleLifecycle
{
    /// <summary>
    /// Called when the module is being initialized
    /// </summary>
    Task InitializeAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Called when the module is being shut down
    /// </summary>
    Task ShutdownAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Interface for module health check (simplified)
/// </summary>
public interface IModuleHealthCheck
{
    /// <summary>
    /// Checks the health status of the module
    /// </summary>
    Task<ModuleHealthStatus> CheckHealthAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets detailed information about the health status
    /// </summary>
    Task<ModuleHealthDetails> GetHealthDetailsAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Module health status
/// </summary>
public enum ModuleHealthStatus
{
    Healthy,
    Degraded,
    Unhealthy
}

/// <summary>
/// Detailed module health information
/// </summary>
public class ModuleHealthDetails
{
    public ModuleHealthStatus Status { get; set; }
    public string? Description { get; set; }
    public Dictionary<string, object> Data { get; set; } = new();
    public DateTime CheckedAt { get; set; } = DateTime.UtcNow;
    public TimeSpan Duration { get; set; }
    public Exception? Exception { get; set; }
}

/// <summary>
/// Module state information with metadata
/// </summary>
public class ModuleStateInfo
{
    public string ModuleName { get; set; } = string.Empty;
    public ModuleState State { get; set; }
    public DateTime LastStateChange { get; set; } = DateTime.UtcNow;
    public string? StateDescription { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
    public Exception? LastError { get; set; }
}

/// <summary>
/// Event arguments for module state changes
/// </summary>
public class ModuleStateChangedEventArgs : EventArgs
{
    public string ModuleName { get; set; } = string.Empty;
    public ModuleState OldState { get; set; }
    public ModuleState NewState { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string? Description { get; set; }
}