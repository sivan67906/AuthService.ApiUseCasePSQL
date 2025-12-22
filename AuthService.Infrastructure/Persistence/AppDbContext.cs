using System.Linq;
using AuthService.Application.Common.Interfaces;
using AuthService.Domain.Entities;
using AuthService.Domain.Entities.Masters;
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

    // Company Module entities
    public DbSet<Company> Companies => Set<Company>();
    public DbSet<Country> Countries => Set<Country>();
    public DbSet<State> States => Set<State>();
    public DbSet<City> Cities => Set<City>();
    public DbSet<Currency> Currencies => Set<Currency>();
    public DbSet<TimeZoneMaster> TimeZones => Set<TimeZoneMaster>();
    public DbSet<CountryTimeZone> CountryTimeZones => Set<CountryTimeZone>();

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

                // Convert all DateTime properties to UTC for PostgreSQL compatibility
                Console.WriteLine($"  [UTC Conversion] Processing entity: {entry.Entity.GetType().Name}");
                
                var entityType = entry.Entity.GetType();
                var dateTimeProperties = entityType.GetProperties()
                    .Where(p => p.PropertyType == typeof(DateTime) || p.PropertyType == typeof(DateTime?))
                    .ToList();
                
                Console.WriteLine($"  [UTC Conversion] Found {dateTimeProperties.Count} DateTime properties");
                
                foreach (var prop in dateTimeProperties)
                {
                    var currentValue = prop.GetValue(entry.Entity);
                    Console.WriteLine($"    Property: {prop.Name}, Type: {prop.PropertyType}, Value: {currentValue}");
                    
                    if (currentValue is DateTime dateTime)
                    {
                        Console.WriteLine($"      Current Kind: {dateTime.Kind}");
                        
                        if (dateTime.Kind == DateTimeKind.Unspecified)
                        {
                            var utcValue = DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
                            prop.SetValue(entry.Entity, utcValue);
                            Console.WriteLine($"      CONVERTED Unspecified -> UTC: {utcValue:O}");
                        }
                        else if (dateTime.Kind == DateTimeKind.Local)
                        {
                            var utcValue = dateTime.ToUniversalTime();
                            prop.SetValue(entry.Entity, utcValue);
                            Console.WriteLine($"      CONVERTED Local -> UTC: {utcValue:O}");
                        }
                        else
                        {
                            Console.WriteLine($"      Already UTC: {dateTime:O}");
                        }
                    }
                    else if (currentValue == null)
                    {
                        Console.WriteLine($"      Value is NULL");
                    }
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
            b.Property(e => e.Code).HasMaxLength(10);
            b.HasOne(r => r.Department)
                .WithMany(d => d.Roles)
                .HasForeignKey(r => r.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);
            b.HasIndex(r => r.Code).IsUnique().HasFilter("\"Code\" IS NOT NULL");
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
            b.Property(e => e.Code).IsRequired().HasMaxLength(10);
            b.Property(e => e.Name).IsRequired().HasMaxLength(100);
            b.Property(e => e.Description).HasMaxLength(500);
            b.HasIndex(e => e.Code).IsUnique();
            b.HasIndex(e => e.Name);
            // Global query filter for soft delete
            b.HasQueryFilter(e => !e.IsDeleted);
        });

        // Configure Permission
        builder.Entity<Permission>(b =>
        {
            b.ToTable("Permissions");
            b.Property(e => e.Code).IsRequired().HasMaxLength(10);
            b.Property(e => e.Name).IsRequired().HasMaxLength(100);
            b.Property(e => e.Description).HasMaxLength(500);
            b.HasIndex(e => e.Code).IsUnique();
            b.HasIndex(e => e.Name);
            // Global query filter for soft delete
            b.HasQueryFilter(e => !e.IsDeleted);
        });

        // Configure Feature
        builder.Entity<Feature>(b =>
        {
            b.ToTable("Features");
            b.Property(e => e.Code).IsRequired().HasMaxLength(10);
            b.Property(e => e.Name).IsRequired().HasMaxLength(100);
            b.Property(e => e.Description).HasMaxLength(500);
            b.Property(e => e.Icon).HasMaxLength(100);
            b.HasOne(f => f.ParentFeature)
                .WithMany(f => f.SubFeatures)
                .HasForeignKey(f => f.ParentFeatureId);
            b.HasIndex(e => e.Code).IsUnique();
            b.HasIndex(e => e.Name);
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
            b.Property(e => e.Code).IsRequired().HasMaxLength(10);
            b.Property(e => e.Name).IsRequired().HasMaxLength(100);
            b.Property(e => e.Description).HasMaxLength(500);
            b.Property(e => e.Url).IsRequired().HasMaxLength(500);
            b.HasIndex(e => e.Code).IsUnique();
            b.HasIndex(e => e.Name);
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

        // ========================
        // Company Module Entities
        // ========================

        // Configure Country
        builder.Entity<Country>(b =>
        {
            b.ToTable("Countries");
            b.Property(e => e.Name).IsRequired().HasMaxLength(100);
            b.Property(e => e.Code).IsRequired().HasMaxLength(2);
            b.Property(e => e.Code3).HasMaxLength(3);
            b.Property(e => e.NumericCode).HasMaxLength(3);
            b.Property(e => e.PhoneCode).HasMaxLength(10);
            b.Property(e => e.CurrencyCode).HasMaxLength(3);
            b.HasIndex(e => e.Code).IsUnique();
            b.HasIndex(e => e.Name);
            b.HasQueryFilter(e => !e.IsDeleted);
        });

        // Configure State
        builder.Entity<State>(b =>
        {
            b.ToTable("States");
            b.Property(e => e.Name).IsRequired().HasMaxLength(100);
            b.Property(e => e.Code).IsRequired().HasMaxLength(10);
            b.HasOne(e => e.Country)
                .WithMany(c => c.States)
                .HasForeignKey(e => e.CountryId)
                .OnDelete(DeleteBehavior.Restrict);
            b.HasIndex(e => new { e.CountryId, e.Code }).IsUnique();
            b.HasIndex(e => e.Name);
            b.HasQueryFilter(e => !e.IsDeleted);
        });

        // Configure City
        builder.Entity<City>(b =>
        {
            b.ToTable("Cities");
            b.Property(e => e.Name).IsRequired().HasMaxLength(100);
            b.Property(e => e.PostalCode).HasMaxLength(20);
            b.HasOne(e => e.State)
                .WithMany(s => s.Cities)
                .HasForeignKey(e => e.StateId)
                .OnDelete(DeleteBehavior.Restrict);
            b.HasIndex(e => new { e.StateId, e.Name });
            b.HasQueryFilter(e => !e.IsDeleted);
        });

        // Configure Currency
        builder.Entity<Currency>(b =>
        {
            b.ToTable("Currencies");
            b.Property(e => e.Code).IsRequired().HasMaxLength(3);
            b.Property(e => e.Name).IsRequired().HasMaxLength(100);
            b.Property(e => e.Symbol).IsRequired().HasMaxLength(10);
            b.HasIndex(e => e.Code).IsUnique();
            b.HasQueryFilter(e => !e.IsDeleted);
        });

        // Configure TimeZoneMaster
        builder.Entity<TimeZoneMaster>(b =>
        {
            b.ToTable("TimeZones");
            b.Property(e => e.Name).IsRequired().HasMaxLength(100);
            b.Property(e => e.Identifier).IsRequired().HasMaxLength(100);
            b.Property(e => e.Offset).IsRequired().HasMaxLength(20);
            b.Property(e => e.DisplayName).HasMaxLength(200);
            b.HasIndex(e => e.Identifier).IsUnique();
            b.HasQueryFilter(e => !e.IsDeleted);
        });

        // Configure Company
        builder.Entity<Company>(b =>
        {
            b.ToTable("Companies");
            
            // Identity fields
            b.Property(e => e.CompanyCode).IsRequired().HasMaxLength(10);
            b.Property(e => e.LegalName).IsRequired().HasMaxLength(200);
            b.Property(e => e.TradeName).HasMaxLength(150);
            b.Property(e => e.ShortName).HasMaxLength(50);
            
            // Registration fields
            b.Property(e => e.RegistrationNumber).HasMaxLength(50);
            b.Property(e => e.PANNumber).HasMaxLength(10);
            b.Property(e => e.GSTIN).HasMaxLength(15);
            b.Property(e => e.TANNumber).HasMaxLength(10);
            b.Property(e => e.OtherTaxId).HasMaxLength(50);
            
            // Address fields
            b.Property(e => e.AddressLine1).IsRequired().HasMaxLength(200);
            b.Property(e => e.AddressLine2).HasMaxLength(200);
            b.Property(e => e.PostalCode).IsRequired().HasMaxLength(20);
            
            // Contact fields
            b.Property(e => e.PrimaryContactName).HasMaxLength(100);
            b.Property(e => e.PrimaryEmail).HasMaxLength(150);
            b.Property(e => e.PrimaryPhone).HasMaxLength(30);
            b.Property(e => e.WebsiteUrl).HasMaxLength(200);
            // LogoFileUrl - supports base64 encoded images (no max length)
            
            // Notes
            b.Property(e => e.Notes).HasMaxLength(1000);

            // Indexes
            b.HasIndex(e => e.CompanyCode).IsUnique();
            b.HasIndex(e => e.LegalName);
            b.HasIndex(e => e.Status);
            b.HasIndex(e => e.PANNumber).IsUnique().HasFilter("\"PANNumber\" IS NOT NULL");
            b.HasIndex(e => e.GSTIN).IsUnique().HasFilter("\"GSTIN\" IS NOT NULL");

            // Self-referencing relationship for parent company
            b.HasOne(e => e.ParentCompany)
                .WithMany(e => e.ChildCompanies)
                .HasForeignKey(e => e.ParentCompanyId)
                .OnDelete(DeleteBehavior.Restrict);

            // Registration country/state relationships
            b.HasOne(e => e.RegistrationCountry)
                .WithMany(c => c.RegisteredCompanies)
                .HasForeignKey(e => e.RegistrationCountryId)
                .OnDelete(DeleteBehavior.Restrict);
            
            b.HasOne(e => e.RegistrationState)
                .WithMany(s => s.RegisteredCompanies)
                .HasForeignKey(e => e.RegistrationStateId)
                .OnDelete(DeleteBehavior.Restrict);

            // Address relationships
            b.HasOne(e => e.AddressCountry)
                .WithMany(c => c.AddressCompanies)
                .HasForeignKey(e => e.CountryId)
                .OnDelete(DeleteBehavior.Restrict);
            
            b.HasOne(e => e.AddressState)
                .WithMany(s => s.AddressCompanies)
                .HasForeignKey(e => e.StateId)
                .OnDelete(DeleteBehavior.Restrict);
            
            b.HasOne(e => e.City)
                .WithMany(c => c.AddressCompanies)
                .HasForeignKey(e => e.CityId)
                .OnDelete(DeleteBehavior.Restrict);
            
            b.HasOne(e => e.TimeZone)
                .WithMany(t => t.Companies)
                .HasForeignKey(e => e.TimeZoneId)
                .OnDelete(DeleteBehavior.Restrict);

            // Currency relationships
            b.HasOne(e => e.BaseCurrency)
                .WithMany(c => c.BaseCurrencyCompanies)
                .HasForeignKey(e => e.BaseCurrencyId)
                .OnDelete(DeleteBehavior.Restrict);
            
            b.HasOne(e => e.ReportingCurrency)
                .WithMany(c => c.ReportingCurrencyCompanies)
                .HasForeignKey(e => e.ReportingCurrencyId)
                .OnDelete(DeleteBehavior.Restrict);

            // Global query filter for soft delete
            b.HasQueryFilter(e => !e.IsDeleted);
        });

        // Configure CountryTimeZone
        builder.Entity<CountryTimeZone>(b =>
        {
            b.ToTable("CountryTimeZones");
            
            b.HasOne(ctz => ctz.Country)
                .WithMany()
                .HasForeignKey(ctz => ctz.CountryId)
                .OnDelete(DeleteBehavior.Cascade);
            
            b.HasOne(ctz => ctz.TimeZoneEntity)
                .WithMany()
                .HasForeignKey(ctz => ctz.TimeZoneId)
                .OnDelete(DeleteBehavior.Cascade);
            
            b.HasIndex(ctz => ctz.CountryId);
            b.HasIndex(ctz => ctz.TimeZoneId);
            b.HasIndex(ctz => new { ctz.CountryId, ctz.TimeZoneId }).IsUnique();
            
            // Global query filter for soft delete
            b.HasQueryFilter(e => !e.IsDeleted);
        });
    }
}
