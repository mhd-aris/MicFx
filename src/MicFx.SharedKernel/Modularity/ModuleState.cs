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

// ModuleStateChangedEventArgs removed for simplicity - use logging instead of complex event system