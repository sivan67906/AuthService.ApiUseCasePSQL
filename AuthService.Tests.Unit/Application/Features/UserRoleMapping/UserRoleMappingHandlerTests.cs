using AuthService.Application.Features.UserRoleMapping.CreateUserRoleMapping;
using AuthService.Application.Features.UserRoleMapping.DeleteUserRoleMapping;
using AuthService.Application.Features.UserRoleMapping.GetAllUserRoleMappings;
using AuthService.Application.Features.UserRoleMapping.GetUserRoleMappingById;
using AuthService.Application.Features.UserRoleMapping.GetUsersWithoutRoles;
using MockQueryable.Moq;
using DepartmentEntity = AuthService.Domain.Entities.Department;
using UserRoleMappingEntity = AuthService.Domain.Entities.UserRoleMapping;

namespace AuthService.Tests.Unit.Application.Features.UserRoleMapping;

#region CreateUserRoleMapping Tests

public class CreateUserRoleMappingCommandHandlerTests : ApplicationTestBase
{
    private readonly CreateUserRoleMappingCommandHandler _handler;

    public CreateUserRoleMappingCommandHandlerTests()
    {
        _handler = new CreateUserRoleMappingCommandHandler(
            DbContextMock.Object,
            UserManagerMock.Object,
            RoleManagerMock.Object);
    }

    [Fact]
    public async Task Handle_ValidMapping_ReturnsCreatedMapping()
    {
        var userId = Guid.NewGuid();
        var roleId = Guid.NewGuid();
        var departmentId = Guid.NewGuid();
        var user = CreateTestUser(id: userId);
        var role = CreateTestRole(id: roleId, name: "Manager");
        var department = CreateTestDepartment(id: departmentId, name: "Sales");

        var users = new List<ApplicationUser> { user };
        var roles = new List<ApplicationRole> { role };
        var departments = new List<DepartmentEntity> { department };
        var userRoleMappings = new List<UserRoleMappingEntity>();

        var mockUsers = users.AsQueryable().BuildMockDbSet();
        var mockRoles = roles.AsQueryable().BuildMockDbSet();
        var mockDepartments = departments.AsQueryable().BuildMockDbSet();
        var mockMappings = userRoleMappings.AsQueryable().BuildMockDbSet();

        DbContextMock.Setup(x => x.Users).Returns(mockUsers.Object);
        DbContextMock.Setup(x => x.Roles).Returns(mockRoles.Object);
        DbContextMock.Setup(x => x.Departments).Returns(mockDepartments.Object);
        DbContextMock.Setup(x => x.UserRoleMappings).Returns(mockMappings.Object);
        DbContextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        UserManagerMock.Setup(x => x.AddToRoleAsync(user, role.Name!))
            .ReturnsAsync(IdentityResult.Success);

        var command = new CreateUserRoleMappingCommand
        {
            UserId = userId,
            RoleId = roleId,
            DepartmentId = departmentId,
            AssignedByEmail = "admin@example.com"
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.UserId.Should().Be(userId);
        result.RoleId.Should().Be(roleId);
        result.DepartmentId.Should().Be(departmentId);
    }

    [Fact]
    public async Task Handle_MappingWithoutDepartment_Succeeds()
    {
        var userId = Guid.NewGuid();
        var roleId = Guid.NewGuid();
        var user = CreateTestUser(id: userId);
        var role = CreateTestRole(id: roleId, name: "User");

        var users = new List<ApplicationUser> { user };
        var roles = new List<ApplicationRole> { role };
        var userRoleMappings = new List<UserRoleMappingEntity>();

        var mockUsers = users.AsQueryable().BuildMockDbSet();
        var mockRoles = roles.AsQueryable().BuildMockDbSet();
        var mockMappings = userRoleMappings.AsQueryable().BuildMockDbSet();

        DbContextMock.Setup(x => x.Users).Returns(mockUsers.Object);
        DbContextMock.Setup(x => x.Roles).Returns(mockRoles.Object);
        DbContextMock.Setup(x => x.UserRoleMappings).Returns(mockMappings.Object);
        DbContextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        UserManagerMock.Setup(x => x.AddToRoleAsync(user, role.Name!))
            .ReturnsAsync(IdentityResult.Success);

        var command = new CreateUserRoleMappingCommand
        {
            UserId = userId,
            RoleId = roleId,
            DepartmentId = null,
            AssignedByEmail = "admin@example.com"
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.DepartmentId.Should().BeNull();
    }

    [Fact]
    public async Task Handle_UserNotFound_ThrowsInvalidOperationException()
    {
        var users = new List<ApplicationUser>();
        var mockUsers = users.AsQueryable().BuildMockDbSet();

        DbContextMock.Setup(x => x.Users).Returns(mockUsers.Object);

        var command = new CreateUserRoleMappingCommand
        {
            UserId = Guid.NewGuid(),
            RoleId = Guid.NewGuid(),
            DepartmentId = Guid.NewGuid(),
            AssignedByEmail = "admin@example.com"
        };

        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*User*not found*");
    }

    [Fact]
    public async Task Handle_RoleNotFound_ThrowsInvalidOperationException()
    {
        var userId = Guid.NewGuid();
        var user = CreateTestUser(id: userId);

        var users = new List<ApplicationUser> { user };
        var roles = new List<ApplicationRole>();

        var mockUsers = users.AsQueryable().BuildMockDbSet();
        var mockRoles = roles.AsQueryable().BuildMockDbSet();

        DbContextMock.Setup(x => x.Users).Returns(mockUsers.Object);
        DbContextMock.Setup(x => x.Roles).Returns(mockRoles.Object);

        var command = new CreateUserRoleMappingCommand
        {
            UserId = userId,
            RoleId = Guid.NewGuid(),
            DepartmentId = Guid.NewGuid(),
            AssignedByEmail = "admin@example.com"
        };

        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Role*not found*");
    }

    [Fact]
    public async Task Handle_DepartmentNotFound_ThrowsInvalidOperationException()
    {
        var userId = Guid.NewGuid();
        var roleId = Guid.NewGuid();
        var user = CreateTestUser(id: userId);
        var role = CreateTestRole(id: roleId);

        var users = new List<ApplicationUser> { user };
        var roles = new List<ApplicationRole> { role };
        var departments = new List<DepartmentEntity>();

        var mockUsers = users.AsQueryable().BuildMockDbSet();
        var mockRoles = roles.AsQueryable().BuildMockDbSet();
        var mockDepartments = departments.AsQueryable().BuildMockDbSet();

        DbContextMock.Setup(x => x.Users).Returns(mockUsers.Object);
        DbContextMock.Setup(x => x.Roles).Returns(mockRoles.Object);
        DbContextMock.Setup(x => x.Departments).Returns(mockDepartments.Object);

        var command = new CreateUserRoleMappingCommand
        {
            UserId = userId,
            RoleId = roleId,
            DepartmentId = Guid.NewGuid(),
            AssignedByEmail = "admin@example.com"
        };

        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Department*not found*");
    }

    [Fact]
    public async Task Handle_DuplicateMapping_ThrowsInvalidOperationException()
    {
        var userId = Guid.NewGuid();
        var roleId = Guid.NewGuid();
        var departmentId = Guid.NewGuid();
        var user = CreateTestUser(id: userId);
        var role = CreateTestRole(id: roleId);
        var department = CreateTestDepartment(id: departmentId);

        var existingMapping = new UserRoleMappingEntity
        {
            UserId = userId,
            RoleId = roleId,
            DepartmentId = departmentId
        };

        var users = new List<ApplicationUser> { user };
        var roles = new List<ApplicationRole> { role };
        var departments = new List<DepartmentEntity> { department };
        var userRoleMappings = new List<UserRoleMappingEntity> { existingMapping };

        var mockUsers = users.AsQueryable().BuildMockDbSet();
        var mockRoles = roles.AsQueryable().BuildMockDbSet();
        var mockDepartments = departments.AsQueryable().BuildMockDbSet();
        var mockMappings = userRoleMappings.AsQueryable().BuildMockDbSet();

        DbContextMock.Setup(x => x.Users).Returns(mockUsers.Object);
        DbContextMock.Setup(x => x.Roles).Returns(mockRoles.Object);
        DbContextMock.Setup(x => x.Departments).Returns(mockDepartments.Object);
        DbContextMock.Setup(x => x.UserRoleMappings).Returns(mockMappings.Object);

        var command = new CreateUserRoleMappingCommand
        {
            UserId = userId,
            RoleId = roleId,
            DepartmentId = departmentId,
            AssignedByEmail = "admin@example.com"
        };

        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*already exists*");
    }
}

#endregion

#region DeleteUserRoleMapping Tests

public class DeleteUserRoleMappingCommandHandlerTests : ApplicationTestBase
{
    private readonly DeleteUserRoleMappingCommandHandler _handler;

    public DeleteUserRoleMappingCommandHandlerTests()
    {
        _handler = new DeleteUserRoleMappingCommandHandler(DbContextMock.Object);
    }

    [Fact]
    public async Task Handle_ExistingMapping_ReturnsTrue()
    {
        var mappingId = Guid.NewGuid();
        var mapping = new UserRoleMappingEntity { Id = mappingId };
        var mappings = new List<UserRoleMappingEntity> { mapping };
        var mockMappings = mappings.AsQueryable().BuildMockDbSet();

        DbContextMock.Setup(x => x.UserRoleMappings).Returns(mockMappings.Object);
        DbContextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var command = new DeleteUserRoleMappingCommand(mappingId);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_MappingNotFound_ThrowsInvalidOperationException()
    {
        var mappings = new List<UserRoleMappingEntity>();
        var mockMappings = mappings.AsQueryable().BuildMockDbSet();

        DbContextMock.Setup(x => x.UserRoleMappings).Returns(mockMappings.Object);

        var command = new DeleteUserRoleMappingCommand(Guid.NewGuid());

        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*not found*");
    }
}

#endregion

#region GetUserRoleMappingById Tests

public class GetUserRoleMappingByIdQueryHandlerTests : ApplicationTestBase
{
    private readonly GetUserRoleMappingByIdQueryHandler _handler;

    public GetUserRoleMappingByIdQueryHandlerTests()
    {
        _handler = new GetUserRoleMappingByIdQueryHandler(DbContextMock.Object);
    }

    [Fact]
    public async Task Handle_ExistingMapping_ReturnsMapping()
    {
        var mappingId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var roleId = Guid.NewGuid();
        var departmentId = Guid.NewGuid();
        var user = CreateTestUser(id: userId);
        var role = CreateTestRole(id: roleId, name: "Manager");
        var department = CreateTestDepartment(id: departmentId, name: "Sales");

        var mapping = new UserRoleMappingEntity
        {
            Id = mappingId,
            UserId = userId,
            RoleId = roleId,
            DepartmentId = departmentId,
            User = user,
            Role = role,
            Department = department
        };
        var mappings = new List<UserRoleMappingEntity> { mapping };
        var mockMappings = mappings.AsQueryable().BuildMockDbSet();

        DbContextMock.Setup(x => x.UserRoleMappings).Returns(mockMappings.Object);

        var query = new GetUserRoleMappingByIdQuery(mappingId);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result!.Id.Should().Be(mappingId);
    }

    [Fact]
    public async Task Handle_NonExistentMapping_ReturnsNull()
    {
        var mappings = new List<UserRoleMappingEntity>();
        var mockMappings = mappings.AsQueryable().BuildMockDbSet();

        DbContextMock.Setup(x => x.UserRoleMappings).Returns(mockMappings.Object);

        var query = new GetUserRoleMappingByIdQuery(Guid.NewGuid());

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().BeNull();
    }
}

#endregion

#region GetAllUserRoleMappings Tests

public class GetAllUserRoleMappingsQueryHandlerTests : ApplicationTestBase
{
    private readonly GetAllUserRoleMappingsQueryHandler _handler;

    public GetAllUserRoleMappingsQueryHandlerTests()
    {
        _handler = new GetAllUserRoleMappingsQueryHandler(DbContextMock.Object);
    }

    [Fact]
    public async Task Handle_MultipleMappings_ReturnsAll()
    {
        var user1 = CreateTestUser(email: "user1@example.com");
        var user2 = CreateTestUser(email: "user2@example.com");
        var role = CreateTestRole(name: "Manager");

        var mappings = new List<UserRoleMappingEntity> {
            new() { Id = Guid.NewGuid(), User = user1, Role = role, IsActive = true },
            new() { Id = Guid.NewGuid(), User = user2, Role = role, IsActive = true }
        };
        var mockMappings = mappings.AsQueryable().BuildMockDbSet();

        DbContextMock.Setup(x => x.UserRoleMappings).Returns(mockMappings.Object);

        var query = new GetAllUserRoleMappingsQuery();

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handle_NoMappings_ReturnsEmptyList()
    {
        var mappings = new List<UserRoleMappingEntity>();
        var mockMappings = mappings.AsQueryable().BuildMockDbSet();

        DbContextMock.Setup(x => x.UserRoleMappings).Returns(mockMappings.Object);

        var query = new GetAllUserRoleMappingsQuery();

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().BeEmpty();
    }
}

#endregion

#region GetUsersWithoutRoles Tests

public class GetUsersWithoutRolesQueryHandlerTests : ApplicationTestBase
{
    private readonly GetUsersWithoutRolesQueryHandler _handler;

    public GetUsersWithoutRolesQueryHandlerTests()
    {
        _handler = new GetUsersWithoutRolesQueryHandler(
            UserManagerMock.Object,
            DbContextMock.Object);
    }

    [Fact]
    public async Task Handle_UsersWithoutRoles_ReturnsOnlyThoseUsers()
    {
        var userWithRole = CreateTestUser(email: "hasrole@example.com");
        var userWithoutRole = CreateTestUser(email: "norole@example.com");

        var mappings = new List<UserRoleMappingEntity> {
            new() { UserId = userWithRole.Id, IsActive = true }
        };
        var mockMappings = mappings.AsQueryable().BuildMockDbSet();

        DbContextMock.Setup(x => x.UserRoleMappings).Returns(mockMappings.Object);

        var users = new List<ApplicationUser> { userWithRole, userWithoutRole };
        var mockUsers = users.AsQueryable().BuildMockDbSet().Object;
        UserManagerMock.Setup(x => x.Users).Returns(mockUsers);

        var query = new GetUsersWithoutRolesQuery();

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().HaveCount(1);
        result.First().Email.Should().Be("norole@example.com");
    }

    [Fact]
    public async Task Handle_AllUsersHaveRoles_ReturnsEmptyList()
    {
        var user = CreateTestUser();

        var mappings = new List<UserRoleMappingEntity> {
            new() { UserId = user.Id, IsActive = true }
        };
        var mockMappings = mappings.AsQueryable().BuildMockDbSet();

        DbContextMock.Setup(x => x.UserRoleMappings).Returns(mockMappings.Object);

        var users = new List<ApplicationUser> { user };
        var mockUsers = users.AsQueryable().BuildMockDbSet().Object;
        UserManagerMock.Setup(x => x.Users).Returns(mockUsers);

        var query = new GetUsersWithoutRolesQuery();

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_NoUsers_ReturnsEmptyList()
    {
        var mappings = new List<UserRoleMappingEntity>();
        var mockMappings = mappings.AsQueryable().BuildMockDbSet();

        DbContextMock.Setup(x => x.UserRoleMappings).Returns(mockMappings.Object);

        var users = new List<ApplicationUser>();
        var mockUsers = users.AsQueryable().BuildMockDbSet().Object;
        UserManagerMock.Setup(x => x.Users).Returns(mockUsers);

        var query = new GetUsersWithoutRolesQuery();

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().BeEmpty();
    }
}

#endregion
