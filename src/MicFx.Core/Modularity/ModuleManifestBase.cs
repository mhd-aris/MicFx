using MicFx.SharedKernel.Modularity;

namespace MicFx.Core.Modularity
{
    /// <summary>
    /// Simple base class for basic module manifests
    /// SIMPLIFIED: Only essential properties
    /// </summary>
    public abstract class SimpleModuleManifestBase : IModuleManifest
    {
        public abstract string Name { get; }
        public abstract string Version { get; }
        public abstract string Description { get; }
        public abstract string Author { get; }

        public virtual string[] Dependencies => new string[] { };
        public virtual bool IsEnabled => true;
    }

    /// <summary>
    /// Extended base class for modules with advanced features
    /// SEPARATED: Advanced features in separate base class
    /// </summary>
    public abstract class ModuleManifestBase : IExtendedModuleManifest, IHotReloadModuleManifest
    {
        // Core properties (required)
        public abstract string Name { get; }
        public abstract string Version { get; }
        public abstract string Description { get; }
        public abstract string Author { get; }

        public virtual string[] Dependencies => new string[] { };
        public virtual bool IsEnabled => true;

        // Extended properties with sensible defaults
        public virtual string[] Tags => new string[] { };
        public virtual string[] OptionalDependencies => new string[] { };
        public virtual string MinimumFrameworkVersion => "1.0.0";
        public virtual int Priority => 100; // Default priority
        public virtual bool IsCritical => false; // Non-critical by default

        // Hot reload properties with safe defaults
        public virtual bool SupportsHotReload => false; // Disabled by default for safety
        public virtual int StartupTimeoutSeconds => 30; // 30 seconds default timeout
        public virtual string[] Capabilities => new string[] { };
    }
}