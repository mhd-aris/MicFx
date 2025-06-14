using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace MicFx.Modules.Auth.Domain.Entities;

/// <summary>
/// Junction entity untuk User-Role many-to-many relationship
/// Extends IdentityUserRole untuk additional metadata
/// </summary>
public class UserRole : IdentityUserRole<string>
{
    /// <summary>
    /// When this role was assigned to the user
    /// </summary>
    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Who assigned this role to the user
    /// </summary>
    [StringLength(100)]
    public string AssignedBy { get; set; } = "System";

    /// <summary>
    /// Optional expiration date for the role assignment
    /// </summary>
    public DateTime? ExpiresAt { get; set; }

    /// <summary>
    /// Whether this role assignment is active
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Optional reason for the role assignment
    /// </summary>
    [StringLength(500)]
    public string? Reason { get; set; }

    /// <summary>
    /// Navigation property to User
    /// </summary>
    public virtual User User { get; set; } = null!;

    /// <summary>
    /// Navigation property to Role
    /// </summary>
    public virtual Role Role { get; set; } = null!;
} 