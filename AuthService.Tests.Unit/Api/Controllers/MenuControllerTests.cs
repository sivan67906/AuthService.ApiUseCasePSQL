using AuthService.Application.Features.Menu.CheckPageAccess;
using AuthService.Application.Features.Menu.CheckPermission;
using AuthService.Application.Features.Menu.GetPagePermissions;
using AuthService.Application.Features.Menu.GetUserDepartment;
using AuthService.Application.Features.Menu.GetUserMenus;
using AuthService.Application.Features.Menu.GetUserRoles;
using MenuItemDto = AuthService.Application.Common.Interfaces.MenuItemDto;

namespace AuthService.Tests.Unit.Api.Controllers;

public class MenuControllerTests : ControllerTestBase
{
    private readonly MenuController _controller;

    public MenuControllerTests()
    {
        _controller = new MenuController(MediatorMock.Object);
    }

    #region GetUserMenus Tests

    [Fact]
    public async Task GetUserMenus_WithAuthenticatedUser_ReturnsOkWithMenus()
    {
        // Arrange
        var userId = Guid.NewGuid();
        SetupControllerContext(_controller, userId.ToString());

        var menus = new List<MenuItemDto>
        {
            new() {
                Id = Guid.NewGuid(),
                Name = "Dashboard",
                Description = "Main dashboard",
                Icon = "dashboard-icon",
                DisplayOrder = 1,
                Level = 0,
                SubMenus = new List<MenuItemDto>(),
                Pages = new List<MenuPageItemDto>()
            }
        };

        MediatorMock.Setup(m => m.Send(It.IsAny<GetUserMenusQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(menus);

        // Act
        var result = await _controller.GetUserMenus();

        // Assert
        var response = AssertOkResult<List<MenuItemDto>>(result);
        response!.Data.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetUserMenus_WithNoMenus_ReturnsOkWithEmptyList()
    {
        // Arrange
        var userId = Guid.NewGuid();
        SetupControllerContext(_controller, userId.ToString());

        MediatorMock.Setup(m => m.Send(It.IsAny<GetUserMenusQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<MenuItemDto>());

        // Act
        var result = await _controller.GetUserMenus();

        // Assert
        var response = AssertOkResult<List<MenuItemDto>>(result);
        response!.Data.Should().BeEmpty();
    }

    [Fact]
    public async Task GetUserMenus_WithInvalidUserId_ReturnsUnauthorized()
    {
        // Arrange
        var unauthenticatedUser = CreateUnauthenticatedUser();
        SetupControllerContext(_controller, unauthenticatedUser);

        // Act
        var result = await _controller.GetUserMenus();

        // Assert
        AssertUnauthorizedResult<List<MenuItemDto>>(result);
    }

    [Fact]
    public async Task GetUserMenus_WithException_ReturnsInternalServerError()
    {
        // Arrange
        var userId = Guid.NewGuid();
        SetupControllerContext(_controller, userId.ToString());

        MediatorMock.Setup(m => m.Send(It.IsAny<GetUserMenusQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.GetUserMenus();

        // Assert
        AssertInternalServerErrorResult<List<MenuItemDto>>(result);
    }

    #endregion

    #region CheckPageAccess Tests

    [Fact]
    public async Task CheckPageAccess_WithAccess_ReturnsOkWithTrue()
    {
        // Arrange
        var userId = Guid.NewGuid();
        SetupControllerContext(_controller, userId.ToString());

        MediatorMock.Setup(m => m.Send(It.IsAny<CheckPageAccessQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.CheckPageAccess("Dashboard");

        // Assert
        var response = AssertOkResult<bool>(result);
        response!.Data.Should().BeTrue();
    }

    [Fact]
    public async Task CheckPageAccess_WithoutAccess_ReturnsOkWithFalse()
    {
        // Arrange
        var userId = Guid.NewGuid();
        SetupControllerContext(_controller, userId.ToString());

        MediatorMock.Setup(m => m.Send(It.IsAny<CheckPageAccessQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.CheckPageAccess("AdminPanel");

        // Assert
        var response = AssertOkResult<bool>(result);
        response!.Data.Should().BeFalse();
    }

    [Fact]
    public async Task CheckPageAccess_WithUnauthenticatedUser_ReturnsUnauthorized()
    {
        // Arrange
        var unauthenticatedUser = CreateUnauthenticatedUser();
        SetupControllerContext(_controller, unauthenticatedUser);

        // Act
        var result = await _controller.CheckPageAccess("Dashboard");

        // Assert
        AssertUnauthorizedResult<bool>(result);
    }

    [Fact]
    public async Task CheckPageAccess_WithException_ReturnsInternalServerError()
    {
        // Arrange
        var userId = Guid.NewGuid();
        SetupControllerContext(_controller, userId.ToString());

        MediatorMock.Setup(m => m.Send(It.IsAny<CheckPageAccessQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.CheckPageAccess("Dashboard");

        // Assert
        AssertInternalServerErrorResult<bool>(result);
    }

    #endregion

    #region CheckPermission Tests

    [Fact]
    public async Task CheckPermission_WithPermission_ReturnsOkWithTrue()
    {
        // Arrange
        var userId = Guid.NewGuid();
        SetupControllerContext(_controller, userId.ToString());

        MediatorMock.Setup(m => m.Send(It.IsAny<CheckPermissionQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.CheckPermission("View");

        // Assert
        var response = AssertOkResult<bool>(result);
        response!.Data.Should().BeTrue();
    }

    [Fact]
    public async Task CheckPermission_WithoutPermission_ReturnsOkWithFalse()
    {
        // Arrange
        var userId = Guid.NewGuid();
        SetupControllerContext(_controller, userId.ToString());

        MediatorMock.Setup(m => m.Send(It.IsAny<CheckPermissionQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.CheckPermission("Delete");

        // Assert
        var response = AssertOkResult<bool>(result);
        response!.Data.Should().BeFalse();
    }

    [Fact]
    public async Task CheckPermission_WithUnauthenticatedUser_ReturnsUnauthorized()
    {
        // Arrange
        var unauthenticatedUser = CreateUnauthenticatedUser();
        SetupControllerContext(_controller, unauthenticatedUser);

        // Act
        var result = await _controller.CheckPermission("View");

        // Assert
        AssertUnauthorizedResult<bool>(result);
    }

    [Fact]
    public async Task CheckPermission_WithException_ReturnsInternalServerError()
    {
        // Arrange
        var userId = Guid.NewGuid();
        SetupControllerContext(_controller, userId.ToString());

        MediatorMock.Setup(m => m.Send(It.IsAny<CheckPermissionQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.CheckPermission("View");

        // Assert
        AssertInternalServerErrorResult<bool>(result);
    }

    #endregion

    #region GetUserRoles Tests

    [Fact]
    public async Task GetUserRoles_WithRoles_ReturnsOkWithRoles()
    {
        // Arrange
        var userId = Guid.NewGuid();
        SetupControllerContext(_controller, userId.ToString());

        var roles = new List<string> { "Admin", "Manager" };

        MediatorMock.Setup(m => m.Send(It.IsAny<GetUserRolesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(roles);

        // Act
        var result = await _controller.GetUserRoles();

        // Assert
        var response = AssertOkResult<List<string>>(result);
        response!.Data.Should().HaveCount(2);
        response.Data.Should().Contain("Admin");
    }

    [Fact]
    public async Task GetUserRoles_WithNoRoles_ReturnsOkWithEmptyList()
    {
        // Arrange
        var userId = Guid.NewGuid();
        SetupControllerContext(_controller, userId.ToString());

        MediatorMock.Setup(m => m.Send(It.IsAny<GetUserRolesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<string>());

        // Act
        var result = await _controller.GetUserRoles();

        // Assert
        var response = AssertOkResult<List<string>>(result);
        response!.Data.Should().BeEmpty();
    }

    [Fact]
    public async Task GetUserRoles_WithUnauthenticatedUser_ReturnsUnauthorized()
    {
        // Arrange
        var unauthenticatedUser = CreateUnauthenticatedUser();
        SetupControllerContext(_controller, unauthenticatedUser);

        // Act
        var result = await _controller.GetUserRoles();

        // Assert
        AssertUnauthorizedResult<List<string>>(result);
    }

    [Fact]
    public async Task GetUserRoles_WithException_ReturnsInternalServerError()
    {
        // Arrange
        var userId = Guid.NewGuid();
        SetupControllerContext(_controller, userId.ToString());

        MediatorMock.Setup(m => m.Send(It.IsAny<GetUserRolesQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.GetUserRoles();

        // Assert
        AssertInternalServerErrorResult<List<string>>(result);
    }

    #endregion

    #region GetUserDepartment Tests

    [Fact]
    public async Task GetUserDepartment_WithDepartment_ReturnsOkWithDepartment()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var departmentId = Guid.NewGuid();
        SetupControllerContext(_controller, userId.ToString());

        MediatorMock.Setup(m => m.Send(It.IsAny<GetUserDepartmentQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(departmentId);

        // Act
        var result = await _controller.GetUserDepartment();

        // Assert
        var response = AssertOkResult<Guid?>(result);
        response!.Data.Should().Be(departmentId);
    }

    [Fact]
    public async Task GetUserDepartment_WithNoDepartment_ReturnsOkWithNull()
    {
        // Arrange
        var userId = Guid.NewGuid();
        SetupControllerContext(_controller, userId.ToString());

        MediatorMock.Setup(m => m.Send(It.IsAny<GetUserDepartmentQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid?)null);

        // Act
        var result = await _controller.GetUserDepartment();

        // Assert
        var response = AssertOkResult<Guid?>(result);
        response!.Data.Should().BeNull();
    }

    [Fact]
    public async Task GetUserDepartment_WithUnauthenticatedUser_ReturnsUnauthorized()
    {
        // Arrange
        var unauthenticatedUser = CreateUnauthenticatedUser();
        SetupControllerContext(_controller, unauthenticatedUser);

        // Act
        var result = await _controller.GetUserDepartment();

        // Assert
        AssertUnauthorizedResult<Guid?>(result);
    }

    [Fact]
    public async Task GetUserDepartment_WithException_ReturnsInternalServerError()
    {
        // Arrange
        var userId = Guid.NewGuid();
        SetupControllerContext(_controller, userId.ToString());

        MediatorMock.Setup(m => m.Send(It.IsAny<GetUserDepartmentQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.GetUserDepartment();

        // Assert
        AssertInternalServerErrorResult<Guid?>(result);
    }

    #endregion

    #region GetPagePermissions Tests

    [Fact]
    public async Task GetPagePermissions_WithPermissions_ReturnsOkWithPermissions()
    {
        // Arrange
        var userId = Guid.NewGuid();
        SetupControllerContext(_controller, userId.ToString());
        var expectedResult = new PagePermissionsDto
        {
            PageName = "Dashboard",
            Permissions = new List<string> { "View", "Edit", "Delete" }
        };

        MediatorMock.Setup(m => m.Send(It.IsAny<GetPagePermissionsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _controller.GetPagePermissions("Dashboard");

        // Assert
        var response = AssertOkResult<PagePermissionsDto>(result);
        response!.Data!.Permissions.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetPagePermissions_WithNoPermissions_ReturnsOkWithEmptyPermissions()
    {
        // Arrange
        var userId = Guid.NewGuid();
        SetupControllerContext(_controller, userId.ToString());
        var expectedResult = new PagePermissionsDto
        {
            PageName = "Dashboard",
            Permissions = new List<string>()
        };

        MediatorMock.Setup(m => m.Send(It.IsAny<GetPagePermissionsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _controller.GetPagePermissions("Dashboard");

        // Assert
        var response = AssertOkResult<PagePermissionsDto>(result);
        response!.Data!.Permissions.Should().BeEmpty();
    }

    [Fact]
    public async Task GetPagePermissions_WithUnauthenticatedUser_ReturnsUnauthorized()
    {
        // Arrange
        var unauthenticatedUser = CreateUnauthenticatedUser();
        SetupControllerContext(_controller, unauthenticatedUser);

        // Act
        var result = await _controller.GetPagePermissions("Dashboard");

        // Assert
        AssertUnauthorizedResult<PagePermissionsDto>(result);
    }

    [Fact]
    public async Task GetPagePermissions_WithException_ReturnsInternalServerError()
    {
        // Arrange
        var userId = Guid.NewGuid();
        SetupControllerContext(_controller, userId.ToString());

        MediatorMock.Setup(m => m.Send(It.IsAny<GetPagePermissionsQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.GetPagePermissions("Dashboard");

        // Assert
        AssertInternalServerErrorResult<PagePermissionsDto>(result);
    }

    #endregion
}
