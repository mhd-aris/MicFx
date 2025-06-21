using MicFx.Core.Modularity;
using MicFx.SharedKernel.Modularity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MicFx.Tests.Core._TestUtilities;

/// <summary>
/// Test manifest implementation for module testing
/// </summary>
public class TestModuleManifest : IModuleManifest
{
    public string Name { get; set; } = string.Empty;
    public string Version { get; set; } = "1.0.0";
    public string Description { get; set; } = "Test Module";
    public string Author { get; set; } = "Test";
    public string[]? Dependencies { get; set; } = Array.Empty<string>();
    public bool IsEnabled { get; set; } = true;
    public int Priority { get; set; } = 100;
}

/// <summary>
/// Concrete test implementation for module testing
/// SIMPLIFIED: Focused only on basic module functionality without complex scenarios
/// </summary>
public class TestModuleStartup : ModuleStartupBase
{
    private readonly TestModuleManifest _manifest;

    public TestModuleStartup(
        string moduleName,
        string[]? dependencies = null,
        int priority = 100,
        string version = "1.0.0",
        ILogger<ModuleStartupBase>? logger = null) : base(logger)
    {
        _manifest = new TestModuleManifest
        {
            Name = moduleName,
            Dependencies = dependencies ?? Array.Empty<string>(),
            Priority = priority,
            Version = version
        };
    }

    public override IModuleManifest Manifest => _manifest;

    public override async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        // Simple success implementation
        await Task.CompletedTask;
    }

    public override async Task ShutdownAsync(CancellationToken cancellationToken = default)
    {
        // Simple success implementation
        await Task.CompletedTask;
    }

    public override void ConfigureServices(IServiceCollection services)
    {
        // Default empty implementation for tests
    }
}

/// <summary>
/// Factory untuk membuat test module implementations
/// SIMPLIFIED: Only basic module creation, remove unused factory methods
/// </summary>
public static class TestModuleFactory
{
    /// <summary>
    /// Creates a basic test module
    /// </summary>
    public static TestModuleStartup CreateBasicModule(
        string name,
        string[]? dependencies = null,
        int priority = 100,
        string version = "1.0.0")
    {
        return new TestModuleStartup(name, dependencies, priority, version);
    }
} 