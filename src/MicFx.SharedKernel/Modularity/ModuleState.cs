namespace MicFx.SharedKernel.Modularity;

/// <summary>
/// Enum that describes the lifecycle status of a module
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
    /// Module has been loaded but not yet started
    /// </summary>
    Loaded = 2,

    /// <summary>
    /// Module is in the startup process
    /// </summary>
    Starting = 3,

    /// <summary>
    /// Module is running and ready for use
    /// </summary>
    Started = 4,

    /// <summary>
    /// Module is in the shutdown process
    /// </summary>
    Stopping = 5,

    /// <summary>
    /// Module has been stopped
    /// </summary>
    Stopped = 6,

    /// <summary>
    /// Module is in an error state
    /// </summary>
    Error = 7,

    /// <summary>
    /// Module is in the reload process (hot reload)
    /// </summary>
    Reloading = 8
}

/// <summary>
/// Interface for modules that support lifecycle events
/// </summary>
public interface IModuleLifecycle
{
    /// <summary>
    /// Called when the module is being loaded
    /// </summary>
    Task OnLoadingAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Called when the module has been loaded
    /// </summary>
    Task OnLoadedAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Called when the module is starting
    /// </summary>
    Task OnStartingAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Called when the module has started
    /// </summary>
    Task OnStartedAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Called when the module is stopping
    /// </summary>
    Task OnStoppingAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Called when the module has stopped
    /// </summary>
    Task OnStoppedAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Called when the module encounters an error
    /// </summary>
    Task OnErrorAsync(Exception error, CancellationToken cancellationToken = default);
}

/// <summary>
/// Interface for modules that support hot reload
/// </summary>
public interface IModuleHotReload
{
    /// <summary>
    /// Called when the module is being reloaded
    /// </summary>
    Task OnReloadingAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Called when the module has been reloaded
    /// </summary>
    Task OnReloadedAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets resources that need to be cleaned up before reload
    /// </summary>
    Task<IEnumerable<IDisposable>> GetReloadResourcesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates whether the module is in a safe state for reload
    /// </summary>
    Task<bool> CanReloadAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Interface for module health check
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