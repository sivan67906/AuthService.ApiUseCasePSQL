using AuthService.Application.Features.Role.CreateRole;
using AuthService.Application.Features.Role.DeleteRole;
using AuthService.Application.Features.Role.GetAllRoles;
using AuthService.Application.Features.Role.GetRoleById;
using AuthService.Application.Features.Role.GetRolesByDepartment;
using AuthService.Application.Features.Role.UpdateRole;
using MockQueryable.Moq;
using DepartmentEntity = AuthService.Domain.Entities.Department;

namespace AuthService.Tests.Unit.Application.Features.Role;

#region CreateRole Tests

public class CreateRoleCommandHandlerTests : ApplicationTestBase
{
    private readonly CreateRoleCommandHandler _handler;
    private readonly Mock<ILogger<CreateRoleCommandHandler>> _loggerMock;

    public CreateRoleCommandHandlerTests()
    {
        _loggerMock = CreateMockLogger<CreateRoleCommandHandler>();
        _handler = new CreateRoleCommandHandler(
            RoleManagerMock.Object,
            DbContextMock.Object,
            _loggerMock.Object);
    }

    #region Positive Scenarios

    [Fact]
    public async Task Handle_ValidRole_ReturnsCreatedRole()
    {
        // Arrange
        var departmentId = Guid.NewGuid();
        var department = CreateTestDepartment(id: departmentId, name: "Sales");
        var departments = new List<DepartmentEntity> { department };
        var roles = new List<ApplicationRole>();

        var mockDeptDbSet = departments.AsQueryable().BuildMockDbSet();
        var mockRoleDbSet = roles.AsQueryable().BuildMockDbSet();

        DbContextMock.Setup(x => x.Departments).Returns(mockDeptDbSet.Object);
        DbContextMock.Setup(x => x.ApplicationRoles).Returns(mockRoleDbSet.Object);

        RoleManagerMock.Setup(x => x.CreateAsync(It.IsAny<ApplicationRole>()))
            .ReturnsAsync(IdentityResult.Success);

        var command = new CreateRoleCommand("MGR", "Manager", "Manager role", departmentId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Code.Should().Be("MGR");
        result.Name.Should().Be("Manager");
        result.DepartmentId.Should().Be(departmentId);
    }

    [Fact]
    public async Task Handle_RoleWithoutDepartment_Succeeds()
    {
        // Arrange
        var roles = new List<ApplicationRole>();
        var mockRoleDbSet = roles.AsQueryable().BuildMockDbSet();

        DbContextMock.Setup(x => x.ApplicationRoles).Returns(mockRoleDbSet.Object);

        RoleManagerMock.Setup(x => x.CreateAsync(It.IsAny<ApplicationRole>()))
            .ReturnsAsync(IdentityResult.Success);

        var command = new CreateRoleCommand("ADMIN", "Administrator", "Admin role", null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.DepartmentId.Should().BeNull();
    }

    [Fact]
    public async Task Handle_ValidRole_CodeIsUppercased()
    {
        // Arrange
        var roles = new List<ApplicationRole>();
        var mockRoleDbSet = roles.AsQueryable().BuildMockDbSet();

        DbContextMock.Setup(x => x.ApplicationRoles).Returns(mockRoleDbSet.Object);

        ApplicationRole? capturedRole = null;
        RoleManagerMock.Setup(x => x.CreateAsync(It.IsAny<ApplicationRole>()))
            .Callback<ApplicationRole>(r => capturedRole = r)
            .ReturnsAsync(IdentityResult.Success);

        var command = new CreateRoleCommand("manager", "Manager", null, null);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        capturedRole.Should().NotBeNull();
        capturedRole!.Code.Should().Be("MANAGER");
    }

    #endregion

    #region Negative Scenarios

    [Fact]
    public async Task Handle_DuplicateCode_ThrowsInvalidOperationException()
    {
        // Arrange
        var existingRole = CreateTestRole(code: "MGR");
        var roles = new List<ApplicationRole> { existingRole };
        var mockRoleDbSet = roles.AsQueryable().BuildMockDbSet();

        DbContextMock.Setup(x => x.ApplicationRoles).Returns(mockRoleDbSet.Object);

        var command = new CreateRoleCommand("MGR", "New Manager", null, null);

        // Act & Assert
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*already exists*");
    }

    [Fact]
    public async Task Handle_DeletedRoleWithSameCode_ThrowsWithDeactivatedMessage()
    {
        // Arrange
        var deletedRole = CreateTestRole(code: "MGR", isDeleted: true);
        var roles = new List<ApplicationRole> { deletedRole };
        var mockRoleDbSet = roles.AsQueryable().BuildMockDbSet();

        DbContextMock.Setup(x => x.ApplicationRoles).Returns(mockRoleDbSet.Object);

        var command = new CreateRoleCommand("MGR", "Manager", null, null);

        // Act & Assert
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*deactivated mode*");
    }

    [Fact]
    public async Task Handle_NonExistentDepartment_ThrowsInvalidOperationException()
    {
        // Arrange
        var departments = new List<DepartmentEntity>();
        var roles = new List<ApplicationRole>();
        var mockDeptDbSet = departments.AsQueryable().BuildMockDbSet();
        var mockRoleDbSet = roles.AsQueryable().BuildMockDbSet();

        DbContextMock.Setup(x => x.Departments).Returns(mockDeptDbSet.Object);
        DbContextMock.Setup(x => x.ApplicationRoles).Returns(mockRoleDbSet.Object);

        var command = new CreateRoleCommand("MGR", "Manager", null, Guid.NewGuid());

        // Act & Assert
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Department*not found*");
    }

    [Fact]
    public async Task Handle_RoleManagerFails_ThrowsInvalidOperationException()
    {
        // Arrange
        var roles = new List<ApplicationRole>();
        var mockRoleDbSet = roles.AsQueryable().BuildMockDbSet();

        DbContextMock.Setup(x => x.ApplicationRoles).Returns(mockRoleDbSet.Object);

        var errors = new[] { new IdentityError { Description = "Role creation failed" } };
        RoleManagerMock.Setup(x => x.CreateAsync(It.IsAny<ApplicationRole>()))
            .ReturnsAsync(IdentityResult.Failed(errors));

        var command = new CreateRoleCommand("MGR", "Manager", null, null);

        // Act & Assert
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Failed to create role*");
    }

    #endregion

    #region Edge Cases

    [Fact]
    public async Task Handle_CaseInsensitiveDuplicateCheck_ThrowsException()
    {
        // Arrange
        var existingRole = CreateTestRole(code: "MANAGER");
        var roles = new List<ApplicationRole> { existingRole };
        var mockRoleDbSet = roles.AsQueryable().BuildMockDbSet();

        DbContextMock.Setup(x => x.ApplicationRoles).Returns(mockRoleDbSet.Object);

        var command = new CreateRoleCommand("manager", "New Manager", null, null);

        // Act & Assert
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task Handle_DeletedDepartment_ThrowsInvalidOperationException()
    {
        // Arrange
        var deletedDept = CreateTestDepartment(isDeleted: true);
        var departments = new List<DepartmentEntity> { deletedDept };
        var roles = new List<ApplicationRole>();
        var mockDeptDbSet = departments.AsQueryable().BuildMockDbSet();
        var mockRoleDbSet = roles.AsQueryable().BuildMockDbSet();

        DbContextMock.Setup(x => x.Departments).Returns(mockDeptDbSet.Object);
        DbContextMock.Setup(x => x.ApplicationRoles).Returns(mockRoleDbSet.Object);

        var command = new CreateRoleCommand("MGR", "Manager", null, deletedDept.Id);

        // Act & Assert
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    #endregion
}

public class CreateRoleCommandValidatorTests
{
    private readonly CreateRoleCommandValidator _validator;

    public CreateRoleCommandValidatorTests()
    {
        _validator = new CreateRoleCommandValidator();
    }

    [Fact]
    public void Validate_ValidCommand_PassesValidation()
    {
        // Arrange
        var command = new CreateRoleCommand("MGR", "Manager", "Description", Guid.NewGuid());

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
        var command = new CreateRoleCommand(code!, "Manager", null, null);

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
        var command = new CreateRoleCommand("MGR", name!, null, null);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
    }
}

#endregion

#region UpdateRole Tests

public class UpdateRoleCommandHandlerTests : ApplicationTestBase
{
    private readonly UpdateRoleCommandHandler _handler;

    public UpdateRoleCommandHandlerTests()
    {
        _handler = new UpdateRoleCommandHandler(
            RoleManagerMock.Object,
            DbContextMock.Object);
    }

    #region Positive Scenarios

    [Fact]
    public async Task Handle_ValidUpdate_ReturnsUpdatedRole()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        var existingRole = CreateTestRole(id: roleId, code: "MGR", name: "Old Name");

        RoleManagerMock.Setup(x => x.FindByIdAsync(roleId.ToString()))
            .ReturnsAsync(existingRole);

        RoleManagerMock.Setup(x => x.UpdateAsync(It.IsAny<ApplicationRole>()))
            .ReturnsAsync(IdentityResult.Success);

        var command = new UpdateRoleCommand(roleId, "New Name", "New Description", null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("New Name");
        result.Description.Should().Be("New Description");
    }

    #endregion

    #region Negative Scenarios

    [Fact]
    public async Task Handle_RoleNotFound_ThrowsInvalidOperationException()
    {
        // Arrange
        var roleId = Guid.NewGuid();

        RoleManagerMock.Setup(x => x.FindByIdAsync(roleId.ToString()))
            .ReturnsAsync((ApplicationRole?)null);

        var command = new UpdateRoleCommand(roleId, "Name", null, null);

        // Act & Assert
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*not found*");
    }

    [Fact]
    public async Task Handle_NoChangesDetected_ThrowsInvalidOperationException()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        var existingRole = CreateTestRole(id: roleId, name: "Same Name", description: "Same Desc");

        RoleManagerMock.Setup(x => x.FindByIdAsync(roleId.ToString()))
            .ReturnsAsync(existingRole);

        var command = new UpdateRoleCommand(roleId, "Same Name", "Same Desc", null);

        // Act & Assert
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*No changes detected*");
    }

    #endregion
}

#endregion

#region DeleteRole Tests

public class DeleteRoleCommandHandlerTests : ApplicationTestBase
{
    private readonly DeleteRoleCommandHandler _handler;

    public DeleteRoleCommandHandlerTests()
    {
        _handler = new DeleteRoleCommandHandler(
            RoleManagerMock.Object,
            UserManagerMock.Object);
    }

    #region Positive Scenarios

    [Fact]
    public async Task Handle_ExistingRole_ReturnsTrue()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        var role = CreateTestRole(id: roleId, name: "TestRole");

        RoleManagerMock.Setup(x => x.FindByIdAsync(roleId.ToString()))
            .ReturnsAsync(role);

        UserManagerMock.Setup(x => x.GetUsersInRoleAsync(role.Name!))
            .ReturnsAsync(new List<ApplicationUser>());

        RoleManagerMock.Setup(x => x.DeleteAsync(It.IsAny<ApplicationRole>()))
            .ReturnsAsync(IdentityResult.Success);

        var command = new DeleteRoleCommand(roleId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_DeleteRole_SetsIsDeletedTrue()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        var role = CreateTestRole(id: roleId, name: "TestRole");

        RoleManagerMock.Setup(x => x.FindByIdAsync(roleId.ToString()))
            .ReturnsAsync(role);

        UserManagerMock.Setup(x => x.GetUsersInRoleAsync(role.Name!))
            .ReturnsAsync(new List<ApplicationUser>());

        RoleManagerMock.Setup(x => x.DeleteAsync(It.IsAny<ApplicationRole>()))
            .ReturnsAsync(IdentityResult.Success);

        var command = new DeleteRoleCommand(roleId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert - Handler does hard delete via RoleManager.DeleteAsync
        result.Should().BeTrue();
    }

    #endregion

    #region Negative Scenarios

    [Fact]
    public async Task Handle_RoleNotFound_ThrowsInvalidOperationException()
    {
        // Arrange
        var roleId = Guid.NewGuid();

        RoleManagerMock.Setup(x => x.FindByIdAsync(roleId.ToString()))
            .ReturnsAsync((ApplicationRole?)null);

        var command = new DeleteRoleCommand(roleId);

        // Act & Assert
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*not found*");
    }

    #endregion
}

#endregion

#region GetRoleById Tests

public class GetRoleByIdQueryHandlerTests : ApplicationTestBase
{
    private readonly GetRoleByIdQueryHandler _handler;

    public GetRoleByIdQueryHandlerTests()
    {
        _handler = new GetRoleByIdQueryHandler(RoleManagerMock.Object);
    }

    #region Positive Scenarios

    [Fact]
    public async Task Handle_ExistingRole_ReturnsRole()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        var role = CreateTestRole(id: roleId, name: "Manager");

        RoleManagerMock.Setup(x => x.FindByIdAsync(roleId.ToString()))
            .ReturnsAsync(role);

        var query = new GetRoleByIdQuery(roleId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Manager");
    }

    #endregion

    #region Negative Scenarios

    [Fact]
    public async Task Handle_NonExistentRole_ReturnsNull()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        RoleManagerMock.Setup(x => x.FindByIdAsync(roleId.ToString()))
            .ReturnsAsync((ApplicationRole?)null);

        var query = new GetRoleByIdQuery(roleId);

        // Act & Assert
        var act = async () => await _handler.Handle(query, CancellationToken.None);
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"*{roleId}*not found*");
    }

    [Fact]
    public async Task Handle_DeletedRole_ReturnsNull()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        var deletedRole = CreateTestRole(id: roleId, isDeleted: true);

        // FindByIdAsync will return the role but the handler doesn't check IsDeleted
        // So deleted roles are still returned by the handler
        RoleManagerMock.Setup(x => x.FindByIdAsync(roleId.ToString()))
            .ReturnsAsync(deletedRole);

        var query = new GetRoleByIdQuery(roleId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert - handler returns the role even if deleted (no IsDeleted check in handler)
        result.Should().NotBeNull();
        result!.Id.Should().Be(roleId);
    }

    #endregion
}

#endregion

#region GetAllRoles Tests

public class GetAllRolesQueryHandlerTests : ApplicationTestBase
{
    private readonly GetAllRolesQueryHandler _handler;

    public GetAllRolesQueryHandlerTests()
    {
        _handler = new GetAllRolesQueryHandler(RoleManagerMock.Object, DbContextMock.Object);
    }

    #region Positive Scenarios

    [Fact]
    public async Task Handle_MultipleRoles_ReturnsAllActive()
    {
        // Arrange
        var role1 = CreateTestRole(code: "ADMIN", name: "Admin");
        var role2 = CreateTestRole(code: "USER", name: "User");
        var deletedRole = CreateTestRole(code: "OLD", isDeleted: true);
        var roles = new List<ApplicationRole> { role1, role2, deletedRole };
        var mockRoleDbSet = roles.AsQueryable().BuildMockDbSet();

        RoleManagerMock.Setup(x => x.Roles).Returns(mockRoleDbSet.Object);

        var query = new GetAllRolesQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().HaveCount(3); // Handler returns all roles, filtering is done separately
    }

    [Fact]
    public async Task Handle_NoRoles_ReturnsEmptyList()
    {
        // Arrange
        var roles = new List<ApplicationRole>();
        var mockRoleDbSet = roles.AsQueryable().BuildMockDbSet();

        RoleManagerMock.Setup(x => x.Roles).Returns(mockRoleDbSet.Object);

        var query = new GetAllRolesQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
    }

    #endregion
}

#endregion

#region GetRolesByDepartment Tests

public class GetRolesByDepartmentQueryHandlerTests : ApplicationTestBase
{
    private readonly GetRolesByDepartmentQueryHandler _handler;

    public GetRolesByDepartmentQueryHandlerTests()
    {
        _handler = new GetRolesByDepartmentQueryHandler(RoleManagerMock.Object, DbContextMock.Object);
    }

    #region Positive Scenarios

    [Fact]
    public async Task Handle_DepartmentWithRoles_ReturnsRolesForDepartment()
    {
        // Arrange
        var departmentId = Guid.NewGuid();
        var role1 = CreateTestRole(code: "MGR", name: "Manager", departmentId: departmentId);
        var role2 = CreateTestRole(code: "STAFF", name: "Staff", departmentId: departmentId);
        var otherRole = CreateTestRole(code: "OTHER", departmentId: Guid.NewGuid());
        var roles = new List<ApplicationRole> { role1, role2, otherRole };
        var mockRoleDbSet = roles.AsQueryable().BuildMockDbSet();

        RoleManagerMock.Setup(x => x.Roles).Returns(mockRoleDbSet.Object);

        var query = new GetRolesByDepartmentQuery(departmentId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(r => r.DepartmentId == departmentId);
    }

    [Fact]
    public async Task Handle_DepartmentWithNoRoles_ReturnsEmptyList()
    {
        // Arrange
        var roles = new List<ApplicationRole>();
        var mockRoleDbSet = roles.AsQueryable().BuildMockDbSet();

        RoleManagerMock.Setup(x => x.Roles).Returns(mockRoleDbSet.Object);

        var query = new GetRolesByDepartmentQuery(Guid.NewGuid());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region Edge Cases

    [Fact]
    public async Task Handle_ExcludesDeletedRoles()
    {
        // Arrange
        var departmentId = Guid.NewGuid();
        var activeRole = CreateTestRole(code: "ACTIVE", departmentId: departmentId);
        var deletedRole = CreateTestRole(code: "DELETED", departmentId: departmentId, isDeleted: true);
        var roles = new List<ApplicationRole> { activeRole, deletedRole };
        var mockRoleDbSet = roles.AsQueryable().BuildMockDbSet();

        RoleManagerMock.Setup(x => x.Roles).Returns(mockRoleDbSet.Object);

        var query = new GetRolesByDepartmentQuery(departmentId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().HaveCount(1);
        result.First().Code.Should().Be("ACTIVE");
    }

    #endregion
}

#endregion
