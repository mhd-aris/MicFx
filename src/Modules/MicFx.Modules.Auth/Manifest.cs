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

        // Essential properties
        public override string[]? Dependencies => Array.Empty<string>(); // Core framework dependencies handled automatically
        public override string MinimumFrameworkVersion => "1.0.0";
        public override int Priority => 300; // High priority as security module (higher number = loads first)
        public override bool IsCritical => true; // Critical for system security
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