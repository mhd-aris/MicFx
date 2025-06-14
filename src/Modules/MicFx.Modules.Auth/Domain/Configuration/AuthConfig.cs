namespace MicFx.Modules.Auth.Domain.Configuration
{
    /// <summary>
    /// Konfigurasi untuk Auth module
    /// Connection string dikelola di level host (shared database)
    /// </summary>
    public class AuthConfig
    {
        /// <summary>
        /// Password requirements
        /// </summary>
        public PasswordConfig Password { get; set; } = new();

        /// <summary>
        /// Lockout settings
        /// </summary>
        public LockoutConfig Lockout { get; set; } = new();

        /// <summary>
        /// Cookie settings
        /// </summary>
        public CookieConfig Cookie { get; set; } = new();

        /// <summary>
        /// Default roles yang akan dibuat saat aplikasi pertama kali dijalankan
        /// </summary>
        public List<string> DefaultRoles { get; set; } = new()
        {
            "SuperAdmin",  // Highest privilege - can manage everything including other admins
            "Admin",       // Can manage users and basic system settings
            "ModuleOwner", // Can manage specific modules
            "UserManager", // Can manage regular users only
            "Editor",      // Can edit content
            "User",        // Regular user
            "Guest"        // Limited access
        };

        /// <summary>
        /// Roles yang memiliki akses ke admin area
        /// </summary>
        public List<string> AdminRoles { get; set; } = new()
        {
            "SuperAdmin",
            "Admin"
        };

        /// <summary>
        /// Roles yang dapat mengelola user lain
        /// </summary>
        public List<string> UserManagementRoles { get; set; } = new()
        {
            "SuperAdmin",
            "Admin",
            "UserManager"
        };

        /// <summary>
        /// Role hierarchy untuk authorization
        /// Higher number = higher privilege
        /// </summary>
        public Dictionary<string, int> RoleHierarchy { get; set; } = new()
        {
            { "Guest", 0 },
            { "User", 10 },
            { "Editor", 20 },
            { "UserManager", 30 },
            { "ModuleOwner", 40 },
            { "Admin", 50 },
            { "SuperAdmin", 100 }
        };

        /// <summary>
        /// Admin user default
        /// </summary>
        public DefaultAdminConfig DefaultAdmin { get; set; } = new();

        /// <summary>
        /// Route prefix untuk auth endpoints
        /// </summary>
        public string RoutePrefix { get; set; } = "auth";
    }

    public class PasswordConfig
    {
        public bool RequireDigit { get; set; } = true;
        public int RequiredLength { get; set; } = 6;
        public bool RequireNonAlphanumeric { get; set; } = false;
        public bool RequireUppercase { get; set; } = true;
        public bool RequireLowercase { get; set; } = true;
        public int RequiredUniqueChars { get; set; } = 1;
    }

    public class LockoutConfig
    {
        public TimeSpan DefaultLockoutTimeSpan { get; set; } = TimeSpan.FromMinutes(5);
        public int MaxFailedAccessAttempts { get; set; } = 5;
        public bool AllowedForNewUsers { get; set; } = true;
    }

    public class CookieConfig
    {
        public string LoginPath { get; set; } = "/auth/login";
        public string LogoutPath { get; set; } = "/auth/logout";
        public string AccessDeniedPath { get; set; } = "/auth/access-denied";
        public TimeSpan ExpireTimeSpan { get; set; } = TimeSpan.FromHours(2);
        public bool SlidingExpiration { get; set; } = true;
        public string CookieName { get; set; } = "MicFx.Auth";
    }

    public class DefaultAdminConfig
    {
        public string Email { get; set; } = "admin@micfx.dev";
        public string Password { get; set; } = "Admin123!";
        public string FirstName { get; set; } = "System";
        public string LastName { get; set; } = "Admin";
        public string Department { get; set; } = "IT";
        public string JobTitle { get; set; } = "System Administrator";
    }
}