using MicFx.Core.Modularity;
using MicFx.SharedKernel.Modularity;
using MicFx.SharedKernel.Interfaces;
using MicFx.Modules.HelloWorld.Services;
using MicFx.Modules.HelloWorld.Areas.Admin;
using Microsoft.Extensions.DependencyInjection;

namespace MicFx.Modules.HelloWorld;

/// <summary>
/// HelloWorld module startup configuration
/// Demonstrates proper module initialization in MicFx framework
/// Follows MicFx conventions for clean module registration
/// </summary>
public class HelloWorldStartup : ModuleStartupBase
{
    /// <summary>
    /// Module manifest providing metadata and configuration
    /// </summary>
    public override IModuleManifest Manifest { get; } = new Manifest();

    /// <summary>
    /// Configure module-specific services
    /// Demonstrates proper service registration pattern in MicFx
    /// </summary>
    /// <param name="services">Service collection for dependency injection</param>
    protected override void ConfigureModuleServices(IServiceCollection services)
    {
        // Register core HelloWorld service as primary business logic layer
        services.AddScoped<IHelloWorldService, HelloWorldService>();

        // Register admin navigation contributor for this module
        // NOTE: Disabled manual registration - using auto-discovery instead
        // services.AddTransient<IAdminNavContributor, HelloWorldAdminNavContributor>();

        // Note: Controllers are automatically discovered by MicFx framework
        // No manual controller registration needed - this demonstrates
        // the "Zero Configuration" principle of MicFx
    }
}