using MicFx.Core.Modularity;

namespace MicFx.Modules.HelloWorld;

/// <summary>
/// HelloWorld module manifest - Primary Proof of Concept (PoC) for MicFx Framework
/// Demonstrates complete module metadata and framework capabilities
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
    /// Comprehensive description of the module's purpose as a PoC
    /// </summary>
    public override string Description => 
        "Primary Proof of Concept (PoC) module demonstrating MicFx framework capabilities including " +
        "clean architecture, structured logging, exception handling, auto-discovery, and zero-configuration patterns. " +
        "Serves as the foundational example for building modular applications with MicFx.";

    /// <summary>
    /// Module author information
    /// </summary>
    public override string Author => "MicFx Framework Team";

    /// <summary>
    /// Descriptive tags for module categorization and discovery
    /// </summary>
    public override string[] Tags => new[]
    {
        "poc",                          // Proof of Concept
        "demo",                         // Demonstration module
        "framework-showcase",           // Framework capabilities showcase
        "clean-architecture",           // Clean architecture implementation
        "structured-logging",           // Structured logging example
        "exception-handling",           // Exception handling patterns
        "auto-discovery",              // Auto-discovery demonstration
        "zero-configuration",          // Zero config principle
        "solid-principles",            // SOLID principles implementation
        "api-first",                   // API-first design
        "enterprise-ready",            // Enterprise-grade patterns
        "getting-started",             // Getting started example
        "best-practices"               // Best practices demonstration
    };

    /// <summary>
    /// Module dependencies - HelloWorld as PoC has no inter-module dependencies
    /// Demonstrates zero-dependency module architecture
    /// </summary>
    public override string[] Dependencies => new string[0];

    /// <summary>
    /// Minimum framework version required
    /// </summary>
    public override string MinimumFrameworkVersion => "1.0.0";

    /// <summary>
    /// Priority for module initialization (higher values load first)
    /// HelloWorld as PoC loads with normal priority
    /// </summary>
    public override int Priority => 100;

    /// <summary>
    /// Whether this module is critical for system operation
    /// PoC modules are typically not critical
    /// </summary>
    public override bool IsCritical => false;

    /// <summary>
    /// Module capabilities for runtime discovery
    /// </summary>
    public override string[] Capabilities => new[]
    {
        "api-endpoints",
        "structured-logging-demo",
        "exception-handling-demo",
        "domain-modeling",
        "service-layer-pattern",
        "clean-architecture-demo",
        "framework-integration-validation"
    };

    /// <summary>
    /// Optional dependencies that enhance functionality if available
    /// </summary>
    public override string[] OptionalDependencies => new[]
    {
        "MicFx.Modules.Auth"           // Authentication for enhanced demos
    };

    /// <summary>
    /// Whether this module supports hot reload
    /// Enabled for PoC to demonstrate framework capabilities
    /// </summary>
    public override bool SupportsHotReload => true;

    /// <summary>
    /// Startup timeout for module initialization
    /// </summary>
    public override int StartupTimeoutSeconds => 30;
}