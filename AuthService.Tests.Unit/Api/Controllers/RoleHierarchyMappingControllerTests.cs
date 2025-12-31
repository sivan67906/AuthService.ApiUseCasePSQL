using AuthService.Application.Features.RoleHierarchyMapping;
using AuthService.Application.Features.RoleHierarchyMapping.CreateRoleHierarchyMapping;
using AuthService.Application.Features.RoleHierarchyMapping.DeleteRoleHierarchyMapping;
using AuthService.Application.Features.RoleHierarchyMapping.GetAllRoleHierarchyMappings;
using AuthService.Application.Features.RoleHierarchyMapping.GetRoleHierarchyMappingById;
using AuthService.Application.Features.RoleHierarchyMapping.UpdateRoleHierarchyMapping;

namespace AuthService.Tests.Unit.Api.Controllers;

public class RoleHierarchyMappingControllerTests : ControllerTestBase
{
    private readonly RoleHierarchyMappingController _controller;

    public RoleHierarchyMappingControllerTests()
    {
        _controller = new RoleHierarchyMappingController(MediatorMock.Object);
    }

    #region Create Tests

    [Fact]
    public async Task Create_WithValidCommand_ReturnsOkWithCreatedMapping()
    {
        // Arrange
        var parentRoleId = Guid.NewGuid();
        var childRoleId = Guid.NewGuid();
        var departmentId = Guid.NewGuid();
        var command = new CreateRoleHierarchyMappingCommand
        {
            ParentRoleId = parentRoleId,
            ChildRoleId = childRoleId,
            Level = 1
        };

        var expectedResult = new RoleHierarchyMappingDto
        {
            Id = Guid.NewGuid(),
            DepartmentId = departmentId,
            DepartmentName = "IT Department",
            ParentRoleId = parentRoleId,
            ParentRoleName = "Manager",
            ParentDepartmentId = departmentId,
            ParentDepartmentName = "IT Department",
            ChildRoleId = childRoleId,
            ChildRoleName = "Developer",
            ChildDepartmentId = departmentId,
            ChildDepartmentName = "IT Department",
            Level = 1,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        MediatorMock.Setup(m => m.Send(It.IsAny<CreateRoleHierarchyMappingCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _controller.Create(command);

        // Assert
        var response = AssertOkResult<RoleHierarchyMappingDto>(result);
        response!.Data!.ParentRoleId.Should().Be(parentRoleId);
        response.Data.ChildRoleId.Should().Be(childRoleId);
        response.Data.Level.Should().Be(1);
    }

    [Fact]
    public async Task Create_WithDifferentDepartmentRoles_ReturnsOkWithCreatedMapping()
    {
        // Arrange
        var departmentId = Guid.NewGuid();
        var parentDepartmentId = Guid.NewGuid();
        var parentRoleId = Guid.NewGuid();
        var childRoleId = Guid.NewGuid();
        var command = new CreateRoleHierarchyMappingCommand
        {
            ParentRoleId = parentRoleId,
            ChildRoleId = childRoleId,
            Level = 2
        };

        var expectedResult = new RoleHierarchyMappingDto
        {
            Id = Guid.NewGuid(),
            DepartmentId = departmentId,
            DepartmentName = "Sales",
            ParentRoleId = parentRoleId,
            ParentRoleName = "Director",
            ParentDepartmentId = parentDepartmentId,
            ParentDepartmentName = "Corporate",
            ChildRoleId = childRoleId,
            ChildRoleName = "Manager",
            ChildDepartmentId = departmentId,
            ChildDepartmentName = "Sales",
            Level = 2,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        MediatorMock.Setup(m => m.Send(It.IsAny<CreateRoleHierarchyMappingCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _controller.Create(command);

        // Assert
        var response = AssertOkResult<RoleHierarchyMappingDto>(result);
        response!.Data!.Level.Should().Be(2);
    }

    [Fact]
    public async Task Create_WithCircularHierarchy_ReturnsBadRequest()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        var command = new CreateRoleHierarchyMappingCommand
        {
            ParentRoleId = roleId,
            ChildRoleId = roleId, // Same role as parent and child
            Level = 1
        };

        MediatorMock.Setup(m => m.Send(It.IsAny<CreateRoleHierarchyMappingCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Circular hierarchy detected"));

        // Act
        var result = await _controller.Create(command);

        // Assert
        AssertBadRequestResult<RoleHierarchyMappingDto>(result);
    }

    [Fact]
    public async Task Create_WithDuplicateMapping_ReturnsBadRequest()
    {
        // Arrange
        var command = new CreateRoleHierarchyMappingCommand
        {
            ParentRoleId = Guid.NewGuid(),
            ChildRoleId = Guid.NewGuid(),
            Level = 1
        };

        MediatorMock.Setup(m => m.Send(It.IsAny<CreateRoleHierarchyMappingCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Hierarchy mapping already exists"));

        // Act
        var result = await _controller.Create(command);

        // Assert
        AssertBadRequestResult<RoleHierarchyMappingDto>(result);
    }

    [Fact]
    public async Task Create_WithInvalidRoleId_ReturnsBadRequest()
    {
        // Arrange
        var command = new CreateRoleHierarchyMappingCommand
        {
            ParentRoleId = Guid.NewGuid(),
            ChildRoleId = Guid.NewGuid(),
            Level = 1
        };

        MediatorMock.Setup(m => m.Send(It.IsAny<CreateRoleHierarchyMappingCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Role not found"));

        // Act
        var result = await _controller.Create(command);

        // Assert
        AssertBadRequestResult<RoleHierarchyMappingDto>(result);
    }

    #endregion

    #region GetAll Tests

    [Fact]
    public async Task GetAll_WithMappings_ReturnsOkWithList()
    {
        // Arrange
        var mappings = new List<RoleHierarchyMappingDto>
        {
            new() {
                Id = Guid.NewGuid(),
                DepartmentId = Guid.NewGuid(),
                DepartmentName = "IT",
                ParentRoleId = Guid.NewGuid(),
                ParentRoleName = "Manager",
                ChildRoleId = Guid.NewGuid(),
                ChildRoleName = "Developer",
                Level = 1,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new() {
                Id = Guid.NewGuid(),
                DepartmentId = Guid.NewGuid(),
                DepartmentName = "HR",
                ParentRoleId = Guid.NewGuid(),
                ParentRoleName = "HR Director",
                ChildRoleId = Guid.NewGuid(),
                ChildRoleName = "HR Manager",
                Level = 1,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            }
        };

        MediatorMock.Setup(m => m.Send(It.IsAny<GetAllRoleHierarchyMappingsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mappings);

        // Act
        var result = await _controller.GetAll();

        // Assert
        var response = AssertOkResult<List<RoleHierarchyMappingDto>>(result);
        response!.Data.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetAll_WithNoMappings_ReturnsOkWithEmptyList()
    {
        // Arrange
        MediatorMock.Setup(m => m.Send(It.IsAny<GetAllRoleHierarchyMappingsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<RoleHierarchyMappingDto>());

        // Act
        var result = await _controller.GetAll();

        // Assert
        var response = AssertOkResult<List<RoleHierarchyMappingDto>>(result);
        response!.Data.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAll_WithException_ReturnsBadRequest()
    {
        // Arrange
        MediatorMock.Setup(m => m.Send(It.IsAny<GetAllRoleHierarchyMappingsQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database connection failed"));

        // Act
        var result = await _controller.GetAll();

        // Assert
        AssertBadRequestResult<List<RoleHierarchyMappingDto>>(result);
    }

    #endregion

    #region GetById Tests

    [Fact]
    public async Task GetById_WithValidId_ReturnsOkWithMapping()
    {
        // Arrange
        var mappingId = Guid.NewGuid();
        var expectedMapping = new RoleHierarchyMappingDto
        {
            Id = mappingId,
            DepartmentId = Guid.NewGuid(),
            DepartmentName = "IT",
            ParentRoleId = Guid.NewGuid(),
            ParentRoleName = "Manager",
            ChildRoleId = Guid.NewGuid(),
            ChildRoleName = "Developer",
            Level = 1,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        MediatorMock.Setup(m => m.Send(It.IsAny<GetRoleHierarchyMappingByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedMapping);

        // Act
        var result = await _controller.GetById(mappingId);

        // Assert
        var response = AssertOkResult<RoleHierarchyMappingDto>(result);
        response!.Data!.Id.Should().Be(mappingId);
    }

    [Fact]
    public async Task GetById_WithNonExistentId_ReturnsNotFound()
    {
        // Arrange
        var mappingId = Guid.NewGuid();

        MediatorMock.Setup(m => m.Send(It.IsAny<GetRoleHierarchyMappingByIdQuery>(), It.IsAny<CancellationToken>()))
#pragma warning disable CS8620
            .Returns(Task.FromResult<RoleHierarchyMappingDto?>(null));
#pragma warning restore CS8620

        // Act
        var result = await _controller.GetById(mappingId);

        // Assert
        AssertNotFoundResult<RoleHierarchyMappingDto>(result);
    }

    [Fact]
    public async Task GetById_WithException_ReturnsBadRequest()
    {
        // Arrange
        var mappingId = Guid.NewGuid();

        MediatorMock.Setup(m => m.Send(It.IsAny<GetRoleHierarchyMappingByIdQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.GetById(mappingId);

        // Assert
        AssertBadRequestResult<RoleHierarchyMappingDto>(result);
    }

    #endregion

    #region Update Tests

    [Fact]
    public async Task Update_WithValidCommand_ReturnsOkWithUpdatedMapping()
    {
        // Arrange
        var mappingId = Guid.NewGuid();
        var parentRoleId = Guid.NewGuid();
        var childRoleId = Guid.NewGuid();
        var command = new UpdateRoleHierarchyMappingCommand
        {
            Id = mappingId,
            ParentRoleId = parentRoleId,
            ChildRoleId = childRoleId,
            Level = 2
        };

        var expectedResult = new RoleHierarchyMappingDto
        {
            Id = mappingId,
            DepartmentId = Guid.NewGuid(),
            DepartmentName = "IT",
            ParentRoleId = parentRoleId,
            ParentRoleName = "Senior Manager",
            ChildRoleId = childRoleId,
            ChildRoleName = "Manager",
            Level = 2,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        MediatorMock.Setup(m => m.Send(It.IsAny<UpdateRoleHierarchyMappingCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _controller.Update(mappingId, command);

        // Assert
        var response = AssertOkResult<RoleHierarchyMappingDto>(result);
        response!.Data!.Level.Should().Be(2);
    }

    [Fact]
    public async Task Update_WithIdMismatch_ReturnsBadRequest()
    {
        // Arrange
        var mappingId = Guid.NewGuid();
        var differentId = Guid.NewGuid();
        var command = new UpdateRoleHierarchyMappingCommand
        {
            Id = differentId,
            ParentRoleId = Guid.NewGuid(),
            ChildRoleId = Guid.NewGuid(),
            Level = 1
        };

        // Act
        var result = await _controller.Update(mappingId, command);

        // Assert
        AssertBadRequestResult<RoleHierarchyMappingDto>(result);
    }

    [Fact]
    public async Task Update_WithNonExistentMapping_ReturnsBadRequest()
    {
        // Arrange
        var mappingId = Guid.NewGuid();
        var command = new UpdateRoleHierarchyMappingCommand
        {
            Id = mappingId,
            ParentRoleId = Guid.NewGuid(),
            ChildRoleId = Guid.NewGuid(),
            Level = 1
        };

        MediatorMock.Setup(m => m.Send(It.IsAny<UpdateRoleHierarchyMappingCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Role hierarchy mapping not found"));

        // Act
        var result = await _controller.Update(mappingId, command);

        // Assert
        AssertBadRequestResult<RoleHierarchyMappingDto>(result);
    }

    [Fact]
    public async Task Update_WithCircularHierarchy_ReturnsBadRequest()
    {
        // Arrange
        var mappingId = Guid.NewGuid();
        var roleId = Guid.NewGuid();
        var command = new UpdateRoleHierarchyMappingCommand
        {
            Id = mappingId,
            ParentRoleId = roleId,
            ChildRoleId = roleId,
            Level = 1
        };

        MediatorMock.Setup(m => m.Send(It.IsAny<UpdateRoleHierarchyMappingCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Update would create circular hierarchy"));

        // Act
        var result = await _controller.Update(mappingId, command);

        // Assert
        AssertBadRequestResult<RoleHierarchyMappingDto>(result);
    }

    #endregion

    #region Delete Tests

    [Fact]
    public async Task Delete_WithValidId_ReturnsOk()
    {
        // Arrange
        var mappingId = Guid.NewGuid();

        MediatorMock.Setup(m => m.Send(It.IsAny<DeleteRoleHierarchyMappingCommand>(), It.IsAny<CancellationToken>()))
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

        MediatorMock.Setup(m => m.Send(It.IsAny<DeleteRoleHierarchyMappingCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Role hierarchy mapping not found"));

        // Act
        var result = await _controller.Delete(mappingId);

        // Assert
        AssertBadRequestResult<bool>(result);
    }

    [Fact]
    public async Task Delete_WithDependentMappings_ReturnsBadRequest()
    {
        // Arrange
        var mappingId = Guid.NewGuid();

        MediatorMock.Setup(m => m.Send(It.IsAny<DeleteRoleHierarchyMappingCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Cannot delete hierarchy mapping with dependent child mappings"));

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

        MediatorMock.Setup(m => m.Send(It.IsAny<DeleteRoleHierarchyMappingCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.Delete(mappingId);

        // Assert
        AssertBadRequestResult<bool>(result);
    }

    #endregion
}
