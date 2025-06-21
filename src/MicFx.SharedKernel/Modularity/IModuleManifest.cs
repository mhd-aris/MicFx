namespace MicFx.SharedKernel.Modularity
{
    /// <summary>
    /// Essential module manifest with core properties only
    /// Simplified from over-engineered version with multiple interfaces
    /// </summary>
    public interface IModuleManifest
    {
        /// <summary>
        /// Module name (required)
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Module version (required)
        /// </summary>
        string Version { get; }

        /// <summary>
        /// Module description (optional)
        /// </summary>
        string Description { get; }

        /// <summary>
        /// Module author (optional)
        /// </summary>
        string Author { get; }

        /// <summary>
        /// Required dependencies (required)
        /// </summary>
        string[]? Dependencies { get; }

        /// <summary>
        /// Whether module is enabled (default: true)
        /// </summary>
        bool IsEnabled { get; }

        /// <summary>
        /// Module priority for startup ordering (higher loads first)
        /// </summary>
        int Priority { get; }
    }
}