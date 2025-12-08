using AuthService.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace AuthService.Application.Common.Interfaces;
/// <summary>
/// Unified DbContext interface for all database operations (PostgreSQL)
/// </summary>
public interface IAppDbContext
{
    // User and Role entities
    DbSet<ApplicationUser> Users { get; }
    DbSet<ApplicationRole> Roles { get; }
    DbSet<ApplicationUser> ApplicationUsers { get; }
    DbSet<ApplicationRole> ApplicationRoles { get; }
    DbSet<IdentityUserRole<Guid>> UserRoles { get; }
    // Core RBAC entities
    DbSet<Department> Departments { get; }
    DbSet<Permission> Permissions { get; }
    DbSet<Feature> Features { get; }
    DbSet<Page> Pages { get; }
    // Mapping entities
    DbSet<RolePermissionMapping> RolePermissionMappings { get; }
    DbSet<RolePagePermissionMapping> RolePagePermissionMappings { get; }
    DbSet<RoleFeatureMapping> RoleFeatureMappings { get; }
    DbSet<PagePermissionMapping> PagePermissionMappings { get; }
    DbSet<PageFeatureMapping> PageFeatureMappings { get; }
    DbSet<RoleHierarchy> RoleHierarchies { get; }
    DbSet<UserRoleMapping> UserRoleMappings { get; }
    DbSet<RoleDepartmentMapping> RoleDepartmentMappings { get; }
    // Additional entities
    DbSet<UserAddress> UserAddresses { get; }
    DbSet<UserRefreshToken> RefreshTokens { get; }
    // Generic Set accessor
    DbSet<T> Set<T>() where T : class;
    // Database operations
    DatabaseFacade Database { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
