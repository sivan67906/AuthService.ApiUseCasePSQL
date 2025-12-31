using AuthService.Application.Features.Permission.CreatePermission;
using AuthService.Application.Features.Permission.DeletePermission;
using AuthService.Application.Features.Permission.GetAllPermissions;
using AuthService.Application.Features.Permission.GetPermission;
using AuthService.Application.Features.Permission.UpdatePermission;

namespace AuthService.Tests.Unit.Api.Controllers;

public class PermissionControllerTests : ControllerTestBase
{
    private readonly PermissionController _controller;

    public PermissionControllerTests()
    {
        _controller = new PermissionController(MediatorMock.Object);
    }

    #region Create Tests

    [Fact]
    public async Task Create_WithValidCommand_ReturnsOkWithCreatedPermission()
    {
        // Arrange
        var command = new CreatePermissionCommand("VIEW", "View Permission", "Allows viewing data");
        var expectedResult = new PermissionDto(
            Guid.NewGuid(),
            "VIEW",
            "View Permission",
            "Allows viewing data",
            true,
            DateTime.UtcNow,
            null
        );

        MediatorMock.Setup(m => m.Send(It.IsAny<CreatePermissionCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _controller.Create(command);

        // Assert
        var response = AssertOkResult<PermissionDto>(result);
        response!.Data!.Code.Should().Be("VIEW");
        response.Data.Name.Should().Be("View Permission");
    }

    [Fact]
    public async Task Create_WithDuplicateName_ReturnsBadRequest()
    {
        // Arrange
        var command = new CreatePermissionCommand("VIEW", "View Permission", null);

        MediatorMock.Setup(m => m.Send(It.IsAny<CreatePermissionCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Permission with name 'View Permission' already exists"));

        // Act
        var result = await _controller.Create(command);

        // Assert
        AssertBadRequestResult<PermissionDto>(result);
    }

    [Fact]
    public async Task Create_WithDuplicateCode_ReturnsBadRequest()
    {
        // Arrange
        var command = new CreatePermissionCommand("VIEW", "New Permission", null);

        MediatorMock.Setup(m => m.Send(It.IsAny<CreatePermissionCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Permission with code 'VIEW' already exists"));

        // Act
        var result = await _controller.Create(command);

        // Assert
        AssertBadRequestResult<PermissionDto>(result);
    }

    [Fact]
    public async Task Create_WithEmptyName_ReturnsBadRequest()
    {
        // Arrange
        var command = new CreatePermissionCommand("VIEW", "", null);

        MediatorMock.Setup(m => m.Send(It.IsAny<CreatePermissionCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Permission name is required"));

        // Act
        var result = await _controller.Create(command);

        // Assert
        AssertBadRequestResult<PermissionDto>(result);
    }

    [Fact]
    public async Task Create_WithEmptyCode_ReturnsBadRequest()
    {
        // Arrange
        var command = new CreatePermissionCommand("", "View Permission", null);

        MediatorMock.Setup(m => m.Send(It.IsAny<CreatePermissionCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Permission code is required"));

        // Act
        var result = await _controller.Create(command);

        // Assert
        AssertBadRequestResult<PermissionDto>(result);
    }

    #endregion

    #region Update Tests

    [Fact]
    public async Task Update_WithValidCommand_ReturnsOkWithUpdatedPermission()
    {
        // Arrange
        var permissionId = Guid.NewGuid();
        var command = new UpdatePermissionCommand(permissionId, "Updated View", "Updated description");
        var expectedResult = new PermissionDto(
            permissionId,
            "VIEW",
            "Updated View",
            "Updated description",
            true,
            DateTime.UtcNow.AddDays(-1),
            DateTime.UtcNow
        );

        MediatorMock.Setup(m => m.Send(It.IsAny<UpdatePermissionCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _controller.Update(permissionId, command);

        // Assert
        var response = AssertOkResult<PermissionDto>(result);
        response!.Data!.Name.Should().Be("Updated View");
    }

    [Fact]
    public async Task Update_WithIdMismatch_ReturnsBadRequest()
    {
        // Arrange
        var permissionId = Guid.NewGuid();
        var differentId = Guid.NewGuid();
        var command = new UpdatePermissionCommand(differentId, "Updated View", null);

        // Act
        var result = await _controller.Update(permissionId, command);

        // Assert
        AssertBadRequestResult<PermissionDto>(result);
    }

    [Fact]
    public async Task Update_WithNonExistentPermission_ReturnsBadRequest()
    {
        // Arrange
        var permissionId = Guid.NewGuid();
        var command = new UpdatePermissionCommand(permissionId, "Updated View", null);

        MediatorMock.Setup(m => m.Send(It.IsAny<UpdatePermissionCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Permission not found"));

        // Act
        var result = await _controller.Update(permissionId, command);

        // Assert
        AssertBadRequestResult<PermissionDto>(result);
    }

    #endregion

    #region Delete Tests

    [Fact]
    public async Task Delete_WithValidId_ReturnsOk()
    {
        // Arrange
        var permissionId = Guid.NewGuid();

        MediatorMock.Setup(m => m.Send(It.IsAny<DeletePermissionCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.Delete(permissionId);

        // Assert
        AssertOkResult<bool>(result);
    }

    [Fact]
    public async Task Delete_WithNonExistentId_ReturnsBadRequest()
    {
        // Arrange
        var permissionId = Guid.NewGuid();

        MediatorMock.Setup(m => m.Send(It.IsAny<DeletePermissionCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Permission not found"));

        // Act
        var result = await _controller.Delete(permissionId);

        // Assert
        AssertBadRequestResult<bool>(result);
    }

    [Fact]
    public async Task Delete_WithPermissionInUse_ReturnsBadRequest()
    {
        // Arrange
        var permissionId = Guid.NewGuid();

        MediatorMock.Setup(m => m.Send(It.IsAny<DeletePermissionCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Cannot delete permission in use"));

        // Act
        var result = await _controller.Delete(permissionId);

        // Assert
        AssertBadRequestResult<bool>(result);
    }

    #endregion

    #region Get Tests

    [Fact]
    public async Task Get_WithValidId_ReturnsOkWithPermission()
    {
        // Arrange
        var permissionId = Guid.NewGuid();
        var expectedResult = new PermissionDto(
            permissionId,
            "VIEW",
            "View Permission",
            "Allows viewing",
            true,
            DateTime.UtcNow,
            null
        );

        MediatorMock.Setup(m => m.Send(It.IsAny<GetPermissionQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _controller.Get(permissionId);

        // Assert
        var response = AssertOkResult<PermissionDto>(result);
        response!.Data!.Id.Should().Be(permissionId);
    }

    [Fact]
    public async Task Get_WithNonExistentId_ReturnsNotFound()
    {
        // Arrange
        var permissionId = Guid.NewGuid();

        MediatorMock.Setup(m => m.Send(It.IsAny<GetPermissionQuery>(), It.IsAny<CancellationToken>()))
#pragma warning disable CS8620
            .Returns(Task.FromResult<PermissionDto?>(null));
#pragma warning restore CS8620

        // Act
        var result = await _controller.Get(permissionId);

        // Assert
        AssertNotFoundResult<PermissionDto>(result);
    }

    [Fact]
    public async Task Get_WithException_ReturnsBadRequest()
    {
        // Arrange
        var permissionId = Guid.NewGuid();

        MediatorMock.Setup(m => m.Send(It.IsAny<GetPermissionQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.Get(permissionId);

        // Assert
        AssertBadRequestResult<PermissionDto>(result);
    }

    #endregion

    #region GetAll Tests

    [Fact]
    public async Task GetAll_WithPermissions_ReturnsOkWithList()
    {
        // Arrange
        var permissions = new List<PermissionDto>
        {
            new(Guid.NewGuid(), "VIEW", "View", null, true, DateTime.UtcNow, null),
            new(Guid.NewGuid(), "CREATE", "Create", null, true, DateTime.UtcNow, null),
            new(Guid.NewGuid(), "UPDATE", "Update", null, true, DateTime.UtcNow, null),
            new(Guid.NewGuid(), "DELETE", "Delete", null, true, DateTime.UtcNow, null)
        };

        MediatorMock.Setup(m => m.Send(It.IsAny<GetAllPermissionsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(permissions);

        // Act
        var result = await _controller.GetAll();

        // Assert
        var response = AssertOkResult<List<PermissionDto>>(result);
        response!.Data.Should().HaveCount(4);
    }

    [Fact]
    public async Task GetAll_WithNoPermissions_ReturnsOkWithEmptyList()
    {
        // Arrange
        MediatorMock.Setup(m => m.Send(It.IsAny<GetAllPermissionsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<PermissionDto>());

        // Act
        var result = await _controller.GetAll();

        // Assert
        var response = AssertOkResult<List<PermissionDto>>(result);
        response!.Data.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAll_WithException_ReturnsBadRequest()
    {
        // Arrange
        MediatorMock.Setup(m => m.Send(It.IsAny<GetAllPermissionsQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.GetAll();

        // Assert
        AssertBadRequestResult<List<PermissionDto>>(result);
    }

    #endregion
}
