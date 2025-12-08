using System.Linq;
using AuthService.Application.Common.Interfaces;
using AuthService.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace AuthService.Infrastructure.Persistence;

/// <summary>
/// Unified DbContext for PostgreSQL database - handles all database operations
/// </summary>
public class AppDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>, IAppDbContext
{
    private readonly IHttpContextAccessor? _httpContextAccessor;

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public AppDbContext(DbContextOptions<AppDbContext> options, IHttpContextAccessor httpContextAccessor) : base(options)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    /// <summary>
    /// Gets the current user's identifier from HttpContext claims
    /// </summary>
    private string GetCurrentUser()
    {
        var user = _httpContextAccessor?.HttpContext?.User;
        if (user?.Identity?.IsAuthenticated == true)
        {
            // Try to get email first, then name, then user id
            return user.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value
                ?? user.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value
                ?? user.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                ?? "System";
        }
        return "System";
    }

    // Entity sets
    public DbSet<UserAddress> UserAddresses => Set<UserAddress>();
    public DbSet<UserRefreshToken> RefreshTokens => Set<UserRefreshToken>();
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<Feature> Features => Set<Feature>();
    public DbSet<Page> Pages => Set<Page>();
    public DbSet<RolePermissionMapping> RolePermissionMappings => Set<RolePermissionMapping>();
    public DbSet<PagePermissionMapping> PagePermissionMappings => Set<PagePermissionMapping>();
    public DbSet<PageFeatureMapping> PageFeatureMappings => Set<PageFeatureMapping>();
    public DbSet<RoleHierarchy> RoleHierarchies => Set<RoleHierarchy>();
    public DbSet<UserRoleMapping> UserRoleMappings => Set<UserRoleMapping>();
    public DbSet<RoleDepartmentMapping> RoleDepartmentMappings => Set<RoleDepartmentMapping>();
    public DbSet<RoleFeatureMapping> RoleFeatureMappings => Set<RoleFeatureMapping>();
    public DbSet<RolePagePermissionMapping> RolePagePermissionMappings => Set<RolePagePermissionMapping>();

    // IAppDbContext properties for compatibility
    public new DbSet<IdentityUserRole<Guid>> UserRoles => Set<IdentityUserRole<Guid>>();
    public DbSet<ApplicationUser> ApplicationUsers => Set<ApplicationUser>();
    public DbSet<ApplicationRole> ApplicationRoles => Set<ApplicationRole>();

    // Expose Database property for raw SQL operations
    public new DatabaseFacade Database => base.Database;

    // Override SaveChangesAsync to add logging and automatic audit field population
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var currentUser = GetCurrentUser();
            var utcNow = DateTime.UtcNow;

            var entries = ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Added || 
                           e.State == EntityState.Modified || 
                           e.State == EntityState.Deleted)
                .ToList();

            Console.WriteLine($"[AppDbContext] SaveChangesAsync called with {entries.Count} changed entities");

            foreach (var entry in entries)
            {
                Console.WriteLine($"  - {entry.Entity.GetType().Name}: {entry.State}");

                // Handle entities implementing IAuditableEntity
                if (entry.Entity is IAuditableEntity auditableEntity)
                {
                    switch (entry.State)
                    {
                        case EntityState.Added:
                            auditableEntity.CreatedAt = utcNow;
                            auditableEntity.CreatedBy = currentUser;
                            auditableEntity.UpdatedAt = utcNow;
                            auditableEntity.ModifiedBy = currentUser;
                            break;

                        case EntityState.Modified:
                            // Preserve original CreatedAt and CreatedBy
                            entry.Property(nameof(IAuditableEntity.CreatedAt)).IsModified = false;
                            entry.Property(nameof(IAuditableEntity.CreatedBy)).IsModified = false;
                            auditableEntity.UpdatedAt = utcNow;
                            auditableEntity.ModifiedBy = currentUser;
                            break;

                        case EntityState.Deleted:
                            // Convert hard delete to soft delete
                            if (entry.Entity is ISoftDeletable softDeletable)
                            {
                                entry.State = EntityState.Modified;
                                softDeletable.IsDeleted = true;
                                auditableEntity.UpdatedAt = utcNow;
                                auditableEntity.ModifiedBy = currentUser;
                            }
                            break;
                    }
                }
                // Handle entities that only implement ISoftDeletable
                else if (entry.Entity is ISoftDeletable softDeletableOnly && entry.State == EntityState.Deleted)
                {
                    entry.State = EntityState.Modified;
                    softDeletableOnly.IsDeleted = true;
                }
            }

            var result = await base.SaveChangesAsync(cancellationToken);
            Console.WriteLine($"[AppDbContext] SaveChangesAsync completed: {result} entities saved");
            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[AppDbContext] SaveChangesAsync FAILED: {ex.Message}");
            throw;
        }
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Configure Identity tables
        builder.Entity<ApplicationUser>(b =>
        {
            b.ToTable("ApplicationUsers");
            // Global query filter for soft delete
            b.HasQueryFilter(e => !e.IsDeleted);
        });

        builder.Entity<ApplicationRole>(b =>
        {
            b.ToTable("ApplicationRoles");
            b.HasOne(r => r.Department)
                .WithMany(d => d.Roles)
                .HasForeignKey(r => r.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);
            b.HasIndex(r => r.Name);
            // Global query filter for soft delete
            b.HasQueryFilter(e => !e.IsDeleted);
        });

        builder.Entity<IdentityUserRole<Guid>>(b =>
        {
            b.ToTable("UserRoles");
            b.HasIndex(ur => ur.UserId);
        });

        builder.Entity<IdentityUserLogin<Guid>>(b =>
        {
            b.ToTable("UserLogins");
        });

        builder.Entity<IdentityUserToken<Guid>>(b =>
        {
            b.ToTable("UserTokens");
        });

        builder.Entity<IdentityRoleClaim<Guid>>(b =>
        {
            b.ToTable("RoleClaims");
        });

        builder.Entity<IdentityUserClaim<Guid>>(b =>
        {
            b.ToTable("UserClaims");
        });

        // Configure Department
        builder.Entity<Department>(b =>
        {
            b.ToTable("Departments");
            b.Property(e => e.Name).IsRequired().HasMaxLength(100);
            b.Property(e => e.Description).HasMaxLength(500);
            b.HasIndex(e => e.Name).IsUnique();
            // Global query filter for soft delete
            b.HasQueryFilter(e => !e.IsDeleted);
        });

        // Configure Permission
        builder.Entity<Permission>(b =>
        {
            b.ToTable("Permissions");
            // Global query filter for soft delete
            b.HasQueryFilter(e => !e.IsDeleted);
        });

        // Configure Feature
        builder.Entity<Feature>(b =>
        {
            b.ToTable("Features");
            b.Property(e => e.Icon).HasMaxLength(100);
            b.HasOne(f => f.ParentFeature)
                .WithMany(f => f.SubFeatures)
                .HasForeignKey(f => f.ParentFeatureId);
            b.HasIndex(f => new { f.ParentFeatureId, f.IsActive });
            b.HasIndex(f => new { f.IsMainMenu, f.IsActive });
            b.HasIndex(f => f.DisplayOrder);
            // Global query filter for soft delete
            b.HasQueryFilter(e => !e.IsDeleted);
        });

        // Configure Page
        builder.Entity<Page>(b =>
        {
            b.ToTable("Pages");
            b.Property(e => e.Url).IsRequired().HasMaxLength(500);
            b.HasIndex(p => p.IsActive);
            b.HasIndex(p => p.DisplayOrder);
            b.HasIndex(p => p.MenuContext);
            // Global query filter for soft delete
            b.HasQueryFilter(e => !e.IsDeleted);
        });

        // Configure RolePermissionMapping
        builder.Entity<RolePermissionMapping>(b =>
        {
            b.ToTable("RolePermissionMappings");
            b.HasOne(rpm => rpm.Role)
                .WithMany(r => r.RolePermissions)
                .HasForeignKey(rpm => rpm.RoleId)
                .OnDelete(DeleteBehavior.Cascade);
            b.HasOne(rpm => rpm.Permission)
                .WithMany(p => p.RolePermissions)
                .HasForeignKey(rpm => rpm.PermissionId);
            b.HasIndex(rpm => new { rpm.RoleId, rpm.PermissionId }).IsUnique();
            b.HasIndex(rpm => new { rpm.RoleId, rpm.IsActive });
            b.HasIndex(rpm => new { rpm.PermissionId, rpm.IsActive });
            // Global query filter for soft delete
            b.HasQueryFilter(e => !e.IsDeleted);
        });

        // Configure PagePermissionMapping - FIXED: Added .WithMany()
        builder.Entity<PagePermissionMapping>(b =>
        {
            b.ToTable("PagePermissionMappings");
            b.HasOne(ppm => ppm.Page)
                .WithMany(p => p.PagePermissions)
                .HasForeignKey(ppm => ppm.PageId);
            b.HasOne(ppm => ppm.Permission)
                .WithMany(p => p.PagePermissions)  // FIXED: Added WithMany()
                .HasForeignKey(ppm => ppm.PermissionId);
            b.HasIndex(ppm => new { ppm.PageId, ppm.PermissionId }).IsUnique();
            b.HasIndex(ppm => ppm.IsActive);
            b.HasIndex(ppm => new { ppm.PermissionId, ppm.IsActive });
            // Global query filter for soft delete
            b.HasQueryFilter(e => !e.IsDeleted);
        });

        // Configure PageFeatureMapping
        builder.Entity<PageFeatureMapping>(b =>
        {
            b.ToTable("PageFeatureMappings");
            b.HasOne(pfm => pfm.Page)
                .WithMany(p => p.PageFeatures)
                .HasForeignKey(pfm => pfm.PageId);
            b.HasOne(pfm => pfm.Feature)
                .WithMany(f => f.PageFeatures)
                .HasForeignKey(pfm => pfm.FeatureId);
            b.HasIndex(pfm => new { pfm.PageId, pfm.FeatureId }).IsUnique();
            b.HasIndex(pfm => pfm.FeatureId);
            // Global query filter for soft delete
            b.HasQueryFilter(e => !e.IsDeleted);
        });

        // Configure RoleHierarchy - FIXED: Added .WithMany()
        builder.Entity<RoleHierarchy>(b =>
        {
            b.ToTable("RoleHierarchies");
            b.HasOne(rh => rh.Department)
                .WithMany(d => d.RoleHierarchies)
                .HasForeignKey(rh => rh.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);
            b.HasOne(rh => rh.ParentRole)
                .WithMany(r => r.ParentRoleHierarchies)  // FIXED: Added WithMany()
                .HasForeignKey(rh => rh.ParentRoleId)
                .OnDelete(DeleteBehavior.Restrict);
            b.HasOne(rh => rh.ChildRole)
                .WithMany(r => r.ChildRoleHierarchies)  // FIXED: Added WithMany()
                .HasForeignKey(rh => rh.ChildRoleId)
                .OnDelete(DeleteBehavior.Restrict);
            b.HasIndex(rh => new { rh.DepartmentId, rh.ParentRoleId, rh.ChildRoleId }).IsUnique();
            // Global query filter for soft delete
            b.HasQueryFilter(e => !e.IsDeleted);
        });

        // Configure UserRoleMapping - FIXED: Added .WithMany()
        builder.Entity<UserRoleMapping>(b =>
        {
            b.ToTable("UserRoleMappings");
            b.Property(e => e.AssignedByEmail).IsRequired().HasMaxLength(256);
            b.HasOne(urm => urm.User)
                .WithMany(u => u.UserRoleMappings)  // FIXED: Added WithMany()
                .HasForeignKey(urm => urm.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            b.HasOne(urm => urm.Role)
                .WithMany(r => r.UserRoleMappings)  // FIXED: Added WithMany()
                .HasForeignKey(urm => urm.RoleId)
                .OnDelete(DeleteBehavior.Cascade);
            b.HasOne(urm => urm.Department)
                .WithMany(d => d.UserRoleMappings)  // FIXED: Added WithMany()
                .HasForeignKey(urm => urm.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);
            b.HasIndex(urm => new { urm.UserId, urm.RoleId, urm.DepartmentId });
            b.HasIndex(urm => new { urm.UserId, urm.IsActive });
            // Global query filter for soft delete
            b.HasQueryFilter(e => !e.IsDeleted);
        });

        // Configure RoleDepartmentMapping - FIXED: Added .WithMany()
        builder.Entity<RoleDepartmentMapping>(b =>
        {
            b.ToTable("RoleDepartmentMappings");
            b.HasOne(rdm => rdm.Role)
                .WithMany(r => r.RoleDepartmentMappings)  // FIXED: Added WithMany()
                .HasForeignKey(rdm => rdm.RoleId)
                .OnDelete(DeleteBehavior.Cascade);
            b.HasOne(rdm => rdm.Department)
                .WithMany(d => d.RoleDepartmentMappings)  // FIXED: Added WithMany()
                .HasForeignKey(rdm => rdm.DepartmentId)
                .OnDelete(DeleteBehavior.Cascade);
            b.HasIndex(rdm => new { rdm.RoleId, rdm.DepartmentId }).IsUnique();
            // Global query filter for soft delete
            b.HasQueryFilter(e => !e.IsDeleted);
        });

        // Configure RoleFeatureMapping
        builder.Entity<RoleFeatureMapping>(b =>
        {
            b.ToTable("RoleFeatureMappings");
            b.HasOne(rfm => rfm.Role)
                .WithMany(r => r.RoleFeatureMappings)
                .HasForeignKey(rfm => rfm.RoleId)
                .OnDelete(DeleteBehavior.Cascade);
            b.HasOne(rfm => rfm.Feature)
                .WithMany(f => f.RoleFeatureMappings)
                .HasForeignKey(rfm => rfm.FeatureId)
                .OnDelete(DeleteBehavior.Cascade);
            b.HasOne(rfm => rfm.Department)
                .WithMany(d => d.RoleFeatureMappings)
                .HasForeignKey(rfm => rfm.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);
            b.HasIndex(rfm => new { rfm.RoleId, rfm.FeatureId, rfm.DepartmentId });
            b.HasIndex(rfm => new { rfm.RoleId, rfm.IsActive });
            // Global query filter for soft delete
            b.HasQueryFilter(e => !e.IsDeleted);
        });

        // Configure RolePagePermissionMapping
        builder.Entity<RolePagePermissionMapping>(b =>
        {
            b.ToTable("RolePagePermissionMappings");
            b.HasOne(rppm => rppm.Role)
                .WithMany(r => r.RolePagePermissionMappings)
                .HasForeignKey(rppm => rppm.RoleId)
                .OnDelete(DeleteBehavior.Cascade);
            b.HasOne(rppm => rppm.Page)
                .WithMany(p => p.RolePagePermissionMappings)
                .HasForeignKey(rppm => rppm.PageId)
                .OnDelete(DeleteBehavior.Cascade);
            b.HasOne(rppm => rppm.Permission)
                .WithMany()
                .HasForeignKey(rppm => rppm.PermissionId)
                .OnDelete(DeleteBehavior.Cascade);
            b.HasOne(rppm => rppm.Department)
                .WithMany(d => d.RolePagePermissionMappings)
                .HasForeignKey(rppm => rppm.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);
            b.HasIndex(rppm => new { rppm.RoleId, rppm.PageId, rppm.PermissionId, rppm.DepartmentId });
            b.HasIndex(rppm => new { rppm.RoleId, rppm.IsActive });
            // Global query filter for soft delete
            b.HasQueryFilter(e => !e.IsDeleted);
        });

        // Configure UserAddress
        builder.Entity<UserAddress>(b =>
        {
            b.ToTable("UserAddresses");
            b.Property(e => e.Id).ValueGeneratedOnAdd();
            b.Property(e => e.CreatedAt).HasDefaultValueSql("timezone('utc', now())");
            // Global query filter for soft delete
            b.HasQueryFilter(e => !e.IsDeleted);
        });

        // Configure UserRefreshToken
        builder.Entity<UserRefreshToken>(b =>
        {
            b.ToTable("UserRefreshTokens");
            // Global query filter for soft delete
            b.HasQueryFilter(e => !e.IsDeleted);
        });
    }
}
