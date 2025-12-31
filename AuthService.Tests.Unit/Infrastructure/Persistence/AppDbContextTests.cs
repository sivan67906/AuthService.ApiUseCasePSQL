namespace AuthService.Tests.Unit.Infrastructure.Persistence;

/// <summary>
/// Unit tests for AppDbContext
/// Tests context initialization, entity configuration, and SaveChanges behavior
/// </summary>
public class AppDbContextTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly DbContextOptions<AppDbContext> _options;

    public AppDbContextTests()
    {
        _options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
            .Options;

        _context = new AppDbContext(_options);
    }

    public void Dispose()
    {
        _context?.Dispose();
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WithOptions_ShouldCreateContext()
    {
        // Arrange & Act
        using var context = new AppDbContext(_options);

        // Assert
        context.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_WithOptionsAndHttpContextAccessor_ShouldCreateContext()
    {
        // Arrange
        var httpContextAccessorMock = new Mock<IHttpContextAccessor>();

        // Act
        using var context = new AppDbContext(_options, httpContextAccessorMock.Object);

        // Assert
        context.Should().NotBeNull();
    }

    #endregion

    #region DbSet Property Tests

    [Fact]
    public void DbSets_ShouldBeAccessible()
    {
        // Assert - all DbSets should be accessible
        _context.Users.Should().NotBeNull();
        _context.Roles.Should().NotBeNull();
        _context.UserRoles.Should().NotBeNull();
        _context.ApplicationUsers.Should().NotBeNull();
        _context.ApplicationRoles.Should().NotBeNull();
        _context.Departments.Should().NotBeNull();
        _context.Permissions.Should().NotBeNull();
        _context.Features.Should().NotBeNull();
        _context.Pages.Should().NotBeNull();
        _context.RolePermissionMappings.Should().NotBeNull();
        _context.RolePagePermissionMappings.Should().NotBeNull();
        _context.RoleFeatureMappings.Should().NotBeNull();
        _context.PagePermissionMappings.Should().NotBeNull();
        _context.PageFeatureMappings.Should().NotBeNull();
        _context.RoleHierarchies.Should().NotBeNull();
        _context.UserRoleMappings.Should().NotBeNull();
        _context.RoleDepartmentMappings.Should().NotBeNull();
        _context.UserAddresses.Should().NotBeNull();
        _context.RefreshTokens.Should().NotBeNull();
        _context.Companies.Should().NotBeNull();
        _context.Countries.Should().NotBeNull();
        _context.States.Should().NotBeNull();
        _context.Cities.Should().NotBeNull();
        _context.Currencies.Should().NotBeNull();
        _context.TimeZones.Should().NotBeNull();
        _context.CountryTimeZones.Should().NotBeNull();
    }

    [Fact]
    public void Database_ShouldBeAccessible()
    {
        // Assert
        _context.Database.Should().NotBeNull();
    }

    [Fact]
    public void ChangeTracker_ShouldBeAccessible()
    {
        // Assert
        _context.ChangeTracker.Should().NotBeNull();
    }

    #endregion

    #region SaveChangesAsync Tests - Add Operations

    [Fact]
    public async Task SaveChangesAsync_AddDepartment_ShouldSetAuditFields()
    {
        // Arrange
        var department = new Department
        {
            Id = Guid.NewGuid(),
            Code = "DEP001",
            Name = "Test Department"
        };

        // Act
        _context.Departments.Add(department);
        await _context.SaveChangesAsync();

        // Assert
        department.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        department.CreatedBy.Should().Be("System");
        department.UpdatedAt.Should().NotBeNull();
        department.ModifiedBy.Should().Be("System");
    }

    [Fact]
    public async Task SaveChangesAsync_AddPermission_ShouldSetAuditFields()
    {
        // Arrange
        var permission = new Permission
        {
            Id = Guid.NewGuid(),
            Code = "VIEW",
            Name = "View"
        };

        // Act
        _context.Permissions.Add(permission);
        await _context.SaveChangesAsync();

        // Assert
        permission.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        permission.CreatedBy.Should().Be("System");
    }

    [Fact]
    public async Task SaveChangesAsync_AddPage_ShouldSetAuditFields()
    {
        // Arrange
        var page = new Page
        {
            Id = Guid.NewGuid(),
            Code = "PAGE001",
            Name = "Test Page",
            Url = "/test-page"
        };

        // Act
        _context.Pages.Add(page);
        await _context.SaveChangesAsync();

        // Assert
        page.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task SaveChangesAsync_AddFeature_ShouldSetAuditFields()
    {
        // Arrange
        var feature = new Feature
        {
            Id = Guid.NewGuid(),
            Code = "FT001",
            Name = "Test Feature",
            IsMainMenu = true
        };

        // Act
        _context.Features.Add(feature);
        await _context.SaveChangesAsync();

        // Assert
        feature.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    #endregion

    #region SaveChangesAsync Tests - Modify Operations

    [Fact]
    public async Task SaveChangesAsync_ModifyDepartment_ShouldUpdateAuditFields()
    {
        // Arrange
        var department = new Department
        {
            Id = Guid.NewGuid(),
            Code = "DEP001",
            Name = "Original Name"
        };
        _context.Departments.Add(department);
        await _context.SaveChangesAsync();

        var originalCreatedAt = department.CreatedAt;
        var originalCreatedBy = department.CreatedBy;

        // Act
        department.Name = "Updated Name";
        _context.Entry(department).State = EntityState.Modified;
        await _context.SaveChangesAsync();

        // Assert
        department.CreatedAt.Should().Be(originalCreatedAt);
        department.CreatedBy.Should().Be(originalCreatedBy);
        department.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        department.ModifiedBy.Should().Be("System");
    }

    #endregion

    #region SaveChangesAsync Tests - Soft Delete

    [Fact]
    public async Task SaveChangesAsync_DeleteDepartment_ShouldSoftDelete()
    {
        // Arrange
        var department = new Department
        {
            Id = Guid.NewGuid(),
            Code = "DEP001",
            Name = "To Be Deleted"
        };
        _context.Departments.Add(department);
        await _context.SaveChangesAsync();

        // Act
        _context.Departments.Remove(department);
        await _context.SaveChangesAsync();

        // Assert - entity should be soft deleted
        department.IsDeleted.Should().BeTrue();
    }

    #endregion

    #region SaveChangesAsync Tests - Return Value

    [Fact]
    public async Task SaveChangesAsync_SingleEntity_ShouldReturnOne()
    {
        // Arrange
        var department = new Department
        {
            Id = Guid.NewGuid(),
            Code = "DEP001",
            Name = "Test"
        };
        _context.Departments.Add(department);

        // Act
        var result = await _context.SaveChangesAsync();

        // Assert
        result.Should().Be(1);
    }

    [Fact]
    public async Task SaveChangesAsync_MultipleEntities_ShouldReturnCount()
    {
        // Arrange
        var departments = new List<Department>
        {
            new() { Id = Guid.NewGuid(), Code = "DEP001", Name = "Test 1" },
            new() { Id = Guid.NewGuid(), Code = "DEP002", Name = "Test 2" },
            new() { Id = Guid.NewGuid(), Code = "DEP003", Name = "Test 3" }
        };
        _context.Departments.AddRange(departments);

        // Act
        var result = await _context.SaveChangesAsync();

        // Assert
        result.Should().Be(3);
    }

    [Fact]
    public async Task SaveChangesAsync_NoChanges_ShouldReturnZero()
    {
        // Act
        var result = await _context.SaveChangesAsync();

        // Assert
        result.Should().Be(0);
    }

    #endregion

    #region DateTime UTC Conversion Tests

    [Fact]
    public async Task SaveChangesAsync_UnspecifiedDateTime_ShouldConvertToUtc()
    {
        // Arrange
        var department = new Department
        {
            Id = Guid.NewGuid(),
            Code = "DEP001",
            Name = "Test",
            CreatedAt = new DateTime(2024, 1, 1, 12, 0, 0, DateTimeKind.Unspecified)
        };
        _context.Departments.Add(department);

        // Act
        await _context.SaveChangesAsync();

        // Assert
        department.CreatedAt.Kind.Should().Be(DateTimeKind.Utc);
    }

    [Fact]
    public async Task SaveChangesAsync_LocalDateTime_ShouldConvertToUtc()
    {
        // Arrange
        var department = new Department
        {
            Id = Guid.NewGuid(),
            Code = "DEP001",
            Name = "Test",
            CreatedAt = new DateTime(2024, 1, 1, 12, 0, 0, DateTimeKind.Local)
        };
        _context.Departments.Add(department);

        // Act
        await _context.SaveChangesAsync();

        // Assert
        department.CreatedAt.Kind.Should().Be(DateTimeKind.Utc);
    }

    [Fact]
    public async Task SaveChangesAsync_UtcDateTime_ShouldRemainUtc()
    {
        // Arrange
        var utcTime = new DateTime(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        var department = new Department
        {
            Id = Guid.NewGuid(),
            Code = "DEP001",
            Name = "Test",
            CreatedAt = utcTime
        };
        _context.Departments.Add(department);

        // Act
        await _context.SaveChangesAsync();

        // Assert
        department.CreatedAt.Kind.Should().Be(DateTimeKind.Utc);
    }

    #endregion

    #region Entry Method Tests

    [Fact]
    public void Entry_ShouldReturnEntityEntry()
    {
        // Arrange
        var department = new Department
        {
            Id = Guid.NewGuid(),
            Code = "DEP001",
            Name = "Test"
        };
        _context.Departments.Add(department);

        // Act
        var entry = _context.Entry(department);

        // Assert
        entry.Should().NotBeNull();
        entry.State.Should().Be(EntityState.Added);
    }

    [Fact]
    public async Task Entry_AfterSave_ShouldShowUnchanged()
    {
        // Arrange
        var department = new Department
        {
            Id = Guid.NewGuid(),
            Code = "DEP001",
            Name = "Test"
        };
        _context.Departments.Add(department);
        await _context.SaveChangesAsync();

        // Act
        var entry = _context.Entry(department);

        // Assert
        entry.State.Should().Be(EntityState.Unchanged);
    }

    #endregion

    #region Set<T> Method Tests

    [Fact]
    public void Set_WithDepartment_ShouldReturnDbSet()
    {
        // Act
        var dbSet = _context.Set<Department>();

        // Assert
        dbSet.Should().NotBeNull();
    }

    [Fact]
    public void Set_WithPermission_ShouldReturnDbSet()
    {
        // Act
        var dbSet = _context.Set<Permission>();

        // Assert
        dbSet.Should().NotBeNull();
    }

    [Fact]
    public void Set_WithFeature_ShouldReturnDbSet()
    {
        // Act
        var dbSet = _context.Set<Feature>();

        // Assert
        dbSet.Should().NotBeNull();
    }

    #endregion

    #region IAppDbContext Implementation Tests

    [Fact]
    public void AppDbContext_ShouldImplementIAppDbContext()
    {
        // Assert
        _context.Should().BeAssignableTo<IAppDbContext>();
    }

    #endregion

    #region Entity Configuration Tests

    [Fact]
    public async Task ApplicationUser_TableName_ShouldBeApplicationUsers()
    {
        // Arrange
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            UserName = "test@example.com"
        };

        // Act
        _context.ApplicationUsers.Add(user);
        await _context.SaveChangesAsync();

        // Assert - user should be saved (table exists)
        var savedUser = await _context.ApplicationUsers.FindAsync(user.Id);
        savedUser.Should().NotBeNull();
    }

    [Fact]
    public async Task ApplicationRole_TableName_ShouldBeApplicationRoles()
    {
        // Arrange
        var role = new ApplicationRole
        {
            Id = Guid.NewGuid(),
            Name = "TestRole"
        };

        // Act
        _context.ApplicationRoles.Add(role);
        await _context.SaveChangesAsync();

        // Assert
        var savedRole = await _context.ApplicationRoles.FindAsync(role.Id);
        savedRole.Should().NotBeNull();
    }

    #endregion

    #region Query Filter Tests

    [Fact]
    public async Task SoftDeletedEntities_ShouldBeFilteredByDefault()
    {
        // Arrange
        var activeDept = new Department { Id = Guid.NewGuid(), Code = "ACTIVE", Name = "Active", IsDeleted = false };
        var deletedDept = new Department { Id = Guid.NewGuid(), Code = "DELETED", Name = "Deleted", IsDeleted = true };

        _context.Departments.Add(activeDept);
        _context.Departments.Add(deletedDept);
        await _context.SaveChangesAsync();

        // Act
        var departments = await _context.Departments.ToListAsync();

        // Assert - only active department should be returned
        departments.Should().ContainSingle();
        departments[0].Code.Should().Be("ACTIVE");
    }

    #endregion

    #region Cancellation Tests

    [Fact]
    public async Task SaveChangesAsync_WithCancellationToken_ShouldRespectCancellation()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var department = new Department
        {
            Id = Guid.NewGuid(),
            Code = "DEP001",
            Name = "Test"
        };
        _context.Departments.Add(department);

        // Act & Assert
        var act = async () => await _context.SaveChangesAsync(cts.Token);
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    #endregion

    #region Edge Cases

    [Fact]
    public async Task SaveChangesAsync_LargeNumberOfEntities_ShouldHandle()
    {
        // Arrange
        var departments = Enumerable.Range(1, 100)
            .Select(i => new Department
            {
                Id = Guid.NewGuid(),
                Code = $"DEP{i:D3}",
                Name = $"Department {i}"
            })
            .ToList();

        _context.Departments.AddRange(departments);

        // Act
        var result = await _context.SaveChangesAsync();

        // Assert
        result.Should().Be(100);
    }

    [Fact]
    public async Task SaveChangesAsync_MultipleSaves_ShouldWork()
    {
        // Arrange & Act
        for (int i = 1; i <= 5; i++)
        {
            var department = new Department
            {
                Id = Guid.NewGuid(),
                Code = $"DEP{i:D3}",
                Name = $"Department {i}"
            };
            _context.Departments.Add(department);
            await _context.SaveChangesAsync();
        }

        // Assert
        var count = await _context.Departments.CountAsync();
        count.Should().Be(5);
    }

    #endregion
}
