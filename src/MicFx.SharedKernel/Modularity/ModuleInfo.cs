namespace MicFx.SharedKernel.Modularity
{
    /// <summary>
    /// Enhanced module information for inter-module communication
    /// </summary>
    public class ModuleInfo
    {
        public string Name { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public List<string> Dependencies { get; set; } = new();
        public bool IsEnabled { get; set; } = true;
        public bool IsAvailable { get; set; } = false;
        public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;
        public Dictionary<string, object> Metadata { get; set; } = new();
    }
}