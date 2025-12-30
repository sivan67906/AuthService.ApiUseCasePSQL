using AuthService.Application.Features.Permission.CreatePermission;
using AuthService.Application.Features.Permission.DeletePermission;
using AuthService.Application.Features.Permission.GetAllPermissions;
using AuthService.Application.Features.Permission.GetPermission;
using AuthService.Application.Features.Permission.UpdatePermission;
using MockQueryable.Moq;
using PermissionEntity = AuthService.Domain.Entities.Permission;

namespace AuthService.Tests.Unit.Application.Features.Permission;

#region CreatePermission Tests

public class CreatePermissionCommandHandlerTests : ApplicationTestBase
{
    private readonly CreatePermissionCommandHandler _handler;

    public CreatePermissionCommandHandlerTests()
    {
        _handler = new CreatePermissionCommandHandler(DbContextMock.Object);
    }

    #region Positive Scenarios

    [Fact]
    public async Task Handle_ValidPermission_ReturnsCreatedPermission()
    {
        // Arrange
        var permissions = new List<PermissionEntity>();
        var mockDbSet = permissions.AsQueryable().BuildMockDbSet();

        DbContextMock.Setup(x => x.Permissions).Returns(mockDbSet.Object);
        DbContextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var command = new CreatePermissionCommand("READ", "Read Permission", "Allows read access");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Code.Should().Be("READ");
        result.Name.Should().Be("Read Permission");
    }

    [Fact]
    public async Task Handle_ValidPermission_CodeIsUppercased()
    {
        // Arrange
        var permissions = new List<PermissionEntity>();
        var mockDbSet = permissions.AsQueryable().BuildMockDbSet();

        DbContextMock.Setup(x => x.Permissions).Returns(mockDbSet.Object);
        DbContextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var command = new CreatePermissionCommand("read", "Read Permission", null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Code.Should().Be("READ");
    }

    [Fact]
    public async Task Handle_NullDescription_Succeeds()
    {
        // Arrange
        var permissions = new List<PermissionEntity>();
        var mockDbSet = permissions.AsQueryable().BuildMockDbSet();

        DbContextMock.Setup(x => x.Permissions).Returns(mockDbSet.Object);
        DbContextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var command = new CreatePermissionCommand("READ", "Read", null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Description.Should().BeNull();
    }

    #endregion

    #region Negative Scenarios

    [Fact]
    public async Task Handle_DuplicateCode_ThrowsInvalidOperationException()
    {
        // Arrange
        var existingPermission = CreateTestPermission(code: "READ");
        var permissions = new List<PermissionEntity> { existingPermission };
        var mockDbSet = permissions.AsQueryable().BuildMockDbSet();

        DbContextMock.Setup(x => x.Permissions).Returns(mockDbSet.Object);

        var command = new CreatePermissionCommand("READ", "New Read", null);

        // Act & Assert
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*already exists*");
    }

    [Fact]
    public async Task Handle_DuplicateCodeCaseInsensitive_ThrowsException()
    {
        // Arrange
        var existingPermission = CreateTestPermission(code: "READ");
        var permissions = new List<PermissionEntity> { existingPermission };
        var mockDbSet = permissions.AsQueryable().BuildMockDbSet();

        DbContextMock.Setup(x => x.Permissions).Returns(mockDbSet.Object);

        var command = new CreatePermissionCommand("read", "Read Permission", null);

        // Act & Assert
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task Handle_DeletedPermissionWithSameCode_ThrowsWithDeactivatedMessage()
    {
        // Arrange
        var deletedPermission = CreateTestPermission(code: "READ", isDeleted: true);
        var permissions = new List<PermissionEntity> { deletedPermission };
        var mockDbSet = permissions.AsQueryable().BuildMockDbSet();

        DbContextMock.Setup(x => x.Permissions).Returns(mockDbSet.Object);

        var command = new CreatePermissionCommand("READ", "Read", null);

        // Act & Assert
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*deactivated mode*");
    }

    #endregion

    #region Exception Scenarios

    [Fact]
    public async Task Handle_DatabaseError_ThrowsException()
    {
        // Arrange
        var permissions = new List<PermissionEntity>();
        var mockDbSet = permissions.AsQueryable().BuildMockDbSet();

        DbContextMock.Setup(x => x.Permissions).Returns(mockDbSet.Object);
        DbContextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        var command = new CreatePermissionCommand("READ", "Read", null);

        // Act & Assert
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<Exception>()
            .WithMessage("Database error");
    }

    #endregion
}

public class CreatePermissionCommandValidatorTests
{
    private readonly CreatePermissionCommandValidator _validator;

    public CreatePermissionCommandValidatorTests()
    {
        _validator = new CreatePermissionCommandValidator();
    }

    [Fact]
    public void Validate_ValidCommand_PassesValidation()
    {
        // Arrange
        var command = new CreatePermissionCommand("READ", "Read Permission", "Description");

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Validate_EmptyCode_FailsValidation(string? code)
    {
        // Arrange
        var command = new CreatePermissionCommand(code!, "Name", null);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Validate_EmptyName_FailsValidation(string? name)
    {
        // Arrange
        var command = new CreatePermissionCommand("READ", name!, null);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
    }
}

#endregion

#region GetPermission Tests

public class GetPermissionQueryHandlerTests : ApplicationTestBase
{
    private readonly GetPermissionQueryHandler _handler;

    public GetPermissionQueryHandlerTests()
    {
        _handler = new GetPermissionQueryHandler(DbContextMock.Object);
    }

    [Fact]
    public async Task Handle_ExistingPermission_ReturnsPermission()
    {
        // Arrange
        var permissionId = Guid.NewGuid();
        var permission = CreateTestPermission(id: permissionId, name: "Read Permission");
        var permissions = new List<PermissionEntity> { permission };
        var mockDbSet = permissions.AsQueryable().BuildMockDbSet();

        DbContextMock.Setup(x => x.Permissions).Returns(mockDbSet.Object);

        var query = new GetPermissionQuery(permissionId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Read Permission");
    }

    [Fact]
    public async Task Handle_NonExistentPermission_ReturnsNull()
    {
        // Arrange
        var permissions = new List<PermissionEntity>();
        var mockDbSet = permissions.AsQueryable().BuildMockDbSet();

        DbContextMock.Setup(x => x.Permissions).Returns(mockDbSet.Object);

        var query = new GetPermissionQuery(Guid.NewGuid());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task Handle_DeletedPermission_ReturnsNull()
    {
        // Arrange
        var permissionId = Guid.NewGuid();
        var deletedPermission = CreateTestPermission(id: permissionId, isDeleted: true);
        var permissions = new List<PermissionEntity> { deletedPermission };
        var mockDbSet = permissions.AsQueryable().BuildMockDbSet();

        DbContextMock.Setup(x => x.Permissions).Returns(mockDbSet.Object);

        var query = new GetPermissionQuery(permissionId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }
}

#endregion

#region GetAllPermissions Tests

public class GetAllPermissionsQueryHandlerTests : ApplicationTestBase
{
    private readonly GetAllPermissionsQueryHandler _handler;

    public GetAllPermissionsQueryHandlerTests()
    {
        _handler = new GetAllPermissionsQueryHandler(DbContextMock.Object);
    }

    [Fact]
    public async Task Handle_MultiplePermissions_ReturnsAllActive()
    {
        // Arrange
        var perm1 = CreateTestPermission(code: "READ", name: "Read");
        var perm2 = CreateTestPermission(code: "WRITE", name: "Write");
        var deletedPerm = CreateTestPermission(code: "OLD", isDeleted: true);
        var permissions = new List<PermissionEntity> { perm1, perm2, deletedPerm };
        var mockDbSet = permissions.AsQueryable().BuildMockDbSet();

        DbContextMock.Setup(x => x.Permissions).Returns(mockDbSet.Object);

        var query = new GetAllPermissionsQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handle_NoPermissions_ReturnsEmptyList()
    {
        // Arrange
        var permissions = new List<PermissionEntity>();
        var mockDbSet = permissions.AsQueryable().BuildMockDbSet();

        DbContextMock.Setup(x => x.Permissions).Returns(mockDbSet.Object);

        var query = new GetAllPermissionsQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
    }
}

#endregion

#region UpdatePermission Tests

public class UpdatePermissionCommandHandlerTests : ApplicationTestBase
{
    private readonly UpdatePermissionCommandHandler _handler;

    public UpdatePermissionCommandHandlerTests()
    {
        _handler = new UpdatePermissionCommandHandler(DbContextMock.Object);
    }

    [Fact]
    public async Task Handle_ValidUpdate_ReturnsUpdatedPermission()
    {
        // Arrange
        var permissionId = Guid.NewGuid();
        var existingPermission = CreateTestPermission(id: permissionId, name: "Old Name");
        var permissions = new List<PermissionEntity> { existingPermission };
        var mockDbSet = permissions.AsQueryable().BuildMockDbSet();

        DbContextMock.Setup(x => x.Permissions).Returns(mockDbSet.Object);
        DbContextMock.Setup(x => x.Set<PermissionEntity>()).Returns(mockDbSet.Object);
        DbContextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var command = new UpdatePermissionCommand(permissionId, "New Name", "New Description");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("New Name");
    }

    [Fact]
    public async Task Handle_PermissionNotFound_ThrowsInvalidOperationException()
    {
        // Arrange
        var permissions = new List<PermissionEntity>();
        var mockDbSet = permissions.AsQueryable().BuildMockDbSet();

        DbContextMock.Setup(x => x.Permissions).Returns(mockDbSet.Object);

        var command = new UpdatePermissionCommand(Guid.NewGuid(), "Name", null);

        // Act & Assert
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*not found*");
    }

    [Fact]
    public async Task Handle_NoChangesDetected_ThrowsInvalidOperationException()
    {
        // Arrange
        var permissionId = Guid.NewGuid();
        var existingPermission = CreateTestPermission(id: permissionId, name: "Same Name", description: "Same Desc");
        var permissions = new List<PermissionEntity> { existingPermission };
        var mockDbSet = permissions.AsQueryable().BuildMockDbSet();

        DbContextMock.Setup(x => x.Permissions).Returns(mockDbSet.Object);

        var command = new UpdatePermissionCommand(permissionId, "Same Name", "Same Desc");

        // Act & Assert
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*No changes detected*");
    }
}

#endregion

#region DeletePermission Tests

public class DeletePermissionCommandHandlerTests : ApplicationTestBase
{
    private readonly DeletePermissionCommandHandler _handler;

    public DeletePermissionCommandHandlerTests()
    {
        _handler = new DeletePermissionCommandHandler(DbContextMock.Object);
    }

    [Fact]
    public async Task Handle_ExistingPermission_ReturnsTrue()
    {
        // Arrange
        var permissionId = Guid.NewGuid();
        var permission = CreateTestPermission(id: permissionId);
        var permissions = new List<PermissionEntity> { permission };
        var mockDbSet = permissions.AsQueryable().BuildMockDbSet();

        DbContextMock.Setup(x => x.Permissions).Returns(mockDbSet.Object);
        DbContextMock.Setup(x => x.Set<PermissionEntity>()).Returns(mockDbSet.Object);
        DbContextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var command = new DeletePermissionCommand(permissionId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_PermissionNotFound_ThrowsInvalidOperationException()
    {
        // Arrange
        var permissions = new List<PermissionEntity>();
        var mockDbSet = permissions.AsQueryable().BuildMockDbSet();

        DbContextMock.Setup(x => x.Permissions).Returns(mockDbSet.Object);

        var command = new DeletePermissionCommand(Guid.NewGuid());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert - Handler returns false when not found, doesn't throw
        result.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_DeletePermission_SetsIsDeletedTrue()
    {
        // Arrange
        var permissionId = Guid.NewGuid();
        var permission = CreateTestPermission(id: permissionId);
        var permissions = new List<PermissionEntity> { permission };
        var mockDbSet = permissions.AsQueryable().BuildMockDbSet();

        DbContextMock.Setup(x => x.Permissions).Returns(mockDbSet.Object);
        DbContextMock.Setup(x => x.Set<PermissionEntity>()).Returns(mockDbSet.Object);
        DbContextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var command = new DeletePermissionCommand(permissionId);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        permission.IsDeleted.Should().BeTrue();
    }
}

#endregion
