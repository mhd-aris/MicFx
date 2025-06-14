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
        public string? Department { get; set; }
        public string? JobTitle { get; set; }
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
        public string? Department { get; set; }
        public string? JobTitle { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public List<string> Roles { get; set; } = new();
        public List<object> Claims { get; set; } = new();
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

        [Display(Name = "Department")]
        public string? Department { get; set; }

        [Display(Name = "Job Title")]
        public string? JobTitle { get; set; }

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
    }

    /// <summary>
    /// Extended view model untuk role details
    /// </summary>
    public class RoleDetailsViewModel : RoleViewModel
    {
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool IsActive { get; set; }
        public List<UserViewModel> Users { get; set; } = new();
        public List<PermissionViewModel> Permissions { get; set; } = new();
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
} 