using MicFx.Core.Modularity;

namespace MicFx.Modules.Auth
{
    /// <summary>
    /// Manifest untuk Auth module
    /// </summary>
    public class Manifest : ModuleManifestBase
    {
        public override string Name => "Auth";
        public override string Version => "1.0.0";
        public override string Description => "Core authentication module untuk MicFx framework dengan ASP.NET Core Identity";
        public override string Author => "MicFx Team";

        // Enhanced dependency management properties
        public override string[] Dependencies => new string[] { }; // Core framework dependencies handled automatically
        public override string[] OptionalDependencies => new string[] { }; // No optional dependencies
        public override string MinimumFrameworkVersion => "1.0.0";
        public override int Priority => 10; // High priority as security module
        public override bool IsCritical => true; // Critical for system security

        // Lifecycle management properties
        public override bool SupportsHotReload => false; // Auth module should not be hot-reloaded for security
        public override int StartupTimeoutSeconds => 60; // Allow more time for database setup
        public override string[] Capabilities => new string[] {
            "authentication",
            "authorization",
            "user-management",
            "identity",
            "security"
        };
        public override string[] Tags => new string[] { "auth", "security", "identity", "core" };

        /// <summary>
        /// Database tables yang akan dibuat oleh module ini
        /// </summary>
        public string[] DatabaseTables => new string[]
        {
            "Users",
            "Roles",
            "AspNetUserRoles",
            "AspNetUserClaims",
            "AspNetUserLogins",
            "AspNetUserTokens",
            "AspNetRoleClaims"
        };

        /// <summary>
        /// API endpoints yang disediakan module ini
        /// </summary>
        public string[] ApiEndpoints => new string[]
        {
            "/auth/login",
            "/auth/register",
            "/auth/logout", 
            "/auth/status",
            "/auth/profile",
            "/auth/change-password",
            "/admin/auth",
            "/admin/auth/users",
            "/admin/auth/roles",
            "/admin/auth/quick-stats",
            "/admin/auth/user-activity",
            "/admin/auth/role-distribution"
        };

        /// <summary>
        /// Services yang di-register oleh module ini
        /// </summary>
        public string[] Services => new string[]
        {
            "IAuthService",
            "AuthService",
            "UserManager<User>",
            "RoleManager<Role>",
            "SignInManager<User>",
            "AuthDbContext"
        };
    }
}