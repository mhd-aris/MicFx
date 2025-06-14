using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace MicFx.Modules.Auth.Domain.Entities
{
    /// <summary>
    /// Custom Role entity untuk MicFx Auth
    /// Extends IdentityRole untuk additional properties dan RBAC support
    /// </summary>
    public class Role : IdentityRole
    {
        /// <summary>
        /// Description of the role
        /// </summary>
        [StringLength(500)]
        public string? Description { get; set; }

        /// <summary>
        /// Indicates if this is a system role (cannot be deleted)
        /// </summary>
        public bool IsSystemRole { get; set; } = false;

        /// <summary>
        /// Whether this role is active
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Priority level for role hierarchy (lower number = higher priority)
        /// SuperAdmin = 1, Admin = 10, ModuleOwner = 20, etc.
        /// </summary>
        public int Priority { get; set; } = 100;

        /// <summary>
        /// When the role was created
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Who created this role
        /// </summary>
        [StringLength(100)]
        public string CreatedBy { get; set; } = "System";

        /// <summary>
        /// When the role was last updated
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// Who last updated this role
        /// </summary>
        [StringLength(100)]
        public string? UpdatedBy { get; set; }

        /// <summary>
        /// Navigation property for user roles
        /// </summary>
        public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();

        /// <summary>
        /// Navigation property for role permissions
        /// </summary>
        public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();

        public Role() : base()
        {
        }

        public Role(string roleName) : base(roleName)
        {
            Name = roleName;
            NormalizedName = roleName.ToUpper();
        }

        public Role(string roleName, string description, bool isSystemRole = false, int priority = 100) : this(roleName)
        {
            Description = description;
            IsSystemRole = isSystemRole;
            Priority = priority;
        }
    }
}