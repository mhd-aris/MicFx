namespace MicFx.SharedKernel.Modularity
{
    public interface IModuleManifest
    {
        string Name { get; }
        string Version { get; }
        string Description { get; }
        string Author { get; }
        string[] Dependencies { get; }
        string[] Tags { get; }
        bool IsEnabled { get; }
        string EntryPoint { get; }
        string[] Controllers { get; }
        string[] Views { get; }
        string[] Routes { get; }

        // Enhanced dependency management properties
        string[] OptionalDependencies { get; }
        string MinimumFrameworkVersion { get; }
        string MaximumFrameworkVersion { get; }
        int Priority { get; }
        bool IsCritical { get; }

        // Lifecycle management properties
        bool SupportsHotReload { get; }
        int StartupTimeoutSeconds { get; }
        string[] Capabilities { get; }
        string[] ConflictsWith { get; }
    }
}