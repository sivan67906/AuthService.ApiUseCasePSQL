using AuthService.Application.DTOs;
using AuthService.Application.Features.RoleFeatureMapping.CreateRoleFeatureMapping;
using AuthService.Application.Features.RoleFeatureMapping.DeleteRoleFeatureMapping;
using AuthService.Application.Features.RoleFeatureMapping.GetAllRoleFeatureMappings;
using AuthService.Application.Features.RoleFeatureMapping.GetRoleFeatureMappingById;
using AuthService.Application.Features.RoleFeatureMapping.GetRoleFeatureMappingsByDepartment;
using AuthService.Application.Features.RoleFeatureMapping.GetRoleFeatureMappingsByRole;
using AuthService.Application.Features.RoleFeatureMapping.UpdateRoleFeatureMapping;

namespace AuthService.Tests.Unit.Api.Controllers;

public class RoleFeatureMappingControllerTests : ControllerTestBase
{
    private readonly RoleFeatureMappingController _controller;

    public RoleFeatureMappingControllerTests()
    {
        _controller = new RoleFeatureMappingController(MediatorMock.Object);
    }

    #region GetAll Tests

    [Fact]
    public async Task GetAll_WithMappings_ReturnsOkWithList()
    {
        // Arrange
        var mappings = new List<RoleFeatureMappingDto>
        {
            new() {
                Id = Guid.NewGuid(),
                RoleId = Guid.NewGuid(),
                RoleName = "Admin",
                FeatureId = Guid.NewGuid(),
                FeatureName = "Dashboard",
                DepartmentId = null,
                DepartmentName = null,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new() {
                Id = Guid.NewGuid(),
                RoleId = Guid.NewGuid(),
                RoleName = "Manager",
                FeatureId = Guid.NewGuid(),
                FeatureName = "Reports",
                DepartmentId = Guid.NewGuid(),
                DepartmentName = "IT",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            }
        };

        MediatorMock.Setup(m => m.Send(It.IsAny<GetAllRoleFeatureMappingsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mappings);

        // Act
        var result = await _controller.GetAll();

        // Assert
        var response = AssertOkResult<List<RoleFeatureMappingDto>>(result);
        response!.Data.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetAll_WithNoMappings_ReturnsOkWithEmptyList()
    {
        // Arrange
        MediatorMock.Setup(m => m.Send(It.IsAny<GetAllRoleFeatureMappingsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<RoleFeatureMappingDto>());

        // Act
        var result = await _controller.GetAll();

        // Assert
        var response = AssertOkResult<List<RoleFeatureMappingDto>>(result);
        response!.Data.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAll_WithException_ReturnsInternalServerError()
    {
        // Arrange
        MediatorMock.Setup(m => m.Send(It.IsAny<GetAllRoleFeatureMappingsQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database connection failed"));

        // Act
        var result = await _controller.GetAll();

        // Assert
        AssertInternalServerErrorResult<List<RoleFeatureMappingDto>>(result);
    }

    #endregion

    #region GetById Tests

    [Fact]
    public async Task GetById_WithValidId_ReturnsOkWithMapping()
    {
        // Arrange
        var mappingId = Guid.NewGuid();
        var expectedMapping = new RoleFeatureMappingDto
        {
            Id = mappingId,
            RoleId = Guid.NewGuid(),
            RoleName = "Admin",
            FeatureId = Guid.NewGuid(),
            FeatureName = "Dashboard",
            DepartmentId = null,
            DepartmentName = null,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        MediatorMock.Setup(m => m.Send(It.IsAny<GetRoleFeatureMappingByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedMapping);

        // Act
        var result = await _controller.GetById(mappingId);

        // Assert
        var response = AssertOkResult<RoleFeatureMappingDto>(result);
        response!.Data!.Id.Should().Be(mappingId);
    }

    [Fact]
    public async Task GetById_WithNonExistentId_ReturnsNotFound()
    {
        // Arrange
        var mappingId = Guid.NewGuid();

        MediatorMock.Setup(m => m.Send(It.IsAny<GetRoleFeatureMappingByIdQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new KeyNotFoundException($"Role feature mapping with ID {mappingId} not found"));

        // Act
        var result = await _controller.GetById(mappingId);

        // Assert
        AssertNotFoundResult<RoleFeatureMappingDto>(result);
    }

    [Fact]
    public async Task GetById_WithException_ReturnsInternalServerError()
    {
        // Arrange
        var mappingId = Guid.NewGuid();

        MediatorMock.Setup(m => m.Send(It.IsAny<GetRoleFeatureMappingByIdQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.GetById(mappingId);

        // Assert
        AssertInternalServerErrorResult<RoleFeatureMappingDto>(result);
    }

    #endregion

    #region Create Tests

    [Fact]
    public async Task Create_WithValidDto_ReturnsCreatedWithMapping()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        var featureId = Guid.NewGuid();
        var dto = new CreateRoleFeatureMappingDto
        {
            RoleId = roleId,
            FeatureId = featureId,
            DepartmentId = null,
            IsActive = true
        };

        var expectedResult = new RoleFeatureMappingDto
        {
            Id = Guid.NewGuid(),
            RoleId = roleId,
            RoleName = "Admin",
            FeatureId = featureId,
            FeatureName = "Dashboard",
            DepartmentId = null,
            DepartmentName = null,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        MediatorMock.Setup(m => m.Send(It.IsAny<CreateRoleFeatureMappingCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _controller.Create(dto);

        // Assert
        var response = AssertCreatedResult<RoleFeatureMappingDto>(result);
        response!.Data!.RoleId.Should().Be(roleId);
        response.Data.FeatureId.Should().Be(featureId);
    }

    [Fact]
    public async Task Create_WithDepartmentId_ReturnsCreatedWithMapping()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        var featureId = Guid.NewGuid();
        var departmentId = Guid.NewGuid();
        var dto = new CreateRoleFeatureMappingDto
        {
            RoleId = roleId,
            FeatureId = featureId,
            DepartmentId = departmentId,
            IsActive = true
        };

        var expectedResult = new RoleFeatureMappingDto
        {
            Id = Guid.NewGuid(),
            RoleId = roleId,
            RoleName = "Admin",
            FeatureId = featureId,
            FeatureName = "Dashboard",
            DepartmentId = departmentId,
            DepartmentName = "IT Department",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        MediatorMock.Setup(m => m.Send(It.IsAny<CreateRoleFeatureMappingCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _controller.Create(dto);

        // Assert
        var response = AssertCreatedResult<RoleFeatureMappingDto>(result);
        response!.Data!.DepartmentId.Should().Be(departmentId);
        response.Data.DepartmentName.Should().Be("IT Department");
    }

    [Fact]
    public async Task Create_WithException_ReturnsInternalServerError()
    {
        // Arrange
        var dto = new CreateRoleFeatureMappingDto
        {
            RoleId = Guid.NewGuid(),
            FeatureId = Guid.NewGuid(),
            DepartmentId = null,
            IsActive = true
        };

        MediatorMock.Setup(m => m.Send(It.IsAny<CreateRoleFeatureMappingCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.Create(dto);

        // Assert
        AssertInternalServerErrorResult<RoleFeatureMappingDto>(result);
    }

    [Fact]
    public async Task Create_WithDuplicateMapping_ReturnsInternalServerError()
    {
        // Arrange
        var dto = new CreateRoleFeatureMappingDto
        {
            RoleId = Guid.NewGuid(),
            FeatureId = Guid.NewGuid(),
            DepartmentId = null,
            IsActive = true
        };

        MediatorMock.Setup(m => m.Send(It.IsAny<CreateRoleFeatureMappingCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Mapping already exists for this role and feature"));

        // Act
        var result = await _controller.Create(dto);

        // Assert
        AssertInternalServerErrorResult<RoleFeatureMappingDto>(result);
    }

    #endregion

    #region Update Tests

    [Fact]
    public async Task Update_WithValidDto_ReturnsOkWithUpdatedMapping()
    {
        // Arrange
        var mappingId = Guid.NewGuid();
        var roleId = Guid.NewGuid();
        var featureId = Guid.NewGuid();
        var dto = new UpdateRoleFeatureMappingDto
        {
            Id = mappingId,
            RoleId = roleId,
            FeatureId = featureId,
            DepartmentId = null,
            IsActive = false
        };

        var expectedResult = new RoleFeatureMappingDto
        {
            Id = mappingId,
            RoleId = roleId,
            RoleName = "Admin",
            FeatureId = featureId,
            FeatureName = "Dashboard",
            DepartmentId = null,
            DepartmentName = null,
            IsActive = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        MediatorMock.Setup(m => m.Send(It.IsAny<UpdateRoleFeatureMappingCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _controller.Update(mappingId, dto);

        // Assert
        var response = AssertOkResult<RoleFeatureMappingDto>(result);
        response!.Data!.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task Update_WithIdMismatch_ReturnsBadRequest()
    {
        // Arrange
        var mappingId = Guid.NewGuid();
        var differentId = Guid.NewGuid();
        var dto = new UpdateRoleFeatureMappingDto
        {
            Id = differentId,
            RoleId = Guid.NewGuid(),
            FeatureId = Guid.NewGuid(),
            DepartmentId = null,
            IsActive = true
        };

        // Act
        var result = await _controller.Update(mappingId, dto);

        // Assert
        AssertBadRequestResult<RoleFeatureMappingDto>(result);
    }

    [Fact]
    public async Task Update_WithNonExistentMapping_ReturnsNotFound()
    {
        // Arrange
        var mappingId = Guid.NewGuid();
        var dto = new UpdateRoleFeatureMappingDto
        {
            Id = mappingId,
            RoleId = Guid.NewGuid(),
            FeatureId = Guid.NewGuid(),
            DepartmentId = null,
            IsActive = true
        };

        MediatorMock.Setup(m => m.Send(It.IsAny<UpdateRoleFeatureMappingCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new KeyNotFoundException($"Role feature mapping with ID {mappingId} not found"));

        // Act
        var result = await _controller.Update(mappingId, dto);

        // Assert
        AssertNotFoundResult<RoleFeatureMappingDto>(result);
    }

    [Fact]
    public async Task Update_WithInvalidOperation_ReturnsBadRequest()
    {
        // Arrange
        var mappingId = Guid.NewGuid();
        var dto = new UpdateRoleFeatureMappingDto
        {
            Id = mappingId,
            RoleId = Guid.NewGuid(),
            FeatureId = Guid.NewGuid(),
            DepartmentId = null,
            IsActive = true
        };

        MediatorMock.Setup(m => m.Send(It.IsAny<UpdateRoleFeatureMappingCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Cannot update mapping - role is inactive"));

        // Act
        var result = await _controller.Update(mappingId, dto);

        // Assert
        AssertBadRequestResult<RoleFeatureMappingDto>(result);
    }

    [Fact]
    public async Task Update_WithException_ReturnsInternalServerError()
    {
        // Arrange
        var mappingId = Guid.NewGuid();
        var dto = new UpdateRoleFeatureMappingDto
        {
            Id = mappingId,
            RoleId = Guid.NewGuid(),
            FeatureId = Guid.NewGuid(),
            DepartmentId = null,
            IsActive = true
        };

        MediatorMock.Setup(m => m.Send(It.IsAny<UpdateRoleFeatureMappingCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.Update(mappingId, dto);

        // Assert
        AssertInternalServerErrorResult<RoleFeatureMappingDto>(result);
    }

    #endregion

    #region Delete Tests

    [Fact]
    public async Task Delete_WithValidId_ReturnsOk()
    {
        // Arrange
        var mappingId = Guid.NewGuid();

        MediatorMock.Setup(m => m.Send(It.IsAny<DeleteRoleFeatureMappingCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.Delete(mappingId);

        // Assert
        AssertOkResult<bool>(result);
    }

    [Fact]
    public async Task Delete_WithNonExistentId_ReturnsNotFound()
    {
        // Arrange
        var mappingId = Guid.NewGuid();

        MediatorMock.Setup(m => m.Send(It.IsAny<DeleteRoleFeatureMappingCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new KeyNotFoundException($"Role feature mapping with ID {mappingId} not found"));

        // Act
        var result = await _controller.Delete(mappingId);

        // Assert
        AssertNotFoundResult<bool>(result);
    }

    [Fact]
    public async Task Delete_WithException_ReturnsInternalServerError()
    {
        // Arrange
        var mappingId = Guid.NewGuid();

        MediatorMock.Setup(m => m.Send(It.IsAny<DeleteRoleFeatureMappingCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.Delete(mappingId);

        // Assert
        AssertInternalServerErrorResult<bool>(result);
    }

    #endregion

    #region GetByDepartment Tests

    [Fact]
    public async Task GetByDepartment_WithValidDepartmentId_ReturnsOkWithMappings()
    {
        // Arrange
        var departmentId = Guid.NewGuid();
        var mappings = new List<RoleFeatureMappingDto>
        {
            new() {
                Id = Guid.NewGuid(),
                RoleId = Guid.NewGuid(),
                RoleName = "IT Admin",
                FeatureId = Guid.NewGuid(),
                FeatureName = "Dashboard",
                DepartmentId = departmentId,
                DepartmentName = "IT",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new() {
                Id = Guid.NewGuid(),
                RoleId = Guid.NewGuid(),
                RoleName = "IT Manager",
                FeatureId = Guid.NewGuid(),
                FeatureName = "Reports",
                DepartmentId = departmentId,
                DepartmentName = "IT",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            }
        };

        MediatorMock.Setup(m => m.Send(It.IsAny<GetRoleFeatureMappingsByDepartmentQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mappings);

        // Act
        var result = await _controller.GetByDepartment(departmentId);

        // Assert
        var response = AssertOkResult<List<RoleFeatureMappingDto>>(result);
        response!.Data.Should().HaveCount(2);
        response.Data.Should().AllSatisfy(m => m.DepartmentId.Should().Be(departmentId));
    }

    [Fact]
    public async Task GetByDepartment_WithNoMappings_ReturnsOkWithEmptyList()
    {
        // Arrange
        var departmentId = Guid.NewGuid();

        MediatorMock.Setup(m => m.Send(It.IsAny<GetRoleFeatureMappingsByDepartmentQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<RoleFeatureMappingDto>());

        // Act
        var result = await _controller.GetByDepartment(departmentId);

        // Assert
        var response = AssertOkResult<List<RoleFeatureMappingDto>>(result);
        response!.Data.Should().BeEmpty();
    }

    [Fact]
    public async Task GetByDepartment_WithException_ReturnsInternalServerError()
    {
        // Arrange
        var departmentId = Guid.NewGuid();

        MediatorMock.Setup(m => m.Send(It.IsAny<GetRoleFeatureMappingsByDepartmentQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.GetByDepartment(departmentId);

        // Assert
        AssertInternalServerErrorResult<List<RoleFeatureMappingDto>>(result);
    }

    #endregion

    #region GetByRole Tests

    [Fact]
    public async Task GetByRole_WithValidRoleId_ReturnsOkWithMappings()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        var mappings = new List<RoleFeatureMappingDto>
        {
            new() {
                Id = Guid.NewGuid(),
                RoleId = roleId,
                RoleName = "Admin",
                FeatureId = Guid.NewGuid(),
                FeatureName = "Dashboard",
                DepartmentId = null,
                DepartmentName = null,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new() {
                Id = Guid.NewGuid(),
                RoleId = roleId,
                RoleName = "Admin",
                FeatureId = Guid.NewGuid(),
                FeatureName = "Settings",
                DepartmentId = null,
                DepartmentName = null,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            }
        };

        MediatorMock.Setup(m => m.Send(It.IsAny<GetRoleFeatureMappingsByRoleQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mappings);

        // Act
        var result = await _controller.GetByRole(roleId);

        // Assert
        var response = AssertOkResult<List<RoleFeatureMappingDto>>(result);
        response!.Data.Should().HaveCount(2);
        response.Data.Should().AllSatisfy(m => m.RoleId.Should().Be(roleId));
    }

    [Fact]
    public async Task GetByRole_WithNoMappings_ReturnsOkWithEmptyList()
    {
        // Arrange
        var roleId = Guid.NewGuid();

        MediatorMock.Setup(m => m.Send(It.IsAny<GetRoleFeatureMappingsByRoleQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<RoleFeatureMappingDto>());

        // Act
        var result = await _controller.GetByRole(roleId);

        // Assert
        var response = AssertOkResult<List<RoleFeatureMappingDto>>(result);
        response!.Data.Should().BeEmpty();
    }

    [Fact]
    public async Task GetByRole_WithException_ReturnsInternalServerError()
    {
        // Arrange
        var roleId = Guid.NewGuid();

        MediatorMock.Setup(m => m.Send(It.IsAny<GetRoleFeatureMappingsByRoleQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.GetByRole(roleId);

        // Assert
        AssertInternalServerErrorResult<List<RoleFeatureMappingDto>>(result);
    }

    #endregion
}
