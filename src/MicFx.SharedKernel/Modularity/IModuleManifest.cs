namespace MicFx.SharedKernel.Modularity
{
    /// <summary>
    /// Core module manifest with essential properties only
    /// SIMPLIFIED: Removed over-complex properties and separated concerns
    /// </summary>
    public interface IModuleManifest
    {
        /// <summary>
        /// Module name (required)
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Module version (required)
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
        /// Required dependencies (required)
        /// </summary>
        string[] Dependencies { get; }

        /// <summary>
        /// Whether module is enabled (default: true)
        /// </summary>
        bool IsEnabled { get; }
    }

    /// <summary>
    /// Extended manifest for advanced module configuration
    /// SEPARATED: Advanced features in separate interface
    /// </summary>
    public interface IExtendedModuleManifest : IModuleManifest
    {
        /// <summary>
        /// Module tags for categorization
        /// </summary>
        string[] Tags { get; }

        /// <summary>
        /// Optional dependencies that enhance functionality
        /// </summary>
        string[] OptionalDependencies { get; }

        /// <summary>
        /// Minimum framework version required
        /// </summary>
        string MinimumFrameworkVersion { get; }

        /// <summary>
        /// Module loading priority (higher loads first)
        /// </summary>
        int Priority { get; }

        /// <summary>
        /// Whether module is critical for system operation
        /// </summary>
        bool IsCritical { get; }
    }

    /// <summary>
    /// Manifest for modules supporting hot reload
    /// SEPARATED: Hot reload capabilities in dedicated interface
    /// </summary>
    public interface IHotReloadModuleManifest : IModuleManifest
    {
        /// <summary>
        /// Whether module supports hot reload
        /// </summary>
        bool SupportsHotReload { get; }

        /// <summary>
        /// Startup timeout in seconds
        /// </summary>
        int StartupTimeoutSeconds { get; }

        /// <summary>
        /// Module capabilities for discovery
        /// </summary>
        string[] Capabilities { get; }
    }
}