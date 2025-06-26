using System.ComponentModel.DataAnnotations;

namespace MicFx.Modules.Auth.Domain.DTOs
{
    /// <summary>
    /// View model untuk menampilkan user dalam list
    /// </summary>
    public class UserViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public List<string> Roles { get; set; } = new();
    }

    /// <summary>
    /// View model untuk detail user
    /// </summary>
    public class UserDetailsViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public List<string> Roles { get; set; } = new();
        public List<object> Claims { get; set; } = new();
    }

    /// <summary>
    /// View model untuk create user
    /// </summary>
    public class CreateUserViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(100, MinimumLength = 6)]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required]
        [Display(Name = "First Name")]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Last Name")]
        public string LastName { get; set; } = string.Empty;

        [Display(Name = "Is Active")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "Roles")]
        public List<string> SelectedRoles { get; set; } = new();

        public List<RoleViewModel> AvailableRoles { get; set; } = new();
    }

    /// <summary>
    /// View model untuk edit user
    /// </summary>
    public class EditUserViewModel
    {
        public string Id { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [Display(Name = "First Name")]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Last Name")]
        public string LastName { get; set; } = string.Empty;

        [Display(Name = "Is Active")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "Roles")]
        public List<string> SelectedRoles { get; set; } = new();

        public List<RoleViewModel> AvailableRoles { get; set; } = new();
    }

    /// <summary>
    /// View model untuk role
    /// </summary>
    public class RoleViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsSelected { get; set; }
        public bool IsSystemRole { get; set; }
        public int Priority { get; set; }
        public int UserCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<string> Permissions { get; set; } = new();
    }

    /// <summary>
    /// Extended view model untuk role details
    /// </summary>
    public class RoleDetailsViewModel : RoleViewModel
    {
        public DateTime? UpdatedAt { get; set; }
        public bool IsActive { get; set; }
        public List<UserViewModel> Users { get; set; } = new();
        public new List<PermissionViewModel> Permissions { get; set; } = new();
    }

    /// <summary>
    /// View model untuk edit role
    /// </summary>
    public class EditRoleViewModel
    {
        public string Id { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Role Name")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "Description")]
        public string? Description { get; set; }

        [Display(Name = "Priority")]
        [Range(0, 100)]
        public int Priority { get; set; }

        [Display(Name = "Is Active")]
        public bool IsActive { get; set; } = true;

        public bool IsSystemRole { get; set; }
        public List<string> SelectedPermissions { get; set; } = new();
        public List<PermissionViewModel> AvailablePermissions { get; set; } = new();
    }

    /// <summary>
    /// View model untuk permission
    /// </summary>
    public class PermissionViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Module { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public bool IsSelected { get; set; }
        public bool IsSystemPermission { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<string> Roles { get; set; } = new();
    }

    /// <summary>
    /// View model untuk dashboard admin
    /// </summary>
    public class AdminDashboardViewModel
    {
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public int TotalRoles { get; set; }
        public int TotalPermissions { get; set; }
        public List<UserViewModel> RecentUsers { get; set; } = new();
        public List<RoleViewModel> TopRoles { get; set; } = new();
    }

    /// <summary>
    /// View model untuk security settings
    /// </summary>
    public class SecuritySettingsViewModel
    {
        [Display(Name = "Minimum Password Length")]
        [Range(6, 20)]
        public int MinPasswordLength { get; set; } = 8;

        [Display(Name = "Password Expiry (days)")]
        [Range(0, 365)]
        public int PasswordExpiryDays { get; set; } = 90;

        [Display(Name = "Require Uppercase")]
        public bool RequireUppercase { get; set; } = true;

        [Display(Name = "Require Lowercase")]
        public bool RequireLowercase { get; set; } = true;

        [Display(Name = "Require Numbers")]
        public bool RequireNumbers { get; set; } = true;

        [Display(Name = "Require Special Characters")]
        public bool RequireSpecialChars { get; set; } = true;

        [Display(Name = "Maximum Login Attempts")]
        [Range(1, 10)]
        public int MaxLoginAttempts { get; set; } = 5;

        [Display(Name = "Lockout Duration (minutes)")]
        [Range(5, 1440)]
        public int LockoutDurationMinutes { get; set; } = 30;

        [Display(Name = "Session Timeout (minutes)")]
        [Range(15, 1440)]
        public int SessionTimeoutMinutes { get; set; } = 120;

        [Display(Name = "Max Concurrent Sessions")]
        [Range(1, 10)]
        public int MaxConcurrentSessions { get; set; } = 3;

        [Display(Name = "Require Two-Factor Authentication")]
        public bool RequireTwoFactor { get; set; } = false;

        [Display(Name = "Allow Remember Device")]
        public bool AllowRememberDevice { get; set; } = true;
    }
} 