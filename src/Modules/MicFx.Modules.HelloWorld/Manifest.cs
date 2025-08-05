using MicFx.Core.Modularity;
using MicFx.SharedKernel.Modularity;

namespace MicFx.Modules.HelloWorld;

/// <summary>
/// HelloWorld module manifest - Demo module for MicFx Framework
/// Demonstrates pragmatic module metadata following clean principles
/// </summary>
public class Manifest : ModuleManifestBase
{
    /// <summary>
    /// Module name following MicFx naming convention
    /// </summary>
    public override string Name => "HelloWorld";

    /// <summary>
    /// Semantic version following SemVer principles
    /// </summary>
    public override string Version => "1.0.0";

    /// <summary>
    /// Simple description focused on purpose
    /// </summary>
    public override string Description => "Demo module showcasing MicFx framework patterns and conventions";

    /// <summary>
    /// Module author information
    /// </summary>
    public override string Author => "MicFx Framework Team";

    /// <summary>
    /// Module category - Demo for PoC purposes
    /// </summary>
    public override ModuleCategory Category => ModuleCategory.Demo;

    /// <summary>
    /// Tags - focused on essential discovery
    /// </summary>
    public override string[] CustomTags => new[] { "poc", "getting-started", "demo" };

    /// <summary>
    /// No dependencies - demonstrates zero-dependency module architecture
    /// </summary>
    public override string? RequiredModule => null;

    /// <summary>
    /// Priority for module initialization - normal priority for demo module
    /// </summary>
    public override int Priority => 100;

    /// <summary>
    /// Not critical for system operation - demo modules are optional
    /// </summary>
    public override bool IsCritical => false;
}