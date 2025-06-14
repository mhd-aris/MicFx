using System.ComponentModel.DataAnnotations;

namespace MicFx.Modules.Auth.Domain.Entities;

/// <summary>
/// Junction entity untuk Role-Permission many-to-many relationship
/// Handles granular permission assignment to roles
/// </summary>
public class RolePermission
{
    /// <summary>
    /// Unique identifier for this role-permission assignment
    /// </summary>
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// Foreign key to Role
    /// </summary>
    [Required]
    public string RoleId { get; set; } = string.Empty;

    /// <summary>
    /// Foreign key to Permission
    /// </summary>
    [Required]
    public int PermissionId { get; set; }

    /// <summary>
    /// When this permission was granted to the role
    /// </summary>
    public DateTime GrantedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Who granted this permission to the role
    /// </summary>
    [StringLength(100)]
    public string GrantedBy { get; set; } = "System";

    /// <summary>
    /// Whether this permission grant is active
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Optional reason for granting this permission
    /// </summary>
    [StringLength(500)]
    public string? Reason { get; set; }

    /// <summary>
    /// Navigation property to Role
    /// </summary>
    public virtual Role Role { get; set; } = null!;

    /// <summary>
    /// Navigation property to Permission
    /// </summary>
    public virtual Permission Permission { get; set; } = null!;
} 