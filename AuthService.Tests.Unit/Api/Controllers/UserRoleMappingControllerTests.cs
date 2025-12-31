using AuthService.Application.Features.UserRoleMapping;
using AuthService.Application.Features.UserRoleMapping.CreateUserRoleMapping;
using AuthService.Application.Features.UserRoleMapping.DeleteUserRoleMapping;
using AuthService.Application.Features.UserRoleMapping.GetAllUserRoleMappings;
using AuthService.Application.Features.UserRoleMapping.GetUserRoleMappingById;
using AuthService.Application.Features.UserRoleMapping.GetUsersWithoutRoles;
using AuthService.Application.Features.UserRoleMapping.UpdateUserRoleMapping;

namespace AuthService.Tests.Unit.Api.Controllers;

public class UserRoleMappingControllerTests : ControllerTestBase
{
    private readonly UserRoleMappingController _controller;

    public UserRoleMappingControllerTests()
    {
        _controller = new UserRoleMappingController(MediatorMock.Object);
    }

    #region Create Tests

    [Fact]
    public async Task Create_WithValidCommand_ReturnsOkWithCreatedMapping()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var roleId = Guid.NewGuid();
        var departmentId = Guid.NewGuid();
        var command = new CreateUserRoleMappingCommand
        {
            UserId = userId,
            RoleId = roleId,
            DepartmentId = departmentId,
            AssignedByEmail = "admin@example.com"
        };

        var expectedResult = new UserRoleMappingDto
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            UserEmail = "user@example.com",
            UserName = "Test User",
            RoleId = roleId,
            RoleName = "Admin",
            DepartmentId = departmentId,
            DepartmentName = "IT Department",
            AssignedByEmail = "admin@example.com",
            AssignedAt = DateTime.UtcNow,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        MediatorMock.Setup(m => m.Send(It.IsAny<CreateUserRoleMappingCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _controller.Create(command);

        // Assert
        var response = AssertOkResult<UserRoleMappingDto>(result);
        response!.Data!.UserId.Should().Be(userId);
        response.Data.RoleId.Should().Be(roleId);
        response.Data.DepartmentId.Should().Be(departmentId);
    }

    [Fact]
    public async Task Create_WithNullDepartmentId_ReturnsOkWithCreatedMapping()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var roleId = Guid.NewGuid();
        var command = new CreateUserRoleMappingCommand
        {
            UserId = userId,
            RoleId = roleId,
            DepartmentId = null,
            AssignedByEmail = "admin@example.com"
        };

        var expectedResult = new UserRoleMappingDto
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            UserEmail = "user@example.com",
            UserName = "Test User",
            RoleId = roleId,
            RoleName = "Super Admin",
            DepartmentId = null,
            DepartmentName = null,
            AssignedByEmail = "admin@example.com",
            AssignedAt = DateTime.UtcNow,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        MediatorMock.Setup(m => m.Send(It.IsAny<CreateUserRoleMappingCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _controller.Create(command);

        // Assert
        var response = AssertOkResult<UserRoleMappingDto>(result);
        response!.Data!.DepartmentId.Should().BeNull();
    }

    [Fact]
    public async Task Create_WithDuplicateMapping_ReturnsBadRequest()
    {
        // Arrange
        var command = new CreateUserRoleMappingCommand
        {
            UserId = Guid.NewGuid(),
            RoleId = Guid.NewGuid(),
            DepartmentId = Guid.NewGuid(),
            AssignedByEmail = "admin@example.com"
        };

        MediatorMock.Setup(m => m.Send(It.IsAny<CreateUserRoleMappingCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("User already has this role assigned"));

        // Act
        var result = await _controller.Create(command);

        // Assert
        AssertBadRequestResult<UserRoleMappingDto>(result);
    }

    [Fact]
    public async Task Create_WithInvalidUserId_ReturnsBadRequest()
    {
        // Arrange
        var command = new CreateUserRoleMappingCommand
        {
            UserId = Guid.NewGuid(),
            RoleId = Guid.NewGuid(),
            DepartmentId = Guid.NewGuid(),
            AssignedByEmail = "admin@example.com"
        };

        MediatorMock.Setup(m => m.Send(It.IsAny<CreateUserRoleMappingCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("User not found"));

        // Act
        var result = await _controller.Create(command);

        // Assert
        AssertBadRequestResult<UserRoleMappingDto>(result);
    }

    [Fact]
    public async Task Create_WithInvalidRoleId_ReturnsBadRequest()
    {
        // Arrange
        var command = new CreateUserRoleMappingCommand
        {
            UserId = Guid.NewGuid(),
            RoleId = Guid.NewGuid(),
            DepartmentId = Guid.NewGuid(),
            AssignedByEmail = "admin@example.com"
        };

        MediatorMock.Setup(m => m.Send(It.IsAny<CreateUserRoleMappingCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Role not found"));

        // Act
        var result = await _controller.Create(command);

        // Assert
        AssertBadRequestResult<UserRoleMappingDto>(result);
    }

    #endregion

    #region GetAll Tests

    [Fact]
    public async Task GetAll_WithMappings_ReturnsOkWithList()
    {
        // Arrange
        var mappings = new List<UserRoleMappingDto>
        {
            new() {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                UserEmail = "user1@example.com",
                UserName = "User One",
                RoleId = Guid.NewGuid(),
                RoleName = "Admin",
                DepartmentId = Guid.NewGuid(),
                DepartmentName = "IT",
                AssignedByEmail = "admin@example.com",
                AssignedAt = DateTime.UtcNow,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new() {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                UserEmail = "user2@example.com",
                UserName = "User Two",
                RoleId = Guid.NewGuid(),
                RoleName = "Manager",
                DepartmentId = Guid.NewGuid(),
                DepartmentName = "HR",
                AssignedByEmail = "admin@example.com",
                AssignedAt = DateTime.UtcNow,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            }
        };

        MediatorMock.Setup(m => m.Send(It.IsAny<GetAllUserRoleMappingsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mappings);

        // Act
        var result = await _controller.GetAll();

        // Assert
        var response = AssertOkResult<List<UserRoleMappingDto>>(result);
        response!.Data.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetAll_WithNoMappings_ReturnsOkWithEmptyList()
    {
        // Arrange
        MediatorMock.Setup(m => m.Send(It.IsAny<GetAllUserRoleMappingsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<UserRoleMappingDto>());

        // Act
        var result = await _controller.GetAll();

        // Assert
        var response = AssertOkResult<List<UserRoleMappingDto>>(result);
        response!.Data.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAll_WithException_ReturnsBadRequest()
    {
        // Arrange
        MediatorMock.Setup(m => m.Send(It.IsAny<GetAllUserRoleMappingsQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database connection failed"));

        // Act
        var result = await _controller.GetAll();

        // Assert
        AssertBadRequestResult<List<UserRoleMappingDto>>(result);
    }

    #endregion

    #region GetUsersWithoutRoles Tests

    [Fact]
    public async Task GetUsersWithoutRoles_WithUsers_ReturnsOkWithList()
    {
        // Arrange
        var usersWithoutRoles = new List<UserWithoutRoleDto>
        {
            new() {
                Id = Guid.NewGuid(),
                Email = "newuser1@example.com",
                UserName = "newuser1",
                FirstName = "New",
                LastName = "User One",
                IsActive = true
            },
            new() {
                Id = Guid.NewGuid(),
                Email = "newuser2@example.com",
                UserName = "newuser2",
                FirstName = "New",
                LastName = "User Two",
                IsActive = true
            }
        };

        MediatorMock.Setup(m => m.Send(It.IsAny<GetUsersWithoutRolesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(usersWithoutRoles);

        // Act
        var result = await _controller.GetUsersWithoutRoles();

        // Assert
        var response = AssertOkResult<List<UserWithoutRoleDto>>(result);
        response!.Data.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetUsersWithoutRoles_WithNoUsers_ReturnsOkWithEmptyList()
    {
        // Arrange
        MediatorMock.Setup(m => m.Send(It.IsAny<GetUsersWithoutRolesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<UserWithoutRoleDto>());

        // Act
        var result = await _controller.GetUsersWithoutRoles();

        // Assert
        var response = AssertOkResult<List<UserWithoutRoleDto>>(result);
        response!.Data.Should().BeEmpty();
    }

    [Fact]
    public async Task GetUsersWithoutRoles_WithException_ReturnsBadRequest()
    {
        // Arrange
        MediatorMock.Setup(m => m.Send(It.IsAny<GetUsersWithoutRolesQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.GetUsersWithoutRoles();

        // Assert
        AssertBadRequestResult<List<UserWithoutRoleDto>>(result);
    }

    #endregion

    #region GetUsersWithoutRolesImmediate Tests

    [Fact]
    public async Task GetUsersWithoutRolesImmediate_WithUsers_ReturnsOkWithList()
    {
        // Arrange
        var usersWithoutRoles = new List<UserWithoutRoleDto>
        {
            new() {
                Id = Guid.NewGuid(),
                Email = "immediate1@example.com",
                UserName = "immediate1",
                FirstName = "Immediate",
                LastName = "User",
                IsActive = true
            }
        };

        MediatorMock.Setup(m => m.Send(It.IsAny<GetUsersWithoutRolesFromCommandDbQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(usersWithoutRoles);

        // Act
        var result = await _controller.GetUsersWithoutRolesImmediate();

        // Assert
        var response = AssertOkResult<List<UserWithoutRoleDto>>(result);
        response!.Data.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetUsersWithoutRolesImmediate_WithNoUsers_ReturnsOkWithEmptyList()
    {
        // Arrange
        MediatorMock.Setup(m => m.Send(It.IsAny<GetUsersWithoutRolesFromCommandDbQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<UserWithoutRoleDto>());

        // Act
        var result = await _controller.GetUsersWithoutRolesImmediate();

        // Assert
        var response = AssertOkResult<List<UserWithoutRoleDto>>(result);
        response!.Data.Should().BeEmpty();
    }

    [Fact]
    public async Task GetUsersWithoutRolesImmediate_WithException_ReturnsBadRequest()
    {
        // Arrange
        MediatorMock.Setup(m => m.Send(It.IsAny<GetUsersWithoutRolesFromCommandDbQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.GetUsersWithoutRolesImmediate();

        // Assert
        AssertBadRequestResult<List<UserWithoutRoleDto>>(result);
    }

    #endregion

    #region GetById Tests

    [Fact]
    public async Task GetById_WithValidId_ReturnsOkWithMapping()
    {
        // Arrange
        var mappingId = Guid.NewGuid();
        var expectedMapping = new UserRoleMappingDto
        {
            Id = mappingId,
            UserId = Guid.NewGuid(),
            UserEmail = "user@example.com",
            UserName = "Test User",
            RoleId = Guid.NewGuid(),
            RoleName = "Admin",
            DepartmentId = Guid.NewGuid(),
            DepartmentName = "IT",
            AssignedByEmail = "admin@example.com",
            AssignedAt = DateTime.UtcNow,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        MediatorMock.Setup(m => m.Send(It.IsAny<GetUserRoleMappingByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedMapping);

        // Act
        var result = await _controller.GetById(mappingId);

        // Assert
        var response = AssertOkResult<UserRoleMappingDto>(result);
        response!.Data!.Id.Should().Be(mappingId);
    }

    [Fact]
    public async Task GetById_WithNonExistentId_ReturnsNotFound()
    {
        // Arrange
        var mappingId = Guid.NewGuid();

        MediatorMock.Setup(m => m.Send(It.IsAny<GetUserRoleMappingByIdQuery>(), It.IsAny<CancellationToken>()))
#pragma warning disable CS8620
            .Returns(Task.FromResult<UserRoleMappingDto?>(null));
#pragma warning restore CS8620

        // Act
        var result = await _controller.GetById(mappingId);

        // Assert
        AssertNotFoundResult<UserRoleMappingDto>(result);
    }

    [Fact]
    public async Task GetById_WithException_ReturnsBadRequest()
    {
        // Arrange
        var mappingId = Guid.NewGuid();

        MediatorMock.Setup(m => m.Send(It.IsAny<GetUserRoleMappingByIdQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.GetById(mappingId);

        // Assert
        AssertBadRequestResult<UserRoleMappingDto>(result);
    }

    #endregion

    #region Update Tests

    [Fact]
    public async Task Update_WithValidCommand_ReturnsOkWithUpdatedMapping()
    {
        // Arrange
        var mappingId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var roleId = Guid.NewGuid();
        var departmentId = Guid.NewGuid();
        var command = new UpdateUserRoleMappingCommand
        {
            Id = mappingId,
            UserId = userId,
            RoleId = roleId,
            DepartmentId = departmentId,
            AssignedByEmail = "admin@example.com"
        };

        var expectedResult = new UserRoleMappingDto
        {
            Id = mappingId,
            UserId = userId,
            UserEmail = "user@example.com",
            UserName = "Test User",
            RoleId = roleId,
            RoleName = "Manager",
            DepartmentId = departmentId,
            DepartmentName = "Sales",
            AssignedByEmail = "admin@example.com",
            AssignedAt = DateTime.UtcNow,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        MediatorMock.Setup(m => m.Send(It.IsAny<UpdateUserRoleMappingCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _controller.Update(mappingId, command);

        // Assert
        var response = AssertOkResult<UserRoleMappingDto>(result);
        response!.Data!.RoleName.Should().Be("Manager");
    }

    [Fact]
    public async Task Update_WithIdMismatch_ReturnsBadRequest()
    {
        // Arrange
        var mappingId = Guid.NewGuid();
        var differentId = Guid.NewGuid();
        var command = new UpdateUserRoleMappingCommand
        {
            Id = differentId,
            UserId = Guid.NewGuid(),
            RoleId = Guid.NewGuid(),
            DepartmentId = Guid.NewGuid(),
            AssignedByEmail = "admin@example.com"
        };

        // Act
        var result = await _controller.Update(mappingId, command);

        // Assert
        AssertBadRequestResult<UserRoleMappingDto>(result);
    }

    [Fact]
    public async Task Update_WithNonExistentMapping_ReturnsBadRequest()
    {
        // Arrange
        var mappingId = Guid.NewGuid();
        var command = new UpdateUserRoleMappingCommand
        {
            Id = mappingId,
            UserId = Guid.NewGuid(),
            RoleId = Guid.NewGuid(),
            DepartmentId = Guid.NewGuid(),
            AssignedByEmail = "admin@example.com"
        };

        MediatorMock.Setup(m => m.Send(It.IsAny<UpdateUserRoleMappingCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("User role mapping not found"));

        // Act
        var result = await _controller.Update(mappingId, command);

        // Assert
        AssertBadRequestResult<UserRoleMappingDto>(result);
    }

    [Fact]
    public async Task Update_WithInvalidRoleChange_ReturnsBadRequest()
    {
        // Arrange
        var mappingId = Guid.NewGuid();
        var command = new UpdateUserRoleMappingCommand
        {
            Id = mappingId,
            UserId = Guid.NewGuid(),
            RoleId = Guid.NewGuid(),
            DepartmentId = Guid.NewGuid(),
            AssignedByEmail = "admin@example.com"
        };

        MediatorMock.Setup(m => m.Send(It.IsAny<UpdateUserRoleMappingCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Cannot change to specified role - role not found"));

        // Act
        var result = await _controller.Update(mappingId, command);

        // Assert
        AssertBadRequestResult<UserRoleMappingDto>(result);
    }

    #endregion

    #region Delete Tests

    [Fact]
    public async Task Delete_WithValidId_ReturnsOk()
    {
        // Arrange
        var mappingId = Guid.NewGuid();

        MediatorMock.Setup(m => m.Send(It.IsAny<DeleteUserRoleMappingCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.Delete(mappingId);

        // Assert
        AssertOkResult<bool>(result);
    }

    [Fact]
    public async Task Delete_WithNonExistentId_ReturnsBadRequest()
    {
        // Arrange
        var mappingId = Guid.NewGuid();

        MediatorMock.Setup(m => m.Send(It.IsAny<DeleteUserRoleMappingCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("User role mapping not found"));

        // Act
        var result = await _controller.Delete(mappingId);

        // Assert
        AssertBadRequestResult<bool>(result);
    }

    [Fact]
    public async Task Delete_WithLastAdminRole_ReturnsBadRequest()
    {
        // Arrange
        var mappingId = Guid.NewGuid();

        MediatorMock.Setup(m => m.Send(It.IsAny<DeleteUserRoleMappingCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Cannot delete the last admin role mapping"));

        // Act
        var result = await _controller.Delete(mappingId);

        // Assert
        AssertBadRequestResult<bool>(result);
    }

    [Fact]
    public async Task Delete_WithException_ReturnsBadRequest()
    {
        // Arrange
        var mappingId = Guid.NewGuid();

        MediatorMock.Setup(m => m.Send(It.IsAny<DeleteUserRoleMappingCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.Delete(mappingId);

        // Assert
        AssertBadRequestResult<bool>(result);
    }

    #endregion
}
