using MockQueryable.Moq;

namespace AuthService.Tests.Unit.Infrastructure.Services;

/// <summary>
/// Unit tests for UserAuthorizationService
/// Tests permission checking, page access, department access, menus, and roles
/// </summary>
public class UserAuthorizationServiceTests
{
    private readonly Mock<IAppDbContext> _dbContextMock;
    private readonly Mock<ILogger<UserAuthorizationService>> _loggerMock;
    private readonly UserAuthorizationService _service;

    // Test data
    private readonly Guid _superAdminUserId = Guid.NewGuid();
    private readonly Guid _departmentAdminUserId = Guid.NewGuid();
    private readonly Guid _regularUserId = Guid.NewGuid();
    private readonly Guid _noRoleUserId = Guid.NewGuid();

    private readonly Guid _superAdminRoleId = Guid.NewGuid();
    private readonly Guid _departmentAdminRoleId = Guid.NewGuid();
    private readonly Guid _staffRoleId = Guid.NewGuid();

    private readonly Guid _financeDepartmentId = Guid.NewGuid();
    private readonly Guid _hrDepartmentId = Guid.NewGuid();

    private readonly Guid _dashboardPageId = Guid.NewGuid();
    private readonly Guid _reportsPageId = Guid.NewGuid();

    private readonly Guid _viewPermissionId = Guid.NewGuid();
    private readonly Guid _createPermissionId = Guid.NewGuid();

    private readonly Guid _mainMenuFeatureId = Guid.NewGuid();
    private readonly Guid _subMenuFeatureId = Guid.NewGuid();

    public UserAuthorizationServiceTests()
    {
        _dbContextMock = new Mock<IAppDbContext>();
        _loggerMock = new Mock<ILogger<UserAuthorizationService>>();

        SetupTestData();

        _service = new UserAuthorizationService(_dbContextMock.Object, _loggerMock.Object);
    }

    private void SetupTestData()
    {
        // Setup roles
        var roles = new List<ApplicationRole>
        {
            new() { Id = _superAdminRoleId, Name = SystemRoles.SuperAdmin, IsActive = true },
            new() { Id = _departmentAdminRoleId, Name = SystemRoles.DepartmentAdmin, DepartmentId = _financeDepartmentId, IsActive = true },
            new() { Id = _staffRoleId, Name = "Staff", DepartmentId = _financeDepartmentId, IsActive = true }
        };

        // Setup user role mappings
        var userRoleMappings = new List<UserRoleMapping>
        {
            new() { Id = Guid.NewGuid(), UserId = _superAdminUserId, RoleId = _superAdminRoleId, DepartmentId = null, IsActive = true },
            new() { Id = Guid.NewGuid(), UserId = _departmentAdminUserId, RoleId = _departmentAdminRoleId, DepartmentId = _financeDepartmentId, IsActive = true },
            new() { Id = Guid.NewGuid(), UserId = _regularUserId, RoleId = _staffRoleId, DepartmentId = _financeDepartmentId, IsActive = true }
        };

        // Setup pages
        var pages = new List<Page>
        {
            new() { Id = _dashboardPageId, Code = "DASH", Name = "Dashboard", Url = "/dashboard", IsActive = true, IsDeleted = false },
            new() { Id = _reportsPageId, Code = "RPT", Name = "Reports", Url = "/reports", IsActive = true, IsDeleted = false }
        };

        // Setup permissions
        var permissions = new List<Permission>
        {
            new() { Id = _viewPermissionId, Code = "VIEW", Name = "View" },
            new() { Id = _createPermissionId, Code = "CREATE", Name = "Create" }
        };

        // Setup role page permission mappings
        var rolePagePermissionMappings = new List<RolePagePermissionMapping>
        {
            // SuperAdmin can view Dashboard (null department = global)
            new() { Id = Guid.NewGuid(), RoleId = _superAdminRoleId, PageId = _dashboardPageId, PermissionId = _viewPermissionId, DepartmentId = null, IsActive = true, IsDeleted = false },
            // Staff can view Dashboard in Finance department
            new() { Id = Guid.NewGuid(), RoleId = _staffRoleId, PageId = _dashboardPageId, PermissionId = _viewPermissionId, DepartmentId = _financeDepartmentId, IsActive = true, IsDeleted = false },
            // Staff can view Reports in Finance department
            new() { Id = Guid.NewGuid(), RoleId = _staffRoleId, PageId = _reportsPageId, PermissionId = _viewPermissionId, DepartmentId = _financeDepartmentId, IsActive = true, IsDeleted = false }
        };

        // Setup features
        var features = new List<Feature>
        {
            new() { Id = _mainMenuFeatureId, Code = "MAIN", Name = "Main Menu", IsMainMenu = true, ParentFeatureId = null, IsActive = true, IsDeleted = false, Level = 0, DisplayOrder = 1 },
            new() { Id = _subMenuFeatureId, Code = "SUB", Name = "Sub Menu", IsMainMenu = false, ParentFeatureId = _mainMenuFeatureId, IsActive = true, IsDeleted = false, Level = 1, DisplayOrder = 1 }
        };

        // Setup role feature mappings
        var roleFeatureMappings = new List<RoleFeatureMapping>
        {
            new() { Id = Guid.NewGuid(), RoleId = _superAdminRoleId, FeatureId = _mainMenuFeatureId, DepartmentId = null, IsActive = true, IsDeleted = false },
            new() { Id = Guid.NewGuid(), RoleId = _superAdminRoleId, FeatureId = _subMenuFeatureId, DepartmentId = null, IsActive = true, IsDeleted = false },
            new() { Id = Guid.NewGuid(), RoleId = _staffRoleId, FeatureId = _mainMenuFeatureId, DepartmentId = _financeDepartmentId, IsActive = true, IsDeleted = false }
        };

        // Setup page feature mappings
        var pageFeatureMappings = new List<PageFeatureMapping>
        {
            new() { Id = Guid.NewGuid(), PageId = _dashboardPageId, FeatureId = _subMenuFeatureId, IsActive = true, IsDeleted = false }
        };

        // Setup mock DbSets using MockQueryable
        _dbContextMock.Setup(x => x.ApplicationRoles).Returns(roles.AsQueryable().BuildMockDbSet().Object);
        _dbContextMock.Setup(x => x.UserRoleMappings).Returns(userRoleMappings.AsQueryable().BuildMockDbSet().Object);
        _dbContextMock.Setup(x => x.Pages).Returns(pages.AsQueryable().BuildMockDbSet().Object);
        _dbContextMock.Setup(x => x.Permissions).Returns(permissions.AsQueryable().BuildMockDbSet().Object);
        _dbContextMock.Setup(x => x.RolePagePermissionMappings).Returns(rolePagePermissionMappings.AsQueryable().BuildMockDbSet().Object);
        _dbContextMock.Setup(x => x.Features).Returns(features.AsQueryable().BuildMockDbSet().Object);
        _dbContextMock.Setup(x => x.RoleFeatureMappings).Returns(roleFeatureMappings.AsQueryable().BuildMockDbSet().Object);
        _dbContextMock.Setup(x => x.PageFeatureMappings).Returns(pageFeatureMappings.AsQueryable().BuildMockDbSet().Object);
    }

    #region UserHasPermissionAsync Tests

    [Fact]
    public async Task UserHasPermissionAsync_SuperAdmin_ShouldReturnTrue()
    {
        // Arrange - SuperAdmin should have all permissions

        // Act
        var result = await _service.UserHasPermissionAsync(_superAdminUserId, "View");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task UserHasPermissionAsync_RegularUserWithPermission_ShouldReturnTrue()
    {
        // Arrange - Regular user with View permission in their department

        // Act
        var result = await _service.UserHasPermissionAsync(_regularUserId, "View");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task UserHasPermissionAsync_UserWithoutRole_ShouldReturnFalse()
    {
        // Arrange - User with no role mappings

        // Act
        var result = await _service.UserHasPermissionAsync(_noRoleUserId, "View");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task UserHasPermissionAsync_NonExistentPermission_ShouldReturnFalse()
    {
        // Arrange
        var permissionName = "NonExistentPermission";

        // Act
        var result = await _service.UserHasPermissionAsync(_regularUserId, permissionName);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task UserHasPermissionAsync_InvalidUserId_ShouldReturnFalse()
    {
        // Arrange
        var invalidUserId = Guid.NewGuid();

        // Act
        var result = await _service.UserHasPermissionAsync(invalidUserId, "View");

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region UserHasAccessToPageAsync Tests

    [Fact]
    public async Task UserHasAccessToPageAsync_SuperAdmin_ShouldReturnTrue()
    {
        // Arrange

        // Act
        var result = await _service.UserHasAccessToPageAsync(_superAdminUserId, "Dashboard");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task UserHasAccessToPageAsync_RegularUserWithAccess_ShouldReturnTrue()
    {
        // Arrange

        // Act
        var result = await _service.UserHasAccessToPageAsync(_regularUserId, "Dashboard");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task UserHasAccessToPageAsync_NonExistentPage_ShouldReturnFalse()
    {
        // Arrange

        // Act
        var result = await _service.UserHasAccessToPageAsync(_regularUserId, "NonExistentPage");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task UserHasAccessToPageAsync_UserWithoutRole_ShouldReturnFalse()
    {
        // Arrange

        // Act
        var result = await _service.UserHasAccessToPageAsync(_noRoleUserId, "Dashboard");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task UserHasAccessToPageAsync_InvalidUserId_ShouldReturnFalse()
    {
        // Arrange
        var invalidUserId = Guid.NewGuid();

        // Act
        var result = await _service.UserHasAccessToPageAsync(invalidUserId, "Dashboard");

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region UserHasAccessToDepartmentAsync Tests

    [Fact]
    public async Task UserHasAccessToDepartmentAsync_NullDepartment_ShouldReturnTrue()
    {
        // Arrange

        // Act
        var result = await _service.UserHasAccessToDepartmentAsync(_regularUserId, null);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task UserHasAccessToDepartmentAsync_UserDepartment_ShouldReturnTrue()
    {
        // Arrange

        // Act
        var result = await _service.UserHasAccessToDepartmentAsync(_regularUserId, _financeDepartmentId);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task UserHasAccessToDepartmentAsync_DifferentDepartment_ShouldReturnFalse()
    {
        // Arrange

        // Act
        var result = await _service.UserHasAccessToDepartmentAsync(_regularUserId, _hrDepartmentId);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task UserHasAccessToDepartmentAsync_SuperAdmin_ShouldReturnTrueForAnyDepartment()
    {
        // Arrange - SuperAdmin has null department, can access any

        // Act
        var result = await _service.UserHasAccessToDepartmentAsync(_superAdminUserId, _hrDepartmentId);

        // Assert
        result.Should().BeTrue();
    }

    #endregion

    #region GetUserRolesAsync Tests

    [Fact]
    public async Task GetUserRolesAsync_ValidUser_ShouldReturnRoles()
    {
        // Arrange

        // Act
        var result = await _service.GetUserRolesAsync(_superAdminUserId);

        // Assert
        result.Should().NotBeEmpty();
        result.Should().Contain(SystemRoles.SuperAdmin);
    }

    [Fact]
    public async Task GetUserRolesAsync_UserWithMultipleRoles_ShouldReturnAllRoles()
    {
        // Arrange

        // Act
        var result = await _service.GetUserRolesAsync(_regularUserId);

        // Assert
        result.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetUserRolesAsync_UserWithoutRoles_ShouldReturnEmptyList()
    {
        // Arrange

        // Act
        var result = await _service.GetUserRolesAsync(_noRoleUserId);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetUserRolesAsync_InvalidUserId_ShouldReturnEmptyList()
    {
        // Arrange
        var invalidUserId = Guid.NewGuid();

        // Act
        var result = await _service.GetUserRolesAsync(invalidUserId);

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region GetUserDepartmentAsync Tests

    [Fact]
    public async Task GetUserDepartmentAsync_RegularUser_ShouldReturnDepartment()
    {
        // Arrange

        // Act
        var result = await _service.GetUserDepartmentAsync(_regularUserId);

        // Assert
        result.Should().Be(_financeDepartmentId);
    }

    [Fact]
    public async Task GetUserDepartmentAsync_SuperAdmin_ShouldReturnNull()
    {
        // Arrange

        // Act
        var result = await _service.GetUserDepartmentAsync(_superAdminUserId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetUserDepartmentAsync_UserWithoutRoles_ShouldReturnNull()
    {
        // Arrange

        // Act
        var result = await _service.GetUserDepartmentAsync(_noRoleUserId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetUserDepartmentAsync_InvalidUserId_ShouldReturnNull()
    {
        // Arrange
        var invalidUserId = Guid.NewGuid();

        // Act
        var result = await _service.GetUserDepartmentAsync(invalidUserId);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetUserMenusAsync Tests

    [Fact]
    public async Task GetUserMenusAsync_SuperAdmin_ShouldReturnMenus()
    {
        // Arrange

        // Act
        var result = await _service.GetUserMenusAsync(_superAdminUserId);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetUserMenusAsync_UserWithoutRoles_ShouldReturnEmptyList()
    {
        // Arrange

        // Act
        var result = await _service.GetUserMenusAsync(_noRoleUserId);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetUserMenusAsync_InvalidUserId_ShouldReturnEmptyList()
    {
        // Arrange
        var invalidUserId = Guid.NewGuid();

        // Act
        var result = await _service.GetUserMenusAsync(invalidUserId);

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region GetUserPagePermissionsAsync Tests

    [Fact]
    public async Task GetUserPagePermissionsAsync_SuperAdmin_ShouldReturnPermissions()
    {
        // Arrange

        // Act
        var result = await _service.GetUserPagePermissionsAsync(_superAdminUserId, "Dashboard");

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetUserPagePermissionsAsync_RegularUser_ShouldReturnPermissions()
    {
        // Arrange

        // Act
        var result = await _service.GetUserPagePermissionsAsync(_regularUserId, "Dashboard");

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetUserPagePermissionsAsync_NonExistentPage_ShouldReturnEmptyList()
    {
        // Arrange

        // Act
        var result = await _service.GetUserPagePermissionsAsync(_regularUserId, "NonExistentPage");

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetUserPagePermissionsAsync_UserWithoutRoles_ShouldReturnEmptyList()
    {
        // Arrange

        // Act
        var result = await _service.GetUserPagePermissionsAsync(_noRoleUserId, "Dashboard");

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region Edge Cases and Exception Handling

    [Fact]
    public async Task AllMethods_WithEmptyGuid_ShouldHandleGracefully()
    {
        // Arrange
        var emptyGuid = Guid.Empty;

        // Act & Assert
        var permission = await _service.UserHasPermissionAsync(emptyGuid, "View");
        permission.Should().BeFalse();

        var pageAccess = await _service.UserHasAccessToPageAsync(emptyGuid, "Dashboard");
        pageAccess.Should().BeFalse();

        var departmentAccess = await _service.UserHasAccessToDepartmentAsync(emptyGuid, _financeDepartmentId);
        departmentAccess.Should().BeFalse();

        var roles = await _service.GetUserRolesAsync(emptyGuid);
        roles.Should().BeEmpty();

        var department = await _service.GetUserDepartmentAsync(emptyGuid);
        department.Should().BeNull();

        var menus = await _service.GetUserMenusAsync(emptyGuid);
        menus.Should().BeEmpty();
    }

    [Theory]
    [InlineData("")]
    [InlineData("View")]
    [InlineData("Create")]
    [InlineData("Update")]
    [InlineData("Delete")]
    [InlineData("Special Permission With Spaces")]
    public async Task UserHasPermissionAsync_VariousPermissionNames_ShouldHandle(string permissionName)
    {
        // Act & Assert - should not throw
        var act = async () => await _service.UserHasPermissionAsync(_regularUserId, permissionName);
        await act.Should().NotThrowAsync();
    }

    [Theory]
    [InlineData("")]
    [InlineData("Dashboard")]
    [InlineData("Non-Existent Page")]
    [InlineData("Page With Special Chars !@#")]
    public async Task UserHasAccessToPageAsync_VariousPageNames_ShouldHandle(string pageName)
    {
        // Act & Assert - should not throw
        var act = async () => await _service.UserHasAccessToPageAsync(_regularUserId, pageName);
        await act.Should().NotThrowAsync();
    }

    #endregion

    #region Interface Compliance

    [Fact]
    public void UserAuthorizationService_ShouldImplementInterface()
    {
        // Assert
        _service.Should().BeAssignableTo<IUserAuthorizationService>();
    }

    #endregion
}
