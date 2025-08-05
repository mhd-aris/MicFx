using MicFx.SharedKernel.Modularity;

namespace MicFx.Core.Modularity
{
    /// <summary>
    /// Base class for module manifests
    /// </summary>
    public abstract class ModuleManifestBase : IModuleManifest
    {
        // Required properties (must be implemented by modules)
        public abstract string Name { get; }
        public abstract string Version { get; }
        
        // Optional properties with default values
        public virtual string Description => $"{Name} module for MicFx";
        public virtual string Author => "MicFx Developer";
        public virtual ModuleCategory Category => ModuleCategory.Business;
        public virtual string[] CustomTags => Array.Empty<string>();
        
        // Single dependency only
        public virtual string? RequiredModule => null;
        
        // Framework properties
        public virtual bool IsCritical => false;
        public virtual int Priority => 100;
    }
}