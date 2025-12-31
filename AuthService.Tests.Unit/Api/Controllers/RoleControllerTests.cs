using AuthService.Application.Features.Role.CreateRole;
using AuthService.Application.Features.Role.DeleteRole;
using AuthService.Application.Features.Role.GetAllRoles;
using AuthService.Application.Features.Role.GetRoleById;
using AuthService.Application.Features.Role.GetRolesByDepartment;
using AuthService.Application.Features.Role.UpdateRole;

namespace AuthService.Tests.Unit.Api.Controllers;

public class RoleControllerTests : ControllerTestBase
{
    private readonly RoleController _controller;

    public RoleControllerTests()
    {
        _controller = new RoleController(MediatorMock.Object);
    }

    #region Create Tests

    [Fact]
    public async Task Create_WithValidCommand_ReturnsOkWithCreatedRole()
    {
        // Arrange
        var command = new CreateRoleCommand("ADMIN", "Administrator", "Admin role with full access", null);
        var expectedResult = new RoleDto(
            Guid.NewGuid(),
            "ADMIN",
            "Administrator",
            "Admin role with full access",
            null,
            null
        );

        MediatorMock.Setup(m => m.Send(It.IsAny<CreateRoleCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _controller.Create(command);

        // Assert
        var response = AssertOkResult<RoleDto>(result);
        response!.Data!.Code.Should().Be("ADMIN");
        response.Data.Name.Should().Be("Administrator");
    }

    [Fact]
    public async Task Create_WithDepartmentId_ReturnsOkWithCreatedRole()
    {
        // Arrange
        var departmentId = Guid.NewGuid();
        var command = new CreateRoleCommand("DEP_ADMIN", "Department Admin", "Department specific admin", departmentId);
        var expectedResult = new RoleDto(
            Guid.NewGuid(),
            "DEP_ADMIN",
            "Department Admin",
            "Department specific admin",
            departmentId,
            "IT Department"
        );

        MediatorMock.Setup(m => m.Send(It.IsAny<CreateRoleCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _controller.Create(command);

        // Assert
        var response = AssertOkResult<RoleDto>(result);
        response!.Data!.DepartmentId.Should().Be(departmentId);
        response.Data.DepartmentName.Should().Be("IT Department");
    }

    [Fact]
    public async Task Create_WithDuplicateName_ReturnsBadRequest()
    {
        // Arrange
        var command = new CreateRoleCommand("ADMIN", "Administrator", "Description", null);

        MediatorMock.Setup(m => m.Send(It.IsAny<CreateRoleCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Role with name 'Administrator' already exists"));

        // Act
        var result = await _controller.Create(command);

        // Assert
        AssertBadRequestResult<RoleDto>(result);
    }

    [Fact]
    public async Task Create_WithEmptyName_ReturnsBadRequest()
    {
        // Arrange
        var command = new CreateRoleCommand("ADMIN", "", "Description", null);

        MediatorMock.Setup(m => m.Send(It.IsAny<CreateRoleCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Role name is required"));

        // Act
        var result = await _controller.Create(command);

        // Assert
        AssertBadRequestResult<RoleDto>(result);
    }

    #endregion

    #region GetAll Tests

    [Fact]
    public async Task GetAll_WithRoles_ReturnsOkWithList()
    {
        // Arrange
        var roles = new List<RoleDto>
        {
            new(Guid.NewGuid(), "ADMIN", "Administrator", "Admin role", null, null),
            new(Guid.NewGuid(), "USER", "User", "Standard user", null, null),
            new(Guid.NewGuid(), "MANAGER", "Manager", "Manager role", null, null)
        };

        MediatorMock.Setup(m => m.Send(It.IsAny<GetAllRolesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(roles);

        // Act
        var result = await _controller.GetAll();

        // Assert
        var response = AssertOkResult<List<RoleDto>>(result);
        response!.Data.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetAll_WithNoRoles_ReturnsOkWithEmptyList()
    {
        // Arrange
        MediatorMock.Setup(m => m.Send(It.IsAny<GetAllRolesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<RoleDto>());

        // Act
        var result = await _controller.GetAll();

        // Assert
        var response = AssertOkResult<List<RoleDto>>(result);
        response!.Data.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAll_WithException_ReturnsBadRequest()
    {
        // Arrange
        MediatorMock.Setup(m => m.Send(It.IsAny<GetAllRolesQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.GetAll();

        // Assert
        AssertBadRequestResult<List<RoleDto>>(result);
    }

    #endregion

    #region GetByDepartment Tests

    [Fact]
    public async Task GetByDepartment_WithValidDepartmentId_ReturnsOkWithRoles()
    {
        // Arrange
        var departmentId = Guid.NewGuid();
        var roles = new List<RoleDto>
        {
            new(Guid.NewGuid(), "DEP_ADMIN", "Department Admin", "Admin role", departmentId, "IT"),
            new(Guid.NewGuid(), "DEP_USER", "Department User", "User role", departmentId, "IT")
        };

        MediatorMock.Setup(m => m.Send(It.IsAny<GetRolesByDepartmentQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(roles);

        // Act
        var result = await _controller.GetByDepartment(departmentId);

        // Assert
        var response = AssertOkResult<List<RoleDto>>(result);
        response!.Data.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetByDepartment_WithNullDepartmentId_ReturnsAllSystemRoles()
    {
        // Arrange
        var roles = new List<RoleDto>
        {
            new(Guid.NewGuid(), "ADMIN", "Administrator", "Admin role", null, null),
            new(Guid.NewGuid(), "USER", "User", "User role", null, null)
        };

        MediatorMock.Setup(m => m.Send(It.IsAny<GetRolesByDepartmentQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(roles);

        // Act
        var result = await _controller.GetByDepartment(null);

        // Assert
        var response = AssertOkResult<List<RoleDto>>(result);
        response!.Data.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetByDepartment_WithException_ReturnsBadRequest()
    {
        // Arrange
        MediatorMock.Setup(m => m.Send(It.IsAny<GetRolesByDepartmentQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.GetByDepartment(Guid.NewGuid());

        // Assert
        AssertBadRequestResult<List<RoleDto>>(result);
    }

    #endregion

    #region GetById Tests

    [Fact]
    public async Task GetById_WithValidId_ReturnsOkWithRole()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        var expectedResult = new RoleDto(roleId, "ADMIN", "Administrator", "Admin role", null, null);

        MediatorMock.Setup(m => m.Send(It.IsAny<GetRoleByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _controller.GetById(roleId);

        // Assert
        var response = AssertOkResult<RoleDto>(result);
        response!.Data!.Id.Should().Be(roleId);
    }

    [Fact]
    public async Task GetById_WithNonExistentId_ReturnsNotFound()
    {
        // Arrange
        var roleId = Guid.NewGuid();

#pragma warning disable CS8620
        MediatorMock.Setup(m => m.Send(It.IsAny<GetRoleByIdQuery>(), It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult<RoleDto?>(null));
#pragma warning restore CS8620

        // Act
        var result = await _controller.GetById(roleId);

        // Assert
        AssertNotFoundResult<RoleDto>(result);
    }

    [Fact]
    public async Task GetById_WithException_ReturnsBadRequest()
    {
        // Arrange
        var roleId = Guid.NewGuid();

        MediatorMock.Setup(m => m.Send(It.IsAny<GetRoleByIdQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.GetById(roleId);

        // Assert
        AssertBadRequestResult<RoleDto>(result);
    }

    #endregion

    #region Update Tests

    [Fact]
    public async Task Update_WithValidCommand_ReturnsOkWithUpdatedRole()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        var command = new UpdateRoleCommand(roleId, "Updated Admin", "Updated description", null);
        var expectedResult = new RoleDto(roleId, "ADMIN", "Updated Admin", "Updated description", null, null);

        MediatorMock.Setup(m => m.Send(It.IsAny<UpdateRoleCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _controller.Update(roleId, command);

        // Assert
        var response = AssertOkResult<RoleDto>(result);
        response!.Data!.Name.Should().Be("Updated Admin");
    }

    [Fact]
    public async Task Update_WithIdMismatch_ReturnsBadRequest()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        var differentId = Guid.NewGuid();
        var command = new UpdateRoleCommand(differentId, "Updated Admin", "Updated description", null);

        // Act
        var result = await _controller.Update(roleId, command);

        // Assert
        AssertBadRequestResult<RoleDto>(result);
    }

    [Fact]
    public async Task Update_WithNonExistentRole_ReturnsBadRequest()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        var command = new UpdateRoleCommand(roleId, "Updated Admin", "Updated description", null);

        MediatorMock.Setup(m => m.Send(It.IsAny<UpdateRoleCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Role not found"));

        // Act
        var result = await _controller.Update(roleId, command);

        // Assert
        AssertBadRequestResult<RoleDto>(result);
    }

    #endregion

    #region Delete Tests

    [Fact]
    public async Task Delete_WithValidId_ReturnsOk()
    {
        // Arrange
        var roleId = Guid.NewGuid();

        MediatorMock.Setup(m => m.Send(It.IsAny<DeleteRoleCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.Delete(roleId);

        // Assert
        AssertOkResult<bool>(result);
    }

    [Fact]
    public async Task Delete_WithNonExistentId_ReturnsBadRequest()
    {
        // Arrange
        var roleId = Guid.NewGuid();

        MediatorMock.Setup(m => m.Send(It.IsAny<DeleteRoleCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Role not found"));

        // Act
        var result = await _controller.Delete(roleId);

        // Assert
        AssertBadRequestResult<bool>(result);
    }

    [Fact]
    public async Task Delete_WithRoleHavingUsers_ReturnsBadRequest()
    {
        // Arrange
        var roleId = Guid.NewGuid();

        MediatorMock.Setup(m => m.Send(It.IsAny<DeleteRoleCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Cannot delete role with assigned users"));

        // Act
        var result = await _controller.Delete(roleId);

        // Assert
        AssertBadRequestResult<bool>(result);
    }

    #endregion
}
