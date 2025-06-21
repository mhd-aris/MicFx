using MicFx.SharedKernel.Modularity;

namespace MicFx.Core.Modularity
{
    /// <summary>
    /// Simplified base class for module manifests
    /// Removed complex features for better maintainability
    /// </summary>
    public abstract class ModuleManifestBase : IModuleManifest
    {
        // Core properties (required)
        public abstract string Name { get; }
        public abstract string Version { get; }
        public abstract string Description { get; }
        public abstract string Author { get; }

        // Essential properties with sensible defaults
        public virtual string[] Dependencies => Array.Empty<string>();
        public virtual bool IsEnabled => true; // Enabled by default
        public virtual int Priority => 100; // Default priority

        // Additional metadata (optional)
        public virtual string[] Tags => Array.Empty<string>();
        public virtual string MinimumFrameworkVersion => "1.0.0";
        public virtual bool IsCritical => false; // Non-critical by default
    }
}