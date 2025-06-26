using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace MicFx.Modules.Auth.Domain.Entities
{
    /// <summary>
    /// User entity untuk MicFx authentication system
    /// Extends IdentityUser untuk leverage ASP.NET Core Identity features
    /// </summary>
    public class User : IdentityUser
    {
        /// <summary>
        /// First name of the user
        /// </summary>
        [Required]
        [StringLength(50)]
        public string FirstName { get; set; } = string.Empty;

        /// <summary>
        /// Last name of the user
        /// </summary>
        [Required]
        [StringLength(50)]
        public string LastName { get; set; } = string.Empty;

        /// <summary>
        /// Full name computed property
        /// </summary>
        public string FullName => $"{FirstName} {LastName}".Trim();

        /// <summary>
        /// When the user account was created
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Last login timestamp
        /// </summary>
        public DateTime? LastLoginAt { get; set; }

        /// <summary>
        /// Whether the user account is active
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Profile picture URL or path
        /// </summary>
        [StringLength(500)]
        public string? ProfilePicture { get; set; }

        /// <summary>
        /// Who created this user account
        /// </summary>
        [StringLength(100)]
        public string CreatedBy { get; set; } = "System";

        /// <summary>
        /// When the user was last updated
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// Who last updated this user
        /// </summary>
        [StringLength(100)]
        public string? UpdatedBy { get; set; }

        /// <summary>
        /// Navigation property for user roles
        /// </summary>
        public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    }
}