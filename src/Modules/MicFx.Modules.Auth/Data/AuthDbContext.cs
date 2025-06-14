using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MicFx.Modules.Auth.Domain.Entities;

namespace MicFx.Modules.Auth.Data;

/// <summary>
/// Database context for Auth module with full RBAC support
/// Uses ASP.NET Core Identity with custom User, Role, and Permission entities
/// </summary>
public class AuthDbContext : IdentityDbContext<User, Role, string, 
    Microsoft.AspNetCore.Identity.IdentityUserClaim<string>,
    UserRole,
    Microsoft.AspNetCore.Identity.IdentityUserLogin<string>,
    Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>,
    Microsoft.AspNetCore.Identity.IdentityUserToken<string>>
{
    public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options)
    {
    }

    /// <summary>
    /// Permissions DbSet
    /// </summary>
    public DbSet<Permission> Permissions { get; set; }

    /// <summary>
    /// Role-Permission junction table
    /// </summary>
    public DbSet<RolePermission> RolePermissions { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Customize table names with Auth module prefix
        builder.Entity<User>().ToTable("Auth_Users");
        builder.Entity<Role>().ToTable("Auth_Roles");
        builder.Entity<UserRole>().ToTable("Auth_UserRoles");
        builder.Entity<Permission>().ToTable("Auth_Permissions");
        builder.Entity<RolePermission>().ToTable("Auth_RolePermissions");
        
        // Customize Identity table names with prefix
        builder.Entity<Microsoft.AspNetCore.Identity.IdentityUserClaim<string>>().ToTable("Auth_UserClaims");
        builder.Entity<Microsoft.AspNetCore.Identity.IdentityUserLogin<string>>().ToTable("Auth_UserLogins");
        builder.Entity<Microsoft.AspNetCore.Identity.IdentityUserToken<string>>().ToTable("Auth_UserTokens");
        builder.Entity<Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>>().ToTable("Auth_RoleClaims");

        // Configure User entity
        builder.Entity<User>(entity =>
        {
            entity.Property(e => e.FirstName).HasMaxLength(50).IsRequired();
            entity.Property(e => e.LastName).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Department).HasMaxLength(100);
            entity.Property(e => e.JobTitle).HasMaxLength(100);
            entity.Property(e => e.ProfilePicture).HasMaxLength(500);
            entity.Property(e => e.CreatedBy).HasMaxLength(100);
            entity.Property(e => e.UpdatedBy).HasMaxLength(100);

            // Indexes for performance
            entity.HasIndex(e => e.Email).IsUnique();
            entity.HasIndex(e => e.IsActive);
            entity.HasIndex(e => e.CreatedAt);
            entity.HasIndex(e => e.LastLoginAt);
        });

        // Configure Role entity
        builder.Entity<Role>(entity =>
        {
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.CreatedBy).HasMaxLength(100);
            entity.Property(e => e.UpdatedBy).HasMaxLength(100);

            // Indexes for performance
            entity.HasIndex(e => e.IsSystemRole);
            entity.HasIndex(e => e.IsActive);
            entity.HasIndex(e => e.Priority);
        });

        // Configure Permission entity
        builder.Entity<Permission>(entity =>
        {
            entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
            entity.Property(e => e.DisplayName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Module).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Category).HasMaxLength(50);
            entity.Property(e => e.CreatedBy).HasMaxLength(100);
            entity.Property(e => e.UpdatedBy).HasMaxLength(100);

            // Unique constraint on permission name
            entity.HasIndex(e => e.Name).IsUnique();
            
            // Indexes for performance
            entity.HasIndex(e => e.Module);
            entity.HasIndex(e => e.Category);
            entity.HasIndex(e => e.IsSystemPermission);
            entity.HasIndex(e => e.IsActive);
        });

        // Configure UserRole junction entity
        builder.Entity<UserRole>(entity =>
        {
            entity.Property(e => e.AssignedBy).HasMaxLength(100);
            entity.Property(e => e.Reason).HasMaxLength(500);

            // Configure relationships
            entity.HasOne(ur => ur.User)
                .WithMany(u => u.UserRoles)
                .HasForeignKey(ur => ur.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(ur => ur.Role)
                .WithMany(r => r.UserRoles)
                .HasForeignKey(ur => ur.RoleId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes for performance
            entity.HasIndex(e => e.AssignedAt);
            entity.HasIndex(e => e.ExpiresAt);
            entity.HasIndex(e => e.IsActive);
        });

        // Configure RolePermission junction entity
        builder.Entity<RolePermission>(entity =>
        {
            entity.Property(e => e.GrantedBy).HasMaxLength(100);
            entity.Property(e => e.Reason).HasMaxLength(500);

            // Configure relationships
            entity.HasOne(rp => rp.Role)
                .WithMany(r => r.RolePermissions)
                .HasForeignKey(rp => rp.RoleId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(rp => rp.Permission)
                .WithMany(p => p.RolePermissions)
                .HasForeignKey(rp => rp.PermissionId)
                .OnDelete(DeleteBehavior.Cascade);

            // Unique constraint to prevent duplicate role-permission assignments
            entity.HasIndex(e => new { e.RoleId, e.PermissionId }).IsUnique();

            // Indexes for performance
            entity.HasIndex(e => e.GrantedAt);
            entity.HasIndex(e => e.IsActive);
        });
    }
}