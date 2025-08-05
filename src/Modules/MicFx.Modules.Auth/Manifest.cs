using MicFx.Core.Modularity;
using MicFx.SharedKernel.Modularity;

namespace MicFx.Modules.Auth
{
    /// <summary>
    /// Auth module manifest - Core authentication and authorization system
    /// Critical security module with high priority loading
    /// </summary>
    public class Manifest : ModuleManifestBase
    {
        /// <summary>
        /// Module name
        /// </summary>
        public override string Name => "Auth";

        /// <summary>
        /// Module version
        /// </summary>
        public override string Version => "1.0.0";

        /// <summary>
        /// Module description
        /// </summary>
        public override string Description => "Core authentication and authorization system using ASP.NET Core Identity";

        /// <summary>
        /// Module author
        /// </summary>
        public override string Author => "MicFx Framework Team";

        /// <summary>
        /// Module category - Core security module
        /// </summary>
        public override ModuleCategory Category => ModuleCategory.Core;

        /// <summary>
        /// Tags for discovery
        /// </summary>
        public override string[] CustomTags => new[] { "security", "identity", "authorization" };

        /// <summary>
        /// No external module dependencies
        /// </summary>
        public override string? RequiredModule => null;

        /// <summary>
        /// High priority - security modules load first
        /// </summary>
        public override int Priority => 1;

        /// <summary>
        /// Critical for system security
        /// </summary>
        public override bool IsCritical => true;
    }
}