using AuthService.Application.Features.Menu.CheckPageAccess;
using AuthService.Application.Features.Menu.CheckPermission;
using AuthService.Application.Features.Menu.GetPagePermissions;
using AuthService.Application.Features.Menu.GetUserDepartment;
using AuthService.Application.Features.Menu.GetUserMenus;
using AuthService.Application.Features.Menu.GetUserRoles;

namespace AuthService.Tests.Unit.Application.Features.Menu;

#region GetUserMenus Tests

public class GetUserMenusQueryHandlerTests : ApplicationTestBase
{
    private readonly GetUserMenusQueryHandler _handler;
    private readonly Mock<ILogger<GetUserMenusQueryHandler>> _loggerMock;

    public GetUserMenusQueryHandlerTests()
    {
        _loggerMock = new Mock<ILogger<GetUserMenusQueryHandler>>();
        _handler = new GetUserMenusQueryHandler(
            UserAuthorizationServiceMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ValidUserId_ReturnsMenus()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var menus = new List<MenuItemDto>
        {
            new() { Name = "Dashboard" },
            new() { Name = "Settings" }
        };

        UserAuthorizationServiceMock
            .Setup(x => x.GetUserMenusAsync(userId))
            .ReturnsAsync(menus);

        var query = new GetUserMenusQuery(userId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handle_NoMenus_ReturnsEmptyList()
    {
        // Arrange
        var userId = Guid.NewGuid();

        UserAuthorizationServiceMock
            .Setup(x => x.GetUserMenusAsync(userId))
            .ReturnsAsync(new List<MenuItemDto>());

        var query = new GetUserMenusQuery(userId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ServiceThrowsException_ThrowsInvalidOperationException()
    {
        // Arrange
        var userId = Guid.NewGuid();

        UserAuthorizationServiceMock
            .Setup(x => x.GetUserMenusAsync(userId))
            .ThrowsAsync(new Exception("Test error"));

        var query = new GetUserMenusQuery(userId);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(query, CancellationToken.None));
    }
}

#endregion

#region CheckPageAccess Tests

public class CheckPageAccessQueryHandlerTests : ApplicationTestBase
{
    private readonly CheckPageAccessQueryHandler _handler;
    private readonly Mock<ILogger<CheckPageAccessQueryHandler>> _loggerMock;

    public CheckPageAccessQueryHandlerTests()
    {
        _loggerMock = new Mock<ILogger<CheckPageAccessQueryHandler>>();
        _handler = new CheckPageAccessQueryHandler(
            UserAuthorizationServiceMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_UserHasAccess_ReturnsTrue()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var pageName = "Dashboard";

        UserAuthorizationServiceMock
            .Setup(x => x.UserHasAccessToPageAsync(userId, pageName))
            .ReturnsAsync(true);

        var query = new CheckPageAccessQuery(userId, pageName);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_UserNoAccess_ReturnsFalse()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var pageName = "AdminPage";

        UserAuthorizationServiceMock
            .Setup(x => x.UserHasAccessToPageAsync(userId, pageName))
            .ReturnsAsync(false);

        var query = new CheckPageAccessQuery(userId, pageName);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeFalse();
    }
}

#endregion

#region CheckPermission Tests

public class CheckPermissionQueryHandlerTests : ApplicationTestBase
{
    private readonly CheckPermissionQueryHandler _handler;
    private readonly Mock<ILogger<CheckPermissionQueryHandler>> _loggerMock;

    public CheckPermissionQueryHandlerTests()
    {
        _loggerMock = new Mock<ILogger<CheckPermissionQueryHandler>>();
        _handler = new CheckPermissionQueryHandler(
            UserAuthorizationServiceMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_UserHasPermission_ReturnsTrue()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var permissionName = "CanView";

        UserAuthorizationServiceMock
            .Setup(x => x.UserHasPermissionAsync(userId, permissionName))
            .ReturnsAsync(true);

        var query = new CheckPermissionQuery(userId, permissionName);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_UserNoPermission_ReturnsFalse()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var permissionName = "CanDelete";

        UserAuthorizationServiceMock
            .Setup(x => x.UserHasPermissionAsync(userId, permissionName))
            .ReturnsAsync(false);

        var query = new CheckPermissionQuery(userId, permissionName);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_ServiceThrowsException_ThrowsInvalidOperationException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var permissionName = "TestPermission";

        UserAuthorizationServiceMock
            .Setup(x => x.UserHasPermissionAsync(userId, permissionName))
            .ThrowsAsync(new Exception("Test error"));

        var query = new CheckPermissionQuery(userId, permissionName);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(query, CancellationToken.None));
    }
}

#endregion

#region GetUserRoles Tests

public class GetUserRolesQueryHandlerTests : ApplicationTestBase
{
    private readonly GetUserRolesQueryHandler _handler;
    private readonly Mock<ILogger<GetUserRolesQueryHandler>> _loggerMock;

    public GetUserRolesQueryHandlerTests()
    {
        _loggerMock = new Mock<ILogger<GetUserRolesQueryHandler>>();
        _handler = new GetUserRolesQueryHandler(
            UserAuthorizationServiceMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_UserHasRoles_ReturnsRoles()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var roles = new List<string> { "Admin", "Manager" };

        UserAuthorizationServiceMock
            .Setup(x => x.GetUserRolesAsync(userId))
            .ReturnsAsync(roles);

        var query = new GetUserRolesQuery(userId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain("Admin");
        result.Should().Contain("Manager");
    }

    [Fact]
    public async Task Handle_UserNoRoles_ReturnsEmptyList()
    {
        // Arrange
        var userId = Guid.NewGuid();

        UserAuthorizationServiceMock
            .Setup(x => x.GetUserRolesAsync(userId))
            .ReturnsAsync(new List<string>());

        var query = new GetUserRolesQuery(userId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
    }
}

#endregion

#region GetUserDepartment Tests

public class GetUserDepartmentQueryHandlerTests : ApplicationTestBase
{
    private readonly GetUserDepartmentQueryHandler _handler;
    private readonly Mock<ILogger<GetUserDepartmentQueryHandler>> _loggerMock;

    public GetUserDepartmentQueryHandlerTests()
    {
        _loggerMock = new Mock<ILogger<GetUserDepartmentQueryHandler>>();
        _handler = new GetUserDepartmentQueryHandler(
            UserAuthorizationServiceMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_UserHasDepartment_ReturnsDepartmentId()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var departmentId = Guid.NewGuid();

        UserAuthorizationServiceMock
            .Setup(x => x.GetUserDepartmentAsync(userId))
            .ReturnsAsync(departmentId);

        var query = new GetUserDepartmentQuery(userId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().Be(departmentId);
    }

    [Fact]
    public async Task Handle_UserNoDepartment_ReturnsNull()
    {
        // Arrange
        var userId = Guid.NewGuid();

        UserAuthorizationServiceMock
            .Setup(x => x.GetUserDepartmentAsync(userId))
            .ReturnsAsync((Guid?)null);

        var query = new GetUserDepartmentQuery(userId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }
}

#endregion

#region GetPagePermissions Tests

public class GetPagePermissionsQueryHandlerTests : ApplicationTestBase
{
    private readonly GetPagePermissionsQueryHandler _handler;
    private readonly Mock<ILogger<GetPagePermissionsQueryHandler>> _loggerMock;

    public GetPagePermissionsQueryHandlerTests()
    {
        _loggerMock = new Mock<ILogger<GetPagePermissionsQueryHandler>>();
        _handler = new GetPagePermissionsQueryHandler(
            UserAuthorizationServiceMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ValidRequest_ReturnsPagePermissions()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var pageName = "Dashboard";
        var permissions = new List<string> { "View", "Create" };

        UserAuthorizationServiceMock
            .Setup(x => x.GetUserPagePermissionsAsync(userId, pageName))
            .ReturnsAsync(permissions);

        var query = new GetPagePermissionsQuery(userId, pageName);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.CanView.Should().BeTrue();
        result.CanCreate.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_NoPermissions_ReturnsEmptyDto()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var pageName = "RestrictedPage";

        UserAuthorizationServiceMock
            .Setup(x => x.GetUserPagePermissionsAsync(userId, pageName))
            .ReturnsAsync(new List<string>());

        var query = new GetPagePermissionsQuery(userId, pageName);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.CanView.Should().BeFalse();
        result.CanCreate.Should().BeFalse();
    }
}

#endregion
