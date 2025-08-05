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
    public ModuleCategory Category { get; set; } = ModuleCategory.Demo;
    public string[] CustomTags { get; set; } = Array.Empty<string>();
    public string? RequiredModule { get; set; }
    public bool IsEnabled { get; set; } = true;
    public bool IsCritical { get; set; } = false;
    public int Priority { get; set; } = 100;
}

/// <summary>
/// Concrete test implementation for module testing
/// </summary>
public class TestModuleStartup : ModuleStartupBase
{
    private readonly TestModuleManifest _manifest;

    public TestModuleStartup(
        string moduleName,
        string? requiredModule = null,
        int priority = 100,
        string version = "1.0.0",
        ILogger<ModuleStartupBase>? logger = null) : base(logger)
    {
        _manifest = new TestModuleManifest
        {
            Name = moduleName,
            RequiredModule = requiredModule,
            Priority = priority,
            Version = version
        };
    }

    public override IModuleManifest Manifest => _manifest;

    public override async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        // Implementation for tests
        await Task.CompletedTask;
    }

    public override async Task ShutdownAsync(CancellationToken cancellationToken = default)
    {
        // Implementation for tests
        await Task.CompletedTask;
    }

    public override void ConfigureServices(IServiceCollection services)
    {
        // Default empty implementation for tests
    }
}

/// <summary>
/// Factory for creating test module implementations
/// </summary>
public static class TestModuleFactory
{
    /// <summary>
    /// Creates a basic test module
    /// </summary>
    public static TestModuleStartup CreateBasicModule(string name, int priority = 100)
    {
        return new TestModuleStartup(name, priority: priority);
    }

    /// <summary>
    /// Creates a module with dependency
    /// </summary>
    public static TestModuleStartup CreateModuleWithDependency(string name, string requiredModule, int priority = 100)
    {
        return new TestModuleStartup(name, requiredModule, priority);
    }

    /// <summary>
    /// Creates multiple test modules for dependency resolution testing
    /// </summary>
    public static List<TestModuleStartup> CreateModuleChain()
    {
        return new List<TestModuleStartup>
        {
            new TestModuleStartup("CoreModule", priority: 1),
            new TestModuleStartup("BusinessModule", "CoreModule", priority: 2),
            new TestModuleStartup("UIModule", "BusinessModule", priority: 3)
        };
    }
}