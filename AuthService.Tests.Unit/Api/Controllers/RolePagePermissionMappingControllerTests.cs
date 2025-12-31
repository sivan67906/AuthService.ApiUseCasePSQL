using AuthService.Application.DTOs;
using AuthService.Application.Features.RolePagePermissionMapping.CreateOrUpdateBatch;
using AuthService.Application.Features.RolePagePermissionMapping.CreateRolePagePermissionMapping;
using AuthService.Application.Features.RolePagePermissionMapping.DeleteRolePagePermissionMapping;
using AuthService.Application.Features.RolePagePermissionMapping.GetAllRolePagePermissionMappings;
using AuthService.Application.Features.RolePagePermissionMapping.GetGroupedRolePagePermissions;
using AuthService.Application.Features.RolePagePermissionMapping.GetRolePagePermissionMappingById;
using AuthService.Application.Features.RolePagePermissionMapping.GetRolePagePermissionMappingsByDepartment;
using AuthService.Application.Features.RolePagePermissionMapping.GetRolePagePermissionMappingsByRole;
using AuthService.Application.Features.RolePagePermissionMapping.GetRolePagePermissionMappingsByRoleAndPage;
using AuthService.Application.Features.RolePagePermissionMapping.UpdateRolePagePermissionMapping;

namespace AuthService.Tests.Unit.Api.Controllers;

public class RolePagePermissionMappingControllerTests : ControllerTestBase
{
    private readonly RolePagePermissionMappingController _controller;

    public RolePagePermissionMappingControllerTests()
    {
        _controller = new RolePagePermissionMappingController(MediatorMock.Object);
    }

    #region GetAll Tests

    [Fact]
    public async Task GetAll_WithMappings_ReturnsOkWithList()
    {
        // Arrange
        var mappings = new List<RolePagePermissionMappingDto>
        {
            new() {
                Id = Guid.NewGuid(),
                RoleId = Guid.NewGuid(),
                RoleName = "Admin",
                PageId = Guid.NewGuid(),
                PageName = "Dashboard",
                PermissionId = Guid.NewGuid(),
                PermissionName = "View",
                DepartmentId = null,
                DepartmentName = null,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new() {
                Id = Guid.NewGuid(),
                RoleId = Guid.NewGuid(),
                RoleName = "Manager",
                PageId = Guid.NewGuid(),
                PageName = "Reports",
                PermissionId = Guid.NewGuid(),
                PermissionName = "Edit",
                DepartmentId = Guid.NewGuid(),
                DepartmentName = "IT",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            }
        };

        MediatorMock.Setup(m => m.Send(It.IsAny<GetAllRolePagePermissionMappingsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mappings);

        // Act
        var result = await _controller.GetAll();

        // Assert
        var response = AssertOkResult<List<RolePagePermissionMappingDto>>(result);
        response!.Data.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetAll_WithNoMappings_ReturnsOkWithEmptyList()
    {
        // Arrange
        MediatorMock.Setup(m => m.Send(It.IsAny<GetAllRolePagePermissionMappingsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<RolePagePermissionMappingDto>());

        // Act
        var result = await _controller.GetAll();

        // Assert
        var response = AssertOkResult<List<RolePagePermissionMappingDto>>(result);
        response!.Data.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAll_WithException_ReturnsInternalServerError()
    {
        // Arrange
        MediatorMock.Setup(m => m.Send(It.IsAny<GetAllRolePagePermissionMappingsQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database connection failed"));

        // Act
        var result = await _controller.GetAll();

        // Assert
        AssertInternalServerErrorResult<List<RolePagePermissionMappingDto>>(result);
    }

    #endregion

    #region GetById Tests

    [Fact]
    public async Task GetById_WithValidId_ReturnsOkWithMapping()
    {
        // Arrange
        var mappingId = Guid.NewGuid();
        var expectedMapping = new RolePagePermissionMappingDto
        {
            Id = mappingId,
            RoleId = Guid.NewGuid(),
            RoleName = "Admin",
            PageId = Guid.NewGuid(),
            PageName = "Dashboard",
            PermissionId = Guid.NewGuid(),
            PermissionName = "View",
            DepartmentId = null,
            DepartmentName = null,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        MediatorMock.Setup(m => m.Send(It.IsAny<GetRolePagePermissionMappingByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedMapping);

        // Act
        var result = await _controller.GetById(mappingId);

        // Assert
        var response = AssertOkResult<RolePagePermissionMappingDto>(result);
        response!.Data!.Id.Should().Be(mappingId);
    }

    [Fact]
    public async Task GetById_WithNonExistentId_ReturnsNotFound()
    {
        // Arrange
        var mappingId = Guid.NewGuid();

        MediatorMock.Setup(m => m.Send(It.IsAny<GetRolePagePermissionMappingByIdQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new KeyNotFoundException($"Mapping with ID {mappingId} not found"));

        // Act
        var result = await _controller.GetById(mappingId);

        // Assert
        AssertNotFoundResult<RolePagePermissionMappingDto>(result);
    }

    [Fact]
    public async Task GetById_WithException_ReturnsInternalServerError()
    {
        // Arrange
        var mappingId = Guid.NewGuid();

        MediatorMock.Setup(m => m.Send(It.IsAny<GetRolePagePermissionMappingByIdQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.GetById(mappingId);

        // Assert
        AssertInternalServerErrorResult<RolePagePermissionMappingDto>(result);
    }

    #endregion

    #region Create Tests

    [Fact]
    public async Task Create_WithValidDto_ReturnsCreatedWithMapping()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        var pageId = Guid.NewGuid();
        var permissionId = Guid.NewGuid();
        var dto = new CreateRolePagePermissionMappingDto
        {
            RoleId = roleId,
            PageId = pageId,
            PermissionId = permissionId,
            DepartmentId = null,
            IsActive = true
        };

        var expectedResult = new RolePagePermissionMappingDto
        {
            Id = Guid.NewGuid(),
            RoleId = roleId,
            RoleName = "Admin",
            PageId = pageId,
            PageName = "Dashboard",
            PermissionId = permissionId,
            PermissionName = "View",
            DepartmentId = null,
            DepartmentName = null,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        MediatorMock.Setup(m => m.Send(It.IsAny<CreateRolePagePermissionMappingCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _controller.Create(dto);

        // Assert
        var response = AssertCreatedResult<RolePagePermissionMappingDto>(result);
        response!.Data!.RoleId.Should().Be(roleId);
        response.Data.PageId.Should().Be(pageId);
        response.Data.PermissionId.Should().Be(permissionId);
    }

    [Fact]
    public async Task Create_WithDepartmentId_ReturnsCreatedWithMapping()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        var pageId = Guid.NewGuid();
        var permissionId = Guid.NewGuid();
        var departmentId = Guid.NewGuid();
        var dto = new CreateRolePagePermissionMappingDto
        {
            RoleId = roleId,
            PageId = pageId,
            PermissionId = permissionId,
            DepartmentId = departmentId,
            IsActive = true
        };

        var expectedResult = new RolePagePermissionMappingDto
        {
            Id = Guid.NewGuid(),
            RoleId = roleId,
            RoleName = "IT Admin",
            PageId = pageId,
            PageName = "IT Dashboard",
            PermissionId = permissionId,
            PermissionName = "Edit",
            DepartmentId = departmentId,
            DepartmentName = "IT Department",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        MediatorMock.Setup(m => m.Send(It.IsAny<CreateRolePagePermissionMappingCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _controller.Create(dto);

        // Assert
        var response = AssertCreatedResult<RolePagePermissionMappingDto>(result);
        response!.Data!.DepartmentId.Should().Be(departmentId);
        response.Data.DepartmentName.Should().Be("IT Department");
    }

    [Fact]
    public async Task Create_WithException_ReturnsInternalServerError()
    {
        // Arrange
        var dto = new CreateRolePagePermissionMappingDto
        {
            RoleId = Guid.NewGuid(),
            PageId = Guid.NewGuid(),
            PermissionId = Guid.NewGuid(),
            DepartmentId = null,
            IsActive = true
        };

        MediatorMock.Setup(m => m.Send(It.IsAny<CreateRolePagePermissionMappingCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.Create(dto);

        // Assert
        AssertInternalServerErrorResult<RolePagePermissionMappingDto>(result);
    }

    #endregion

    #region Update Tests

    [Fact]
    public async Task Update_WithValidDto_ReturnsOkWithUpdatedMapping()
    {
        // Arrange
        var mappingId = Guid.NewGuid();
        var dto = new UpdateRolePagePermissionMappingDto
        {
            Id = mappingId,
            RoleId = Guid.NewGuid(),
            PageId = Guid.NewGuid(),
            PermissionId = Guid.NewGuid(),
            DepartmentId = null,
            IsActive = false
        };

        var expectedResult = new RolePagePermissionMappingDto
        {
            Id = mappingId,
            RoleId = dto.RoleId,
            RoleName = "Admin",
            PageId = dto.PageId,
            PageName = "Dashboard",
            PermissionId = dto.PermissionId,
            PermissionName = "View",
            DepartmentId = null,
            DepartmentName = null,
            IsActive = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        MediatorMock.Setup(m => m.Send(It.IsAny<UpdateRolePagePermissionMappingCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _controller.Update(mappingId, dto);

        // Assert
        var response = AssertOkResult<RolePagePermissionMappingDto>(result);
        response!.Data!.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task Update_WithIdMismatch_ReturnsBadRequest()
    {
        // Arrange
        var mappingId = Guid.NewGuid();
        var differentId = Guid.NewGuid();
        var dto = new UpdateRolePagePermissionMappingDto
        {
            Id = differentId,
            RoleId = Guid.NewGuid(),
            PageId = Guid.NewGuid(),
            PermissionId = Guid.NewGuid(),
            DepartmentId = null,
            IsActive = true
        };

        // Act
        var result = await _controller.Update(mappingId, dto);

        // Assert
        AssertBadRequestResult<RolePagePermissionMappingDto>(result);
    }

    [Fact]
    public async Task Update_WithNonExistentMapping_ReturnsNotFound()
    {
        // Arrange
        var mappingId = Guid.NewGuid();
        var dto = new UpdateRolePagePermissionMappingDto
        {
            Id = mappingId,
            RoleId = Guid.NewGuid(),
            PageId = Guid.NewGuid(),
            PermissionId = Guid.NewGuid(),
            DepartmentId = null,
            IsActive = true
        };

        MediatorMock.Setup(m => m.Send(It.IsAny<UpdateRolePagePermissionMappingCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new KeyNotFoundException($"Mapping with ID {mappingId} not found"));

        // Act
        var result = await _controller.Update(mappingId, dto);

        // Assert
        AssertNotFoundResult<RolePagePermissionMappingDto>(result);
    }

    [Fact]
    public async Task Update_WithInvalidOperation_ReturnsBadRequest()
    {
        // Arrange
        var mappingId = Guid.NewGuid();
        var dto = new UpdateRolePagePermissionMappingDto
        {
            Id = mappingId,
            RoleId = Guid.NewGuid(),
            PageId = Guid.NewGuid(),
            PermissionId = Guid.NewGuid(),
            DepartmentId = null,
            IsActive = true
        };

        MediatorMock.Setup(m => m.Send(It.IsAny<UpdateRolePagePermissionMappingCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Cannot update - permission is inactive"));

        // Act
        var result = await _controller.Update(mappingId, dto);

        // Assert
        AssertBadRequestResult<RolePagePermissionMappingDto>(result);
    }

    [Fact]
    public async Task Update_WithException_ReturnsInternalServerError()
    {
        // Arrange
        var mappingId = Guid.NewGuid();
        var dto = new UpdateRolePagePermissionMappingDto
        {
            Id = mappingId,
            RoleId = Guid.NewGuid(),
            PageId = Guid.NewGuid(),
            PermissionId = Guid.NewGuid(),
            DepartmentId = null,
            IsActive = true
        };

        MediatorMock.Setup(m => m.Send(It.IsAny<UpdateRolePagePermissionMappingCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.Update(mappingId, dto);

        // Assert
        AssertInternalServerErrorResult<RolePagePermissionMappingDto>(result);
    }

    #endregion

    #region Delete Tests

    [Fact]
    public async Task Delete_WithValidId_ReturnsOk()
    {
        // Arrange
        var mappingId = Guid.NewGuid();

        MediatorMock.Setup(m => m.Send(It.IsAny<DeleteRolePagePermissionMappingCommand>(), It.IsAny<CancellationToken>()))
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

        MediatorMock.Setup(m => m.Send(It.IsAny<DeleteRolePagePermissionMappingCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new KeyNotFoundException($"Mapping with ID {mappingId} not found"));

        // Act
        var result = await _controller.Delete(mappingId);

        // Assert
        AssertNotFoundResult<bool>(result);
    }

    [Fact]
    public async Task Delete_WithInvalidOperation_ReturnsBadRequest()
    {
        // Arrange
        var mappingId = Guid.NewGuid();

        MediatorMock.Setup(m => m.Send(It.IsAny<DeleteRolePagePermissionMappingCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Cannot delete - mapping is in use"));

        // Act
        var result = await _controller.Delete(mappingId);

        // Assert
        AssertBadRequestResult<bool>(result);
    }

    [Fact]
    public async Task Delete_WithException_ReturnsInternalServerError()
    {
        // Arrange
        var mappingId = Guid.NewGuid();

        MediatorMock.Setup(m => m.Send(It.IsAny<DeleteRolePagePermissionMappingCommand>(), It.IsAny<CancellationToken>()))
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
        var mappings = new List<RolePagePermissionMappingDto>
        {
            new() {
                Id = Guid.NewGuid(),
                RoleId = Guid.NewGuid(),
                RoleName = "IT Admin",
                PageId = Guid.NewGuid(),
                PageName = "IT Dashboard",
                PermissionId = Guid.NewGuid(),
                PermissionName = "View",
                DepartmentId = departmentId,
                DepartmentName = "IT",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            }
        };

        MediatorMock.Setup(m => m.Send(It.IsAny<GetRolePagePermissionMappingsByDepartmentQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mappings);

        // Act
        var result = await _controller.GetByDepartment(departmentId);

        // Assert
        var response = AssertOkResult<List<RolePagePermissionMappingDto>>(result);
        response!.Data.Should().HaveCount(1);
        response.Data.Should().AllSatisfy(m => m.DepartmentId.Should().Be(departmentId));
    }

    [Fact]
    public async Task GetByDepartment_WithNoMappings_ReturnsOkWithEmptyList()
    {
        // Arrange
        var departmentId = Guid.NewGuid();

        MediatorMock.Setup(m => m.Send(It.IsAny<GetRolePagePermissionMappingsByDepartmentQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<RolePagePermissionMappingDto>());

        // Act
        var result = await _controller.GetByDepartment(departmentId);

        // Assert
        var response = AssertOkResult<List<RolePagePermissionMappingDto>>(result);
        response!.Data.Should().BeEmpty();
    }

    [Fact]
    public async Task GetByDepartment_WithException_ReturnsInternalServerError()
    {
        // Arrange
        var departmentId = Guid.NewGuid();

        MediatorMock.Setup(m => m.Send(It.IsAny<GetRolePagePermissionMappingsByDepartmentQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.GetByDepartment(departmentId);

        // Assert
        AssertInternalServerErrorResult<List<RolePagePermissionMappingDto>>(result);
    }

    #endregion

    #region GetByRole Tests

    [Fact]
    public async Task GetByRole_WithValidRoleId_ReturnsOkWithMappings()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        var mappings = new List<RolePagePermissionMappingDto>
        {
            new() {
                Id = Guid.NewGuid(),
                RoleId = roleId,
                RoleName = "Admin",
                PageId = Guid.NewGuid(),
                PageName = "Dashboard",
                PermissionId = Guid.NewGuid(),
                PermissionName = "View",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new() {
                Id = Guid.NewGuid(),
                RoleId = roleId,
                RoleName = "Admin",
                PageId = Guid.NewGuid(),
                PageName = "Settings",
                PermissionId = Guid.NewGuid(),
                PermissionName = "Edit",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            }
        };

        MediatorMock.Setup(m => m.Send(It.IsAny<GetRolePagePermissionMappingsByRoleQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mappings);

        // Act
        var result = await _controller.GetByRole(roleId);

        // Assert
        var response = AssertOkResult<List<RolePagePermissionMappingDto>>(result);
        response!.Data.Should().HaveCount(2);
        response.Data.Should().AllSatisfy(m => m.RoleId.Should().Be(roleId));
    }

    [Fact]
    public async Task GetByRole_WithException_ReturnsInternalServerError()
    {
        // Arrange
        var roleId = Guid.NewGuid();

        MediatorMock.Setup(m => m.Send(It.IsAny<GetRolePagePermissionMappingsByRoleQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.GetByRole(roleId);

        // Assert
        AssertInternalServerErrorResult<List<RolePagePermissionMappingDto>>(result);
    }

    #endregion

    #region GetByRoleAndPage Tests

    [Fact]
    public async Task GetByRoleAndPage_WithValidIds_ReturnsOkWithMappings()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        var pageId = Guid.NewGuid();
        var mappings = new List<RolePagePermissionMappingDto>
        {
            new() {
                Id = Guid.NewGuid(),
                RoleId = roleId,
                RoleName = "Admin",
                PageId = pageId,
                PageName = "Dashboard",
                PermissionId = Guid.NewGuid(),
                PermissionName = "View",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new() {
                Id = Guid.NewGuid(),
                RoleId = roleId,
                RoleName = "Admin",
                PageId = pageId,
                PageName = "Dashboard",
                PermissionId = Guid.NewGuid(),
                PermissionName = "Edit",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            }
        };

        MediatorMock.Setup(m => m.Send(It.IsAny<GetRolePagePermissionMappingsByRoleAndPageQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mappings);

        // Act
        var result = await _controller.GetByRoleAndPage(roleId, pageId);

        // Assert
        var response = AssertOkResult<List<RolePagePermissionMappingDto>>(result);
        response!.Data.Should().HaveCount(2);
        response.Data.Should().AllSatisfy(m =>
        {
            m.RoleId.Should().Be(roleId);
            m.PageId.Should().Be(pageId);
        });
    }

    [Fact]
    public async Task GetByRoleAndPage_WithNoMappings_ReturnsOkWithEmptyList()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        var pageId = Guid.NewGuid();

        MediatorMock.Setup(m => m.Send(It.IsAny<GetRolePagePermissionMappingsByRoleAndPageQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<RolePagePermissionMappingDto>());

        // Act
        var result = await _controller.GetByRoleAndPage(roleId, pageId);

        // Assert
        var response = AssertOkResult<List<RolePagePermissionMappingDto>>(result);
        response!.Data.Should().BeEmpty();
    }

    [Fact]
    public async Task GetByRoleAndPage_WithException_ReturnsInternalServerError()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        var pageId = Guid.NewGuid();

        MediatorMock.Setup(m => m.Send(It.IsAny<GetRolePagePermissionMappingsByRoleAndPageQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.GetByRoleAndPage(roleId, pageId);

        // Assert
        AssertInternalServerErrorResult<List<RolePagePermissionMappingDto>>(result);
    }

    #endregion

    #region GetGrouped Tests

    [Fact]
    public async Task GetGrouped_WithMappings_ReturnsOkWithGroupedList()
    {
        // Arrange
        var groupedMappings = new List<RolePagePermissionGroupDto>
        {
            new() {
                DepartmentId = Guid.NewGuid(),
                DepartmentName = "IT",
                RoleId = Guid.NewGuid(),
                RoleName = "Admin",
                PageId = Guid.NewGuid(),
                PageName = "Dashboard",
                PageUrl = "/dashboard",
                Permissions = new List<PermissionBadgeDto>
                {
                    new() {
                        Id = Guid.NewGuid(),
                        PermissionId = Guid.NewGuid(),
                        PermissionName = "View",
                        PermissionCode = "VIEW",
                        BadgeColor = "primary"
                    },
                    new() {
                        Id = Guid.NewGuid(),
                        PermissionId = Guid.NewGuid(),
                        PermissionName = "Edit",
                        PermissionCode = "EDIT",
                        BadgeColor = "success"
                    }
                },
                CreatedAt = DateTime.UtcNow
            }
        };

        MediatorMock.Setup(m => m.Send(It.IsAny<GetGroupedRolePagePermissionsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(groupedMappings);

        // Act
        var result = await _controller.GetGrouped();

        // Assert
        var response = AssertOkResult<List<RolePagePermissionGroupDto>>(result);
        response!.Data.Should().HaveCount(1);
        response.Data!.First().Permissions.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetGrouped_WithNoMappings_ReturnsOkWithEmptyList()
    {
        // Arrange
        MediatorMock.Setup(m => m.Send(It.IsAny<GetGroupedRolePagePermissionsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<RolePagePermissionGroupDto>());

        // Act
        var result = await _controller.GetGrouped();

        // Assert
        var response = AssertOkResult<List<RolePagePermissionGroupDto>>(result);
        response!.Data.Should().BeEmpty();
    }

    [Fact]
    public async Task GetGrouped_WithException_ReturnsInternalServerError()
    {
        // Arrange
        MediatorMock.Setup(m => m.Send(It.IsAny<GetGroupedRolePagePermissionsQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.GetGrouped();

        // Assert
        AssertInternalServerErrorResult<List<RolePagePermissionGroupDto>>(result);
    }

    #endregion

    #region CreateOrUpdateBatch Tests

    [Fact]
    public async Task CreateOrUpdateBatch_WithValidDto_ReturnsOkWithMappings()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        var pageId = Guid.NewGuid();
        var departmentId = Guid.NewGuid();
        var permissionIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
        var dto = new CreateOrUpdatePermissionBatchDto
        {
            DepartmentId = departmentId,
            RoleId = roleId,
            PageId = pageId,
            PermissionIds = permissionIds
        };

        var expectedResult = new List<RolePagePermissionMappingDto>
        {
            new() {
                Id = Guid.NewGuid(),
                RoleId = roleId,
                RoleName = "Admin",
                PageId = pageId,
                PageName = "Dashboard",
                PermissionId = permissionIds[0],
                PermissionName = "View",
                DepartmentId = departmentId,
                DepartmentName = "IT",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new() {
                Id = Guid.NewGuid(),
                RoleId = roleId,
                RoleName = "Admin",
                PageId = pageId,
                PageName = "Dashboard",
                PermissionId = permissionIds[1],
                PermissionName = "Edit",
                DepartmentId = departmentId,
                DepartmentName = "IT",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            }
        };

        MediatorMock.Setup(m => m.Send(It.IsAny<CreateOrUpdateRolePagePermissionBatchCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _controller.CreateOrUpdateBatch(dto);

        // Assert
        var response = AssertOkResult<List<RolePagePermissionMappingDto>>(result);
        response!.Data.Should().HaveCount(2);
    }

    [Fact]
    public async Task CreateOrUpdateBatch_WithEmptyPermissionIds_ReturnsOkWithEmptyList()
    {
        // Arrange
        var dto = new CreateOrUpdatePermissionBatchDto
        {
            DepartmentId = Guid.NewGuid(),
            RoleId = Guid.NewGuid(),
            PageId = Guid.NewGuid(),
            PermissionIds = new List<Guid>()
        };

        MediatorMock.Setup(m => m.Send(It.IsAny<CreateOrUpdateRolePagePermissionBatchCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<RolePagePermissionMappingDto>());

        // Act
        var result = await _controller.CreateOrUpdateBatch(dto);

        // Assert
        var response = AssertOkResult<List<RolePagePermissionMappingDto>>(result);
        response!.Data.Should().BeEmpty();
    }

    [Fact]
    public async Task CreateOrUpdateBatch_WithInvalidOperation_ReturnsBadRequest()
    {
        // Arrange
        var dto = new CreateOrUpdatePermissionBatchDto
        {
            DepartmentId = Guid.NewGuid(),
            RoleId = Guid.NewGuid(),
            PageId = Guid.NewGuid(),
            PermissionIds = new List<Guid> { Guid.NewGuid() }
        };

        MediatorMock.Setup(m => m.Send(It.IsAny<CreateOrUpdateRolePagePermissionBatchCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Role not found"));

        // Act
        var result = await _controller.CreateOrUpdateBatch(dto);

        // Assert
        AssertBadRequestResult<List<RolePagePermissionMappingDto>>(result);
    }

    [Fact]
    public async Task CreateOrUpdateBatch_WithException_ReturnsInternalServerError()
    {
        // Arrange
        var dto = new CreateOrUpdatePermissionBatchDto
        {
            DepartmentId = Guid.NewGuid(),
            RoleId = Guid.NewGuid(),
            PageId = Guid.NewGuid(),
            PermissionIds = new List<Guid> { Guid.NewGuid() }
        };

        MediatorMock.Setup(m => m.Send(It.IsAny<CreateOrUpdateRolePagePermissionBatchCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.CreateOrUpdateBatch(dto);

        // Assert
        AssertInternalServerErrorResult<List<RolePagePermissionMappingDto>>(result);
    }

    [Fact]
    public async Task CreateOrUpdateBatch_WithNullDepartmentId_ReturnsOkWithMappings()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        var pageId = Guid.NewGuid();
        var dto = new CreateOrUpdatePermissionBatchDto
        {
            DepartmentId = null,
            RoleId = roleId,
            PageId = pageId,
            PermissionIds = new List<Guid> { Guid.NewGuid() }
        };

        var expectedResult = new List<RolePagePermissionMappingDto>
        {
            new() {
                Id = Guid.NewGuid(),
                RoleId = roleId,
                RoleName = "Super Admin",
                PageId = pageId,
                PageName = "System Settings",
                PermissionId = dto.PermissionIds[0],
                PermissionName = "Full Access",
                DepartmentId = null,
                DepartmentName = null,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            }
        };

        MediatorMock.Setup(m => m.Send(It.IsAny<CreateOrUpdateRolePagePermissionBatchCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _controller.CreateOrUpdateBatch(dto);

        // Assert
        var response = AssertOkResult<List<RolePagePermissionMappingDto>>(result);
        response!.Data.Should().HaveCount(1);
        response.Data!.First().DepartmentId.Should().BeNull();
    }

    #endregion
}
