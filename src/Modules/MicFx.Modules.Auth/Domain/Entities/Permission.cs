using System.ComponentModel.DataAnnotations;

namespace MicFx.Modules.Auth.Domain.Entities;

/// <summary>
/// Permission entity untuk granular access control
/// Represents specific actions that can be performed in the system
/// </summary>
public class Permission
{
    /// <summary>
    /// Unique identifier for the permission
    /// </summary>
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// Unique name of the permission (e.g., "users.create", "admin.access")
    /// </summary>
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Human-readable display name
    /// </summary>
    [Required]
    [StringLength(100)]
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Description of what this permission allows
    /// </summary>
    [StringLength(500)]
    public string? Description { get; set; }

    /// <summary>
    /// Module or area this permission belongs to (e.g., "Auth", "HelloWorld", "Admin")
    /// </summary>
    [Required]
    [StringLength(50)]
    public string Module { get; set; } = string.Empty;

    /// <summary>
    /// Category within the module (e.g., "Users", "Roles", "Settings")
    /// </summary>
    [StringLength(50)]
    public string? Category { get; set; }

    /// <summary>
    /// Whether this is a system permission (cannot be deleted)
    /// </summary>
    public bool IsSystemPermission { get; set; } = false;

    /// <summary>
    /// Whether this permission is active
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// When the permission was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Who created this permission
    /// </summary>
    [StringLength(100)]
    public string CreatedBy { get; set; } = "System";

    /// <summary>
    /// When the permission was last updated
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Who last updated this permission
    /// </summary>
    [StringLength(100)]
    public string? UpdatedBy { get; set; }

    /// <summary>
    /// Navigation property for role permissions
    /// </summary>
    public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
} 