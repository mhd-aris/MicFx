using MicFx.SharedKernel.Modularity;

namespace MicFx.Modules.HelloWorld.ViewModels;

/// <summary>
/// ViewModel untuk halaman About HelloWorld Module
/// </summary>
public class AboutViewModel
{
    public string Title { get; set; } = string.Empty;
    public IModuleManifest Manifest { get; set; } = null!;
    public Dictionary<string, object> Health { get; set; } = new();
} 