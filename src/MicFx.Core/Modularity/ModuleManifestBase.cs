using MicFx.SharedKernel.Modularity;

namespace MicFx.Core.Modularity
{
    /// <summary>
    /// Base class for module manifests with sensible defaults
    /// </summary>
    public abstract class ModuleManifestBase : IModuleManifest
    {
        public abstract string Name { get; }
        public abstract string Version { get; }
        public abstract string Description { get; }
        public abstract string Author { get; }

        public virtual string[] Dependencies => new string[] { };
        public virtual string[] Tags => new string[] { };
        public virtual bool IsEnabled => true;
        public virtual string EntryPoint => $"{Name}Startup";
        public virtual string[] Controllers => new string[] { };
        public virtual string[] Views => new string[] { };
        public virtual string[] Routes => new string[] { };

        // Enhanced dependency management properties with sensible defaults
        public virtual string[] OptionalDependencies => new string[] { };
        public virtual string MinimumFrameworkVersion => "1.0.0";
        public virtual string MaximumFrameworkVersion => "99.0.0";
        public virtual int Priority => 100; // Default priority, lower numbers = higher priority
        public virtual bool IsCritical => false; // Non-critical by default

        // Lifecycle management properties with sensible defaults
        public virtual bool SupportsHotReload => false; // Disabled by default for safety
        public virtual int StartupTimeoutSeconds => 30; // 30 seconds default timeout
        public virtual string[] Capabilities => new string[] { };
        public virtual string[] ConflictsWith => new string[] { };
    }
}