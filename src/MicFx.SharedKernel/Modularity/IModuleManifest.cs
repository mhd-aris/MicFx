namespace MicFx.SharedKernel.Modularity
{
    /// <summary>
    /// Pragmatic module manifest interface focused on essential properties
    /// </summary>
    public interface IModuleManifest
    {
        /// <summary>
        /// Module name (required) - must be unique
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Module version (required) - semantic versioning recommended
        /// </summary>
        string Version { get; }

        /// <summary>
        /// Module description (optional)
        /// </summary>
        string Description { get; }

        /// <summary>
        /// Module author (optional)
        /// </summary>
        string Author { get; }

        /// <summary>
        /// Module category for organization and discovery
        /// </summary>
        ModuleCategory Category { get; }

        /// <summary>
        /// Custom tags for additional metadata (max 3 recommended)
        /// </summary>
        string[] CustomTags { get; }

        /// <summary>
        /// Required module dependency 
        /// </summary>
        string? RequiredModule { get; }

        /// <summary>
        /// Whether this module is critical for system operation
        /// </summary>
        bool IsCritical { get; }
       
        /// <summary>
        /// Module priority for startup ordering (lower number = higher priority, loads first)
        /// </summary>
        int Priority { get; }
    }

    /// <summary>
    /// Framework-defined module categories for consistency
    /// </summary>
    public enum ModuleCategory 
    { 
        Core,           // Framework core modules
        Business,       // Business domain modules  
        Integration,    // External integration modules
        Demo            // Demo/PoC modules
    }
}