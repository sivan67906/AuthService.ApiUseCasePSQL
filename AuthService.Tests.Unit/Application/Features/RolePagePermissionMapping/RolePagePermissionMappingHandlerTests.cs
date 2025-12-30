using AuthService.Application.Features.RolePagePermissionMapping.CreateRolePagePermissionMapping;
using AuthService.Application.Features.RolePagePermissionMapping.DeleteRolePagePermissionMapping;
using AuthService.Application.Features.RolePagePermissionMapping.GetAllRolePagePermissionMappings;
using AuthService.Application.Features.RolePagePermissionMapping.GetRolePagePermissionMappingById;
using AuthService.Application.Features.RolePagePermissionMapping.GetRolePagePermissionMappingsByRole;
using AuthService.Application.Features.RolePagePermissionMapping.GetRolePagePermissionMappingsByRoleAndPage;
using AuthService.Application.Features.RolePagePermissionMapping.UpdateRolePagePermissionMapping;
using MockQueryable.Moq;
using RolePagePermissionMappingEntity = AuthService.Domain.Entities.RolePagePermissionMapping;

namespace AuthService.Tests.Unit.Application.Features.RolePagePermissionMapping;

#region CreateRolePagePermissionMapping Tests

public class CreateRolePagePermissionMappingCommandHandlerTests : ApplicationTestBase
{
    private readonly CreateRolePagePermissionMappingCommandHandler _handler;

    public CreateRolePagePermissionMappingCommandHandlerTests()
    {
        _handler = new CreateRolePagePermissionMappingCommandHandler(DbContextMock.Object);
    }

    #region Positive Scenarios

    [Fact]
    public async Task Handle_ValidMapping_ReturnsCreatedMapping()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        var pageId = Guid.NewGuid();
        var permissionId = Guid.NewGuid();
        var departmentId = Guid.NewGuid();
        var mappings = new List<RolePagePermissionMappingEntity>();
        var mockMappings = mappings.AsQueryable().BuildMockDbSet();

        DbContextMock.Setup(x => x.RolePagePermissionMappings).Returns(mockMappings.Object);
        DbContextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var command = new CreateRolePagePermissionMappingCommand(roleId, pageId, permissionId, departmentId, true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.RoleId.Should().Be(roleId);
        result.PageId.Should().Be(pageId);
        result.PermissionId.Should().Be(permissionId);
    }

    [Fact]
    public async Task Handle_MappingWithoutDepartment_Succeeds()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        var pageId = Guid.NewGuid();
        var permissionId = Guid.NewGuid();
        var mappings = new List<RolePagePermissionMappingEntity>();
        var mockMappings = mappings.AsQueryable().BuildMockDbSet();

        DbContextMock.Setup(x => x.RolePagePermissionMappings).Returns(mockMappings.Object);
        DbContextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var command = new CreateRolePagePermissionMappingCommand(roleId, pageId, permissionId, null, true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.DepartmentId.Should().BeNull();
    }

    #endregion

    #region Negative Scenarios

    [Fact]
    public async Task Handle_DuplicateMapping_ThrowsInvalidOperationException()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        var pageId = Guid.NewGuid();
        var permissionId = Guid.NewGuid();
        var existingMapping = new RolePagePermissionMappingEntity
        {
            Id = Guid.NewGuid(),
            RoleId = roleId,
            PageId = pageId,
            PermissionId = permissionId,
            IsDeleted = false
        };
        var mappings = new List<RolePagePermissionMappingEntity> { existingMapping };
        var mockMappings = mappings.AsQueryable().BuildMockDbSet();

        DbContextMock.Setup(x => x.RolePagePermissionMappings).Returns(mockMappings.Object);

        var command = new CreateRolePagePermissionMappingCommand(roleId, pageId, permissionId, null, true);

        // Act & Assert
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*already exists*");
    }

    [Fact]
    public async Task Handle_DeletedMappingWithSameKeys_ThrowsWithDeactivatedMessage()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        var pageId = Guid.NewGuid();
        var permissionId = Guid.NewGuid();
        var deletedMapping = new RolePagePermissionMappingEntity
        {
            Id = Guid.NewGuid(),
            RoleId = roleId,
            PageId = pageId,
            PermissionId = permissionId,
            IsDeleted = true
        };
        var mappings = new List<RolePagePermissionMappingEntity> { deletedMapping };
        var mockMappings = mappings.AsQueryable().BuildMockDbSet();

        DbContextMock.Setup(x => x.RolePagePermissionMappings).Returns(mockMappings.Object);

        var command = new CreateRolePagePermissionMappingCommand(roleId, pageId, permissionId, null, true);

        // Act & Assert
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*deactivated mode*");
    }

    #endregion
}

public class CreateRolePagePermissionMappingCommandValidatorTests
{
    private readonly CreateRolePagePermissionMappingCommandValidator _validator;

    public CreateRolePagePermissionMappingCommandValidatorTests()
    {
        _validator = new CreateRolePagePermissionMappingCommandValidator();
    }

    [Fact]
    public void Validate_ValidCommand_PassesValidation()
    {
        var command = new CreateRolePagePermissionMappingCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), true);
        var result = _validator.Validate(command);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_EmptyRoleId_FailsValidation()
    {
        var command = new CreateRolePagePermissionMappingCommand(Guid.Empty, Guid.NewGuid(), Guid.NewGuid(), null, true);
        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validate_EmptyPageId_FailsValidation()
    {
        var command = new CreateRolePagePermissionMappingCommand(Guid.NewGuid(), Guid.Empty, Guid.NewGuid(), null, true);
        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validate_EmptyPermissionId_FailsValidation()
    {
        var command = new CreateRolePagePermissionMappingCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.Empty, null, true);
        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
    }
}

#endregion

#region GetRolePagePermissionMappingById Tests

public class GetRolePagePermissionMappingByIdQueryHandlerTests : ApplicationTestBase
{
    private readonly GetRolePagePermissionMappingByIdQueryHandler _handler;

    public GetRolePagePermissionMappingByIdQueryHandlerTests()
    {
        _handler = new GetRolePagePermissionMappingByIdQueryHandler(DbContextMock.Object);
    }

    [Fact]
    public async Task Handle_ExistingMapping_ReturnsMapping()
    {
        // Arrange
        var mappingId = Guid.NewGuid();
        var role = CreateTestRole(name: "Admin");
        var page = CreateTestPage(name: "Users");
        var permission = CreateTestPermission(code: "READ");
        var mapping = new RolePagePermissionMappingEntity
        {
            Id = mappingId,
            RoleId = role.Id,
            PageId = page.Id,
            PermissionId = permission.Id,
            Role = role,
            Page = page,
            Permission = permission,
            IsDeleted = false
        };
        var mappings = new List<RolePagePermissionMappingEntity> { mapping };
        var mockMappings = mappings.AsQueryable().BuildMockDbSet();

        DbContextMock.Setup(x => x.RolePagePermissionMappings).Returns(mockMappings.Object);

        var query = new GetRolePagePermissionMappingByIdQuery(mappingId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(mappingId);
    }

    [Fact]
    public async Task Handle_NonExistentMapping_ReturnsNull()
    {
        // Arrange
        var mappings = new List<RolePagePermissionMappingEntity>();
        var mockMappings = mappings.AsQueryable().BuildMockDbSet();

        DbContextMock.Setup(x => x.RolePagePermissionMappings).Returns(mockMappings.Object);

        var query = new GetRolePagePermissionMappingByIdQuery(Guid.NewGuid());

        // Act & Assert
        var act = async () => await _handler.Handle(query, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("*not found*");
    }

    [Fact]
    public async Task Handle_DeletedMapping_ReturnsNull()
    {
        // Arrange
        var mappingId = Guid.NewGuid();
        var deletedMapping = new RolePagePermissionMappingEntity
        {
            Id = mappingId,
            RoleId = Guid.NewGuid(),
            PageId = Guid.NewGuid(),
            PermissionId = Guid.NewGuid(),
            IsDeleted = true
        };
        var mappings = new List<RolePagePermissionMappingEntity> { deletedMapping };
        var mockMappings = mappings.AsQueryable().BuildMockDbSet();

        DbContextMock.Setup(x => x.RolePagePermissionMappings).Returns(mockMappings.Object);

        var query = new GetRolePagePermissionMappingByIdQuery(mappingId);

        // Act & Assert
        var act = async () => await _handler.Handle(query, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("*not found*");
    }
}

#endregion

#region GetAllRolePagePermissionMappings Tests

public class GetAllRolePagePermissionMappingsQueryHandlerTests : ApplicationTestBase
{
    private readonly GetAllRolePagePermissionMappingsQueryHandler _handler;

    public GetAllRolePagePermissionMappingsQueryHandlerTests()
    {
        _handler = new GetAllRolePagePermissionMappingsQueryHandler(DbContextMock.Object);
    }

    [Fact]
    public async Task Handle_MultipleMappings_ReturnsAllActive()
    {
        // Arrange
        var role = CreateTestRole(name: "Admin");
        var page = CreateTestPage(name: "Users");
        var permission = CreateTestPermission(code: "READ");
        var permission2 = CreateTestPermission(code: "WRITE");
        var role2 = CreateTestRole(name: "Other");
        var page2 = CreateTestPage(name: "Settings");
        var permission3 = CreateTestPermission(code: "DELETE");
        var mappings = new List<RolePagePermissionMappingEntity> {
            new() { Id = Guid.NewGuid(), RoleId = role.Id, PageId = page.Id, PermissionId = permission.Id, Role = role, Page = page, Permission = permission, IsDeleted = false },
            new() { Id = Guid.NewGuid(), RoleId = role.Id, PageId = page.Id, PermissionId = permission2.Id, Role = role, Page = page, Permission = permission2, IsDeleted = false },
            new() { Id = Guid.NewGuid(), RoleId = role2.Id, PageId = page2.Id, PermissionId = permission3.Id, Role = role2, Page = page2, Permission = permission3, IsDeleted = true }
        };
        var mockMappings = mappings.AsQueryable().BuildMockDbSet();

        DbContextMock.Setup(x => x.RolePagePermissionMappings).Returns(mockMappings.Object);

        var query = new GetAllRolePagePermissionMappingsQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handle_NoMappings_ReturnsEmptyList()
    {
        // Arrange
        var mappings = new List<RolePagePermissionMappingEntity>();
        var mockMappings = mappings.AsQueryable().BuildMockDbSet();

        DbContextMock.Setup(x => x.RolePagePermissionMappings).Returns(mockMappings.Object);

        var query = new GetAllRolePagePermissionMappingsQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
    }
}

#endregion

#region GetRolePagePermissionMappingsByRole Tests

public class GetRolePagePermissionMappingsByRoleQueryHandlerTests : ApplicationTestBase
{
    private readonly GetRolePagePermissionMappingsByRoleQueryHandler _handler;

    public GetRolePagePermissionMappingsByRoleQueryHandlerTests()
    {
        _handler = new GetRolePagePermissionMappingsByRoleQueryHandler(DbContextMock.Object);
    }

    [Fact]
    public async Task Handle_ValidRoleId_ReturnsMappings()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        var role = CreateTestRole(id: roleId, name: "Admin");
        var page1 = CreateTestPage(name: "Users");
        var page2 = CreateTestPage(name: "Settings");
        var permission1 = CreateTestPermission(code: "READ", name: "Read");
        var permission2 = CreateTestPermission(code: "WRITE", name: "Write");
        var otherRole = CreateTestRole(name: "Other");
        var otherPage = CreateTestPage(name: "Other");
        var otherPermission = CreateTestPermission(code: "DELETE", name: "Delete");
        var mappings = new List<RolePagePermissionMappingEntity> {
            new() { Id = Guid.NewGuid(), RoleId = roleId, PageId = page1.Id, PermissionId = permission1.Id, Role = role, Page = page1, Permission = permission1, IsDeleted = false },
            new() { Id = Guid.NewGuid(), RoleId = roleId, PageId = page2.Id, PermissionId = permission2.Id, Role = role, Page = page2, Permission = permission2, IsDeleted = false },
            new() { Id = Guid.NewGuid(), RoleId = otherRole.Id, PageId = otherPage.Id, PermissionId = otherPermission.Id, Role = otherRole, Page = otherPage, Permission = otherPermission, IsDeleted = false }
        };
        var mockMappings = mappings.AsQueryable().BuildMockDbSet();

        DbContextMock.Setup(x => x.RolePagePermissionMappings).Returns(mockMappings.Object);

        var query = new GetRolePagePermissionMappingsByRoleQuery(roleId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);
        result.Should().AllSatisfy(m => m.RoleId.Should().Be(roleId));
    }

    [Fact]
    public async Task Handle_NoMappingsForRole_ReturnsEmptyList()
    {
        // Arrange
        var mappings = new List<RolePagePermissionMappingEntity>();
        var mockMappings = mappings.AsQueryable().BuildMockDbSet();

        DbContextMock.Setup(x => x.RolePagePermissionMappings).Returns(mockMappings.Object);

        var query = new GetRolePagePermissionMappingsByRoleQuery(Guid.NewGuid());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
    }
}

#endregion

#region GetRolePagePermissionMappingsByRoleAndPage Tests

public class GetRolePagePermissionMappingsByRoleAndPageQueryHandlerTests : ApplicationTestBase
{
    private readonly GetRolePagePermissionMappingsByRoleAndPageQueryHandler _handler;

    public GetRolePagePermissionMappingsByRoleAndPageQueryHandlerTests()
    {
        _handler = new GetRolePagePermissionMappingsByRoleAndPageQueryHandler(DbContextMock.Object);
    }

    [Fact]
    public async Task Handle_ValidRoleIdAndPageId_ReturnsMappings()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        var pageId = Guid.NewGuid();
        var role = CreateTestRole(id: roleId, name: "Admin");
        var page = CreateTestPage(id: pageId, name: "Users");
        var permission1 = CreateTestPermission(code: "READ");
        var permission2 = CreateTestPermission(code: "WRITE");
        var otherPage = CreateTestPage(name: "Other");
        var otherPermission = CreateTestPermission(code: "DELETE");
        var mappings = new List<RolePagePermissionMappingEntity> {
            new() { Id = Guid.NewGuid(), RoleId = roleId, PageId = pageId, PermissionId = permission1.Id, Role = role, Page = page, Permission = permission1, IsDeleted = false },
            new() { Id = Guid.NewGuid(), RoleId = roleId, PageId = pageId, PermissionId = permission2.Id, Role = role, Page = page, Permission = permission2, IsDeleted = false },
            new() { Id = Guid.NewGuid(), RoleId = roleId, PageId = otherPage.Id, PermissionId = otherPermission.Id, Role = role, Page = otherPage, Permission = otherPermission, IsDeleted = false }
        };
        var mockMappings = mappings.AsQueryable().BuildMockDbSet();

        DbContextMock.Setup(x => x.RolePagePermissionMappings).Returns(mockMappings.Object);

        var query = new GetRolePagePermissionMappingsByRoleAndPageQuery(roleId, pageId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);
        result.Should().AllSatisfy(m =>
        {
            m.RoleId.Should().Be(roleId);
            m.PageId.Should().Be(pageId);
        });
    }
}

#endregion

#region DeleteRolePagePermissionMapping Tests

public class DeleteRolePagePermissionMappingCommandHandlerTests : ApplicationTestBase
{
    private readonly DeleteRolePagePermissionMappingCommandHandler _handler;

    public DeleteRolePagePermissionMappingCommandHandlerTests()
    {
        _handler = new DeleteRolePagePermissionMappingCommandHandler(DbContextMock.Object);
    }

    [Fact]
    public async Task Handle_ExistingMapping_ReturnsTrue()
    {
        // Arrange
        var mappingId = Guid.NewGuid();
        var permission = CreateTestPermission(name: "Read"); // Non-View permission
        var mapping = new RolePagePermissionMappingEntity
        {
            Id = mappingId,
            RoleId = Guid.NewGuid(),
            PageId = Guid.NewGuid(),
            PermissionId = permission.Id,
            Permission = permission,
            IsDeleted = false
        };
        var mappings = new List<RolePagePermissionMappingEntity> { mapping };
        var mockMappings = mappings.AsQueryable().BuildMockDbSet();

        DbContextMock.Setup(x => x.RolePagePermissionMappings).Returns(mockMappings.Object);
        DbContextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var command = new DeleteRolePagePermissionMappingCommand(mappingId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_MappingNotFound_ThrowsKeyNotFoundException()
    {
        // Arrange
        var mappings = new List<RolePagePermissionMappingEntity>();
        var mockMappings = mappings.AsQueryable().BuildMockDbSet();

        DbContextMock.Setup(x => x.RolePagePermissionMappings).Returns(mockMappings.Object);

        var command = new DeleteRolePagePermissionMappingCommand(Guid.NewGuid());

        // Act & Assert
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("*not found*");
    }
}

#endregion

#region UpdateRolePagePermissionMapping Tests

public class UpdateRolePagePermissionMappingCommandHandlerTests : ApplicationTestBase
{
    private readonly UpdateRolePagePermissionMappingCommandHandler _handler;

    public UpdateRolePagePermissionMappingCommandHandlerTests()
    {
        _handler = new UpdateRolePagePermissionMappingCommandHandler(DbContextMock.Object);
    }

    [Fact]
    public async Task Handle_ValidUpdate_ReturnsUpdatedMapping()
    {
        // Arrange
        var mappingId = Guid.NewGuid();
        var existingMapping = new RolePagePermissionMappingEntity
        {
            Id = mappingId,
            RoleId = Guid.NewGuid(),
            PageId = Guid.NewGuid(),
            PermissionId = Guid.NewGuid(),
            IsActive = true,
            IsDeleted = false
        };
        var mappings = new List<RolePagePermissionMappingEntity> { existingMapping };
        var mockMappings = mappings.AsQueryable().BuildMockDbSet();

        DbContextMock.Setup(x => x.RolePagePermissionMappings).Returns(mockMappings.Object);
        DbContextMock.Setup(x => x.Set<RolePagePermissionMappingEntity>()).Returns(mockMappings.Object);
        DbContextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var command = new UpdateRolePagePermissionMappingCommand(mappingId, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), null, false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_MappingNotFound_ThrowsInvalidOperationException()
    {
        // Arrange
        var mappings = new List<RolePagePermissionMappingEntity>();
        var mockMappings = mappings.AsQueryable().BuildMockDbSet();

        DbContextMock.Setup(x => x.RolePagePermissionMappings).Returns(mockMappings.Object);

        var command = new UpdateRolePagePermissionMappingCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), null, true);

        // Act & Assert
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*not found*");
    }
}

#endregion
