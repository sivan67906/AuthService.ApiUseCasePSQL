using AuthService.Application.Features.UserRoleMapping.GetUsersWithoutRoles;
using MockQueryable.Moq;

using UserRoleMappingEntity = AuthService.Domain.Entities.UserRoleMapping;

namespace AuthService.Tests.Unit.Application.Features.UserRoleMapping;

/// <summary>
/// Unit tests for GetUsersWithoutRolesFromCommandDbQueryHandler.
/// </summary>
public class GetUsersWithoutRolesFromCommandDbQueryHandlerTests : ApplicationTestBase
{
    private readonly GetUsersWithoutRolesFromCommandDbQueryHandler _handler;

    public GetUsersWithoutRolesFromCommandDbQueryHandlerTests()
    {
        _handler = new GetUsersWithoutRolesFromCommandDbQueryHandler(
            UserManagerMock.Object,
            DbContextMock.Object);
    }

    [Fact]
    public async Task Handle_WhenSomeUsersHaveMappingsInCommandDb_ReturnsOnlyUsersWithoutMappings()
    {
        // Arrange
        var userWithRole = CreateTestUser(email: "hasrole@example.com");
        var userWithoutRole = CreateTestUser(email: "norole@example.com");
        var inactiveUser = CreateTestUser(email: "inactive@example.com", isActive: false);

        var mappings = new List<UserRoleMappingEntity>
        {
            new() { UserId = userWithRole.Id, RoleId = Guid.NewGuid(), IsActive = true },
            new() { UserId = Guid.NewGuid(), RoleId = Guid.NewGuid(), IsActive = false } // inactive mapping ignored
        };

        DbContextMock.Setup(x => x.UserRoleMappings)
            .Returns(mappings.AsQueryable().BuildMockDbSet().Object);

        var users = new List<ApplicationUser> { userWithRole, userWithoutRole, inactiveUser };
        UserManagerMock.Setup(x => x.Users)
            .Returns(users.AsQueryable().BuildMockDbSet().Object);

        var query = new GetUsersWithoutRolesFromCommandDbQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().HaveCount(1);
        result[0].Email.Should().Be("norole@example.com");
        result[0].IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WhenAllUsersHaveActiveMappings_ReturnsEmptyList()
    {
        // Arrange
        var user = CreateTestUser(email: "user@example.com");

        DbContextMock.Setup(x => x.UserRoleMappings)
            .Returns(new List<UserRoleMappingEntity>
            {
                new() { UserId = user.Id, RoleId = Guid.NewGuid(), IsActive = true }
            }.AsQueryable().BuildMockDbSet().Object);

        UserManagerMock.Setup(x => x.Users)
            .Returns(new List<ApplicationUser> { user }.AsQueryable().BuildMockDbSet().Object);

        // Act
        var result = await _handler.Handle(new GetUsersWithoutRolesFromCommandDbQuery(), CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
    }
}
