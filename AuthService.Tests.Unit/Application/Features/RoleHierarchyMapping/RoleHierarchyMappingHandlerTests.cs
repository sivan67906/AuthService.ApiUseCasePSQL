using AuthService.Application.Features.RoleHierarchyMapping.CreateRoleHierarchyMapping;
using AuthService.Application.Features.RoleHierarchyMapping.DeleteRoleHierarchyMapping;
using AuthService.Application.Features.RoleHierarchyMapping.GetAllRoleHierarchyMappings;
using AuthService.Application.Features.RoleHierarchyMapping.GetRoleHierarchyMappingById;
using AuthService.Application.Features.RoleHierarchyMapping.UpdateRoleHierarchyMapping;
using MockQueryable.Moq;
using DepartmentEntity = AuthService.Domain.Entities.Department;
using RoleHierarchyEntity = AuthService.Domain.Entities.RoleHierarchy;

namespace AuthService.Tests.Unit.Application.Features.RoleHierarchyMapping;

#region CreateRoleHierarchyMapping Tests

public class CreateRoleHierarchyMappingCommandHandlerTests : ApplicationTestBase
{
    private readonly CreateRoleHierarchyMappingCommandHandler _handler;

    public CreateRoleHierarchyMappingCommandHandlerTests()
    {
        _handler = new CreateRoleHierarchyMappingCommandHandler(DbContextMock.Object);
    }

    #region Positive Scenarios

    [Fact]
    public async Task Handle_ValidMapping_ReturnsCreatedMapping()
    {
        // Arrange
        var departmentId = Guid.NewGuid();
        var department = CreateTestDepartment(id: departmentId, name: "IT Department");
        var parentRole = CreateTestRole(name: "Manager", departmentId: departmentId);
        parentRole.Department = department;
        var childRole = CreateTestRole(name: "Developer", departmentId: departmentId);
        childRole.Department = department;

        var roles = new List<ApplicationRole> { parentRole, childRole };
        var hierarchies = new List<RoleHierarchyEntity>();

        var mockRoles = roles.AsQueryable().BuildMockDbSet();
        var mockHierarchies = hierarchies.AsQueryable().BuildMockDbSet();

        DbContextMock.Setup(x => x.Roles).Returns(mockRoles.Object);
        DbContextMock.Setup(x => x.RoleHierarchies).Returns(mockHierarchies.Object);
        DbContextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var command = new CreateRoleHierarchyMappingCommand
        {
            ParentRoleId = parentRole.Id,
            ChildRoleId = childRole.Id,
            Level = 1
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.ParentRoleId.Should().Be(parentRole.Id);
        result.ChildRoleId.Should().Be(childRole.Id);
        result.Level.Should().Be(1);
    }

    [Fact]
    public async Task Handle_ValidMapping_SetsDepartmentFromRoles()
    {
        // Arrange
        var departmentId = Guid.NewGuid();
        var department = CreateTestDepartment(id: departmentId, name: "HR Department");
        var parentRole = CreateTestRole(name: "HR Manager", departmentId: departmentId);
        parentRole.Department = department;
        var childRole = CreateTestRole(name: "HR Staff", departmentId: departmentId);
        childRole.Department = department;

        var roles = new List<ApplicationRole> { parentRole, childRole };
        var hierarchies = new List<RoleHierarchyEntity>();

        var mockRoles = roles.AsQueryable().BuildMockDbSet();
        var mockHierarchies = hierarchies.AsQueryable().BuildMockDbSet();

        DbContextMock.Setup(x => x.Roles).Returns(mockRoles.Object);
        DbContextMock.Setup(x => x.RoleHierarchies).Returns(mockHierarchies.Object);
        DbContextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var command = new CreateRoleHierarchyMappingCommand
        {
            ParentRoleId = parentRole.Id,
            ChildRoleId = childRole.Id,
            Level = 1
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.DepartmentId.Should().Be(departmentId);
    }

    #endregion

    #region Negative Scenarios

    [Fact]
    public async Task Handle_ParentRoleNotFound_ThrowsInvalidOperationException()
    {
        // Arrange
        var roles = new List<ApplicationRole>();
        var hierarchies = new List<RoleHierarchyEntity>();

        var mockRoles = roles.AsQueryable().BuildMockDbSet();
        var mockHierarchies = hierarchies.AsQueryable().BuildMockDbSet();

        DbContextMock.Setup(x => x.Roles).Returns(mockRoles.Object);
        DbContextMock.Setup(x => x.RoleHierarchies).Returns(mockHierarchies.Object);

        var command = new CreateRoleHierarchyMappingCommand
        {
            ParentRoleId = Guid.NewGuid(),
            ChildRoleId = Guid.NewGuid(),
            Level = 1
        };

        // Act & Assert
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Parent role not found*");
    }

    [Fact]
    public async Task Handle_ChildRoleNotFound_ThrowsInvalidOperationException()
    {
        // Arrange
        var departmentId = Guid.NewGuid();
        var department = CreateTestDepartment(id: departmentId, name: "IT");
        var parentRole = CreateTestRole(name: "Manager", departmentId: departmentId);
        parentRole.Department = department;

        var roles = new List<ApplicationRole> { parentRole };
        var hierarchies = new List<RoleHierarchyEntity>();

        var mockRoles = roles.AsQueryable().BuildMockDbSet();
        var mockHierarchies = hierarchies.AsQueryable().BuildMockDbSet();

        DbContextMock.Setup(x => x.Roles).Returns(mockRoles.Object);
        DbContextMock.Setup(x => x.RoleHierarchies).Returns(mockHierarchies.Object);

        var command = new CreateRoleHierarchyMappingCommand
        {
            ParentRoleId = parentRole.Id,
            ChildRoleId = Guid.NewGuid(),
            Level = 1
        };

        // Act & Assert
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Child role not found*");
    }

    [Fact]
    public async Task Handle_DuplicateMapping_ThrowsInvalidOperationException()
    {
        // Arrange
        var departmentId = Guid.NewGuid();
        var department = CreateTestDepartment(id: departmentId, name: "IT");
        var parentRole = CreateTestRole(name: "Manager", departmentId: departmentId);
        parentRole.Department = department;
        var childRole = CreateTestRole(name: "Developer", departmentId: departmentId);
        childRole.Department = department;

        var existingHierarchy = new RoleHierarchyEntity
        {
            Id = Guid.NewGuid(),
            DepartmentId = departmentId,
            ParentRoleId = parentRole.Id,
            ChildRoleId = childRole.Id,
            Level = 1,
            IsDeleted = false
        };

        var roles = new List<ApplicationRole> { parentRole, childRole };
        var hierarchies = new List<RoleHierarchyEntity> { existingHierarchy };

        var mockRoles = roles.AsQueryable().BuildMockDbSet();
        var mockHierarchies = hierarchies.AsQueryable().BuildMockDbSet();

        DbContextMock.Setup(x => x.Roles).Returns(mockRoles.Object);
        DbContextMock.Setup(x => x.RoleHierarchies).Returns(mockHierarchies.Object);

        var command = new CreateRoleHierarchyMappingCommand
        {
            ParentRoleId = parentRole.Id,
            ChildRoleId = childRole.Id,
            Level = 1
        };

        // Act & Assert
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*already exists*");
    }

    [Fact]
    public async Task Handle_CircularHierarchy_ThrowsInvalidOperationException()
    {
        // Arrange
        var departmentId = Guid.NewGuid();
        var department = CreateTestDepartment(id: departmentId, name: "IT");
        var role = CreateTestRole(name: "Manager", departmentId: departmentId);
        role.Department = department;

        var roles = new List<ApplicationRole> { role };
        var hierarchies = new List<RoleHierarchyEntity>();

        var mockRoles = roles.AsQueryable().BuildMockDbSet();
        var mockHierarchies = hierarchies.AsQueryable().BuildMockDbSet();

        DbContextMock.Setup(x => x.Roles).Returns(mockRoles.Object);
        DbContextMock.Setup(x => x.RoleHierarchies).Returns(mockHierarchies.Object);

        var command = new CreateRoleHierarchyMappingCommand
        {
            ParentRoleId = role.Id,
            ChildRoleId = role.Id, // Same role - circular
            Level = 1
        };

        // Act & Assert
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*cannot be its own parent*");
    }

    [Fact]
    public async Task Handle_DifferentDepartments_ThrowsInvalidOperationException()
    {
        // Arrange
        var department1Id = Guid.NewGuid();
        var department2Id = Guid.NewGuid();
        var department1 = CreateTestDepartment(id: department1Id, name: "IT");
        var department2 = CreateTestDepartment(id: department2Id, name: "HR");
        var parentRole = CreateTestRole(name: "IT Manager", departmentId: department1Id);
        parentRole.Department = department1;
        var childRole = CreateTestRole(name: "HR Staff", departmentId: department2Id);
        childRole.Department = department2;

        var roles = new List<ApplicationRole> { parentRole, childRole };
        var hierarchies = new List<RoleHierarchyEntity>();

        var mockRoles = roles.AsQueryable().BuildMockDbSet();
        var mockHierarchies = hierarchies.AsQueryable().BuildMockDbSet();

        DbContextMock.Setup(x => x.Roles).Returns(mockRoles.Object);
        DbContextMock.Setup(x => x.RoleHierarchies).Returns(mockHierarchies.Object);

        var command = new CreateRoleHierarchyMappingCommand
        {
            ParentRoleId = parentRole.Id,
            ChildRoleId = childRole.Id,
            Level = 1
        };

        // Act & Assert
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*same department*");
    }

    #endregion
}

#endregion

#region GetAllRoleHierarchyMappings Tests

public class GetAllRoleHierarchyMappingsQueryHandlerTests : ApplicationTestBase
{
    private readonly GetAllRoleHierarchyMappingsQueryHandler _handler;

    public GetAllRoleHierarchyMappingsQueryHandlerTests()
    {
        _handler = new GetAllRoleHierarchyMappingsQueryHandler(DbContextMock.Object);
    }

    [Fact]
    public async Task Handle_WithMappings_ReturnsAllMappings()
    {
        // Arrange
        var departmentId = Guid.NewGuid();
        var department = CreateTestDepartment(id: departmentId, name: "IT");
        var parentRole = CreateTestRole(name: "Manager", departmentId: departmentId);
        parentRole.Department = department;
        var childRole = CreateTestRole(name: "Developer", departmentId: departmentId);
        childRole.Department = department;

        var hierarchies = new List<RoleHierarchyEntity>
        {
            new() {
                Id = Guid.NewGuid(),
                DepartmentId = departmentId,
                Department = department,
                ParentRoleId = parentRole.Id,
                ParentRole = parentRole,
                ChildRoleId = childRole.Id,
                ChildRole = childRole,
                Level = 1,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            }
        };

        var mockHierarchies = hierarchies.AsQueryable().BuildMockDbSet();
        DbContextMock.Setup(x => x.RoleHierarchies).Returns(mockHierarchies.Object);

        var query = new GetAllRoleHierarchyMappingsQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().HaveCount(1);
        result.First().ParentRoleName.Should().Be("Manager");
        result.First().ChildRoleName.Should().Be("Developer");
    }

    [Fact]
    public async Task Handle_NoMappings_ReturnsEmptyList()
    {
        // Arrange
        var hierarchies = new List<RoleHierarchyEntity>();
        var mockHierarchies = hierarchies.AsQueryable().BuildMockDbSet();

        DbContextMock.Setup(x => x.RoleHierarchies).Returns(mockHierarchies.Object);

        var query = new GetAllRoleHierarchyMappingsQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_MultipleMappings_ReturnsAllOrderedByDate()
    {
        // Arrange
        var departmentId = Guid.NewGuid();
        var department = CreateTestDepartment(id: departmentId, name: "IT");
        var managerRole = CreateTestRole(name: "Manager", departmentId: departmentId);
        managerRole.Department = department;
        var seniorDevRole = CreateTestRole(name: "Senior Developer", departmentId: departmentId);
        seniorDevRole.Department = department;
        var devRole = CreateTestRole(name: "Developer", departmentId: departmentId);
        devRole.Department = department;

        var hierarchies = new List<RoleHierarchyEntity>
        {
            new() {
                Id = Guid.NewGuid(),
                DepartmentId = departmentId,
                Department = department,
                ParentRoleId = managerRole.Id,
                ParentRole = managerRole,
                ChildRoleId = seniorDevRole.Id,
                ChildRole = seniorDevRole,
                Level = 1,
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddDays(-1)
            },
            new() {
                Id = Guid.NewGuid(),
                DepartmentId = departmentId,
                Department = department,
                ParentRoleId = seniorDevRole.Id,
                ParentRole = seniorDevRole,
                ChildRoleId = devRole.Id,
                ChildRole = devRole,
                Level = 2,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            }
        };

        var mockHierarchies = hierarchies.AsQueryable().BuildMockDbSet();
        DbContextMock.Setup(x => x.RoleHierarchies).Returns(mockHierarchies.Object);

        var query = new GetAllRoleHierarchyMappingsQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);
    }
}

#endregion

#region GetRoleHierarchyMappingById Tests

public class GetRoleHierarchyMappingByIdQueryHandlerTests : ApplicationTestBase
{
    private readonly GetRoleHierarchyMappingByIdQueryHandler _handler;

    public GetRoleHierarchyMappingByIdQueryHandlerTests()
    {
        _handler = new GetRoleHierarchyMappingByIdQueryHandler(DbContextMock.Object);
    }

    [Fact]
    public async Task Handle_ExistingMapping_ReturnsMapping()
    {
        // Arrange
        var mappingId = Guid.NewGuid();
        var departmentId = Guid.NewGuid();
        var department = CreateTestDepartment(id: departmentId, name: "IT");
        var parentRole = CreateTestRole(name: "Manager", departmentId: departmentId);
        parentRole.Department = department;
        var childRole = CreateTestRole(name: "Developer", departmentId: departmentId);
        childRole.Department = department;

        var hierarchy = new RoleHierarchyEntity
        {
            Id = mappingId,
            DepartmentId = departmentId,
            Department = department,
            ParentRoleId = parentRole.Id,
            ParentRole = parentRole,
            ChildRoleId = childRole.Id,
            ChildRole = childRole,
            Level = 1,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var hierarchies = new List<RoleHierarchyEntity> { hierarchy };
        var mockHierarchies = hierarchies.AsQueryable().BuildMockDbSet();

        DbContextMock.Setup(x => x.RoleHierarchies).Returns(mockHierarchies.Object);

        var query = new GetRoleHierarchyMappingByIdQuery(mappingId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(mappingId);
        result.ParentRoleName.Should().Be("Manager");
        result.ChildRoleName.Should().Be("Developer");
    }

    [Fact]
    public async Task Handle_NonExistentMapping_ReturnsNull()
    {
        // Arrange
        var hierarchies = new List<RoleHierarchyEntity>();
        var mockHierarchies = hierarchies.AsQueryable().BuildMockDbSet();

        DbContextMock.Setup(x => x.RoleHierarchies).Returns(mockHierarchies.Object);

        var query = new GetRoleHierarchyMappingByIdQuery(Guid.NewGuid());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }
}

#endregion

#region UpdateRoleHierarchyMapping Tests

public class UpdateRoleHierarchyMappingCommandHandlerTests : ApplicationTestBase
{
    private readonly UpdateRoleHierarchyMappingCommandHandler _handler;

    public UpdateRoleHierarchyMappingCommandHandlerTests()
    {
        _handler = new UpdateRoleHierarchyMappingCommandHandler(DbContextMock.Object);
    }

    [Fact]
    public async Task Handle_ValidUpdate_ReturnsUpdatedMapping()
    {
        // Arrange
        var mappingId = Guid.NewGuid();
        var departmentId = Guid.NewGuid();
        var department = CreateTestDepartment(id: departmentId, name: "IT");
        var parentRole = CreateTestRole(name: "Manager", departmentId: departmentId);
        parentRole.Department = department;
        var childRole = CreateTestRole(name: "Developer", departmentId: departmentId);
        childRole.Department = department;
        var newChildRole = CreateTestRole(name: "Senior Developer", departmentId: departmentId);
        newChildRole.Department = department;

        var existingHierarchy = new RoleHierarchyEntity
        {
            Id = mappingId,
            DepartmentId = departmentId,
            Department = department,
            ParentRoleId = parentRole.Id,
            ParentRole = parentRole,
            ChildRoleId = childRole.Id,
            ChildRole = childRole,
            Level = 1,
            IsActive = true,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow
        };

        var roles = new List<ApplicationRole> { parentRole, childRole, newChildRole };
        var hierarchies = new List<RoleHierarchyEntity> { existingHierarchy };
        var departments = new List<DepartmentEntity> { department };

        var mockRoles = roles.AsQueryable().BuildMockDbSet();
        var mockHierarchies = hierarchies.AsQueryable().BuildMockDbSet();
        var mockDepartments = departments.AsQueryable().BuildMockDbSet();

        DbContextMock.Setup(x => x.Roles).Returns(mockRoles.Object);
        DbContextMock.Setup(x => x.RoleHierarchies).Returns(mockHierarchies.Object);
        DbContextMock.Setup(x => x.Departments).Returns(mockDepartments.Object);
        DbContextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var command = new UpdateRoleHierarchyMappingCommand
        {
            Id = mappingId,
            ParentRoleId = parentRole.Id,
            ChildRoleId = newChildRole.Id,
            Level = 2
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.ChildRoleId.Should().Be(newChildRole.Id);
        result.Level.Should().Be(2);
    }

    [Fact]
    public async Task Handle_MappingNotFound_ThrowsInvalidOperationException()
    {
        // Arrange
        var hierarchies = new List<RoleHierarchyEntity>();
        var mockHierarchies = hierarchies.AsQueryable().BuildMockDbSet();

        DbContextMock.Setup(x => x.RoleHierarchies).Returns(mockHierarchies.Object);

        var command = new UpdateRoleHierarchyMappingCommand
        {
            Id = Guid.NewGuid(),
            ParentRoleId = Guid.NewGuid(),
            ChildRoleId = Guid.NewGuid(),
            Level = 1
        };

        // Act & Assert
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*not found*");
    }

    [Fact]
    public async Task Handle_NoChanges_ThrowsInvalidOperationException()
    {
        // Arrange
        var mappingId = Guid.NewGuid();
        var departmentId = Guid.NewGuid();
        var department = CreateTestDepartment(id: departmentId, name: "IT");
        var parentRole = CreateTestRole(name: "Manager", departmentId: departmentId);
        parentRole.Department = department;
        var childRole = CreateTestRole(name: "Developer", departmentId: departmentId);
        childRole.Department = department;

        var existingHierarchy = new RoleHierarchyEntity
        {
            Id = mappingId,
            DepartmentId = departmentId,
            Department = department,
            ParentRoleId = parentRole.Id,
            ParentRole = parentRole,
            ChildRoleId = childRole.Id,
            ChildRole = childRole,
            Level = 1,
            IsActive = true,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow
        };

        var roles = new List<ApplicationRole> { parentRole, childRole };
        var hierarchies = new List<RoleHierarchyEntity> { existingHierarchy };

        var mockRoles = roles.AsQueryable().BuildMockDbSet();
        var mockHierarchies = hierarchies.AsQueryable().BuildMockDbSet();

        DbContextMock.Setup(x => x.Roles).Returns(mockRoles.Object);
        DbContextMock.Setup(x => x.RoleHierarchies).Returns(mockHierarchies.Object);

        var command = new UpdateRoleHierarchyMappingCommand
        {
            Id = mappingId,
            ParentRoleId = parentRole.Id,
            ChildRoleId = childRole.Id,
            Level = 1 // Same values - no change
        };

        // Act & Assert
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*No changes detected*");
    }

    [Fact]
    public async Task Handle_CircularHierarchy_ThrowsInvalidOperationException()
    {
        // Arrange
        var mappingId = Guid.NewGuid();
        var departmentId = Guid.NewGuid();
        var department = CreateTestDepartment(id: departmentId, name: "IT");
        var role = CreateTestRole(name: "Manager", departmentId: departmentId);
        role.Department = department;

        var existingHierarchy = new RoleHierarchyEntity
        {
            Id = mappingId,
            DepartmentId = departmentId,
            Department = department,
            ParentRoleId = role.Id,
            ParentRole = role,
            ChildRoleId = Guid.NewGuid(),
            ChildRole = CreateTestRole(name: "Developer", departmentId: departmentId),
            Level = 1,
            IsActive = true,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow
        };

        var roles = new List<ApplicationRole> { role };
        var hierarchies = new List<RoleHierarchyEntity> { existingHierarchy };

        var mockRoles = roles.AsQueryable().BuildMockDbSet();
        var mockHierarchies = hierarchies.AsQueryable().BuildMockDbSet();

        DbContextMock.Setup(x => x.Roles).Returns(mockRoles.Object);
        DbContextMock.Setup(x => x.RoleHierarchies).Returns(mockHierarchies.Object);

        var command = new UpdateRoleHierarchyMappingCommand
        {
            Id = mappingId,
            ParentRoleId = role.Id,
            ChildRoleId = role.Id, // Same role - circular
            Level = 1
        };

        // Act & Assert
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*cannot be its own parent*");
    }
}

#endregion

#region DeleteRoleHierarchyMapping Tests

public class DeleteRoleHierarchyMappingCommandHandlerTests : ApplicationTestBase
{
    private readonly DeleteRoleHierarchyMappingCommandHandler _handler;

    public DeleteRoleHierarchyMappingCommandHandlerTests()
    {
        _handler = new DeleteRoleHierarchyMappingCommandHandler(DbContextMock.Object);
    }

    [Fact]
    public async Task Handle_ExistingMapping_ReturnsTrue()
    {
        // Arrange
        var mappingId = Guid.NewGuid();
        var hierarchy = new RoleHierarchyEntity
        {
            Id = mappingId,
            DepartmentId = Guid.NewGuid(),
            ParentRoleId = Guid.NewGuid(),
            ChildRoleId = Guid.NewGuid(),
            Level = 1,
            IsActive = true,
            IsDeleted = false
        };

        var hierarchies = new List<RoleHierarchyEntity> { hierarchy };
        var mockHierarchies = hierarchies.AsQueryable().BuildMockDbSet();

        DbContextMock.Setup(x => x.RoleHierarchies).Returns(mockHierarchies.Object);
        DbContextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var command = new DeleteRoleHierarchyMappingCommand(mappingId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_MappingNotFound_ThrowsInvalidOperationException()
    {
        // Arrange
        var hierarchies = new List<RoleHierarchyEntity>();
        var mockHierarchies = hierarchies.AsQueryable().BuildMockDbSet();

        DbContextMock.Setup(x => x.RoleHierarchies).Returns(mockHierarchies.Object);

        var command = new DeleteRoleHierarchyMappingCommand(Guid.NewGuid());

        // Act & Assert
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*not found*");
    }
}

#endregion
