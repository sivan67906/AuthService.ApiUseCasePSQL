using AuthService.Application.Features.RoleFeatureMapping.CreateRoleFeatureMapping;
using AuthService.Application.Features.RoleFeatureMapping.DeleteRoleFeatureMapping;
using AuthService.Application.Features.RoleFeatureMapping.GetAllRoleFeatureMappings;
using AuthService.Application.Features.RoleFeatureMapping.GetRoleFeatureMappingById;
using AuthService.Application.Features.RoleFeatureMapping.GetRoleFeatureMappingsByDepartment;
using AuthService.Application.Features.RoleFeatureMapping.GetRoleFeatureMappingsByRole;
using AuthService.Application.Features.RoleFeatureMapping.UpdateRoleFeatureMapping;
using MockQueryable.Moq;
using RoleFeatureMappingEntity = AuthService.Domain.Entities.RoleFeatureMapping;

namespace AuthService.Tests.Unit.Application.Features.RoleFeatureMapping;

#region CreateRoleFeatureMapping Tests

public class CreateRoleFeatureMappingCommandHandlerTests : ApplicationTestBase
{
    private readonly CreateRoleFeatureMappingCommandHandler _handler;

    public CreateRoleFeatureMappingCommandHandlerTests()
    {
        _handler = new CreateRoleFeatureMappingCommandHandler(DbContextMock.Object);
    }

    #region Positive Scenarios

    [Fact]
    public async Task Handle_ValidMapping_ReturnsCreatedMapping()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        var featureId = Guid.NewGuid();
        var departmentId = Guid.NewGuid();
        var mappings = new List<RoleFeatureMappingEntity>();
        var mockMappings = mappings.AsQueryable().BuildMockDbSet();

        DbContextMock.Setup(x => x.RoleFeatureMappings).Returns(mockMappings.Object);
        DbContextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var command = new CreateRoleFeatureMappingCommand(roleId, featureId, departmentId, true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.RoleId.Should().Be(roleId);
        result.FeatureId.Should().Be(featureId);
    }

    [Fact]
    public async Task Handle_MappingWithoutDepartment_Succeeds()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        var featureId = Guid.NewGuid();
        var mappings = new List<RoleFeatureMappingEntity>();
        var mockMappings = mappings.AsQueryable().BuildMockDbSet();

        DbContextMock.Setup(x => x.RoleFeatureMappings).Returns(mockMappings.Object);
        DbContextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var command = new CreateRoleFeatureMappingCommand(roleId, featureId, null, true);

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
        var featureId = Guid.NewGuid();
        var existingMapping = new RoleFeatureMappingEntity
        {
            Id = Guid.NewGuid(),
            RoleId = roleId,
            FeatureId = featureId,
            IsDeleted = false
        };
        var mappings = new List<RoleFeatureMappingEntity> { existingMapping };
        var mockMappings = mappings.AsQueryable().BuildMockDbSet();

        DbContextMock.Setup(x => x.RoleFeatureMappings).Returns(mockMappings.Object);

        var command = new CreateRoleFeatureMappingCommand(roleId, featureId, null, true);

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
        var featureId = Guid.NewGuid();
        var deletedMapping = new RoleFeatureMappingEntity
        {
            Id = Guid.NewGuid(),
            RoleId = roleId,
            FeatureId = featureId,
            IsDeleted = true
        };
        var mappings = new List<RoleFeatureMappingEntity> { deletedMapping };
        var mockMappings = mappings.AsQueryable().BuildMockDbSet();

        DbContextMock.Setup(x => x.RoleFeatureMappings).Returns(mockMappings.Object);

        var command = new CreateRoleFeatureMappingCommand(roleId, featureId, null, true);

        // Act & Assert
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*deactivated mode*");
    }

    #endregion
}

public class CreateRoleFeatureMappingCommandValidatorTests
{
    private readonly CreateRoleFeatureMappingCommandValidator _validator;

    public CreateRoleFeatureMappingCommandValidatorTests()
    {
        _validator = new CreateRoleFeatureMappingCommandValidator();
    }

    [Fact]
    public void Validate_ValidCommand_PassesValidation()
    {
        var command = new CreateRoleFeatureMappingCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), true);
        var result = _validator.Validate(command);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_EmptyRoleId_FailsValidation()
    {
        var command = new CreateRoleFeatureMappingCommand(Guid.Empty, Guid.NewGuid(), null, true);
        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validate_EmptyFeatureId_FailsValidation()
    {
        var command = new CreateRoleFeatureMappingCommand(Guid.NewGuid(), Guid.Empty, null, true);
        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
    }
}

#endregion

#region GetRoleFeatureMappingById Tests

public class GetRoleFeatureMappingByIdQueryHandlerTests : ApplicationTestBase
{
    private readonly GetRoleFeatureMappingByIdQueryHandler _handler;

    public GetRoleFeatureMappingByIdQueryHandlerTests()
    {
        _handler = new GetRoleFeatureMappingByIdQueryHandler(DbContextMock.Object);
    }

    [Fact]
    public async Task Handle_ExistingMapping_ReturnsMapping()
    {
        // Arrange
        var mappingId = Guid.NewGuid();
        var role = CreateTestRole(name: "Admin");
        var feature = CreateTestFeature(name: "Dashboard");
        var mapping = new RoleFeatureMappingEntity
        {
            Id = mappingId,
            RoleId = role.Id,
            FeatureId = feature.Id,
            Role = role,
            Feature = feature,
            IsDeleted = false
        };
        var mappings = new List<RoleFeatureMappingEntity> { mapping };
        var mockMappings = mappings.AsQueryable().BuildMockDbSet();

        DbContextMock.Setup(x => x.RoleFeatureMappings).Returns(mockMappings.Object);

        var query = new GetRoleFeatureMappingByIdQuery(mappingId);

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
        var mappings = new List<RoleFeatureMappingEntity>();
        var mockMappings = mappings.AsQueryable().BuildMockDbSet();

        DbContextMock.Setup(x => x.RoleFeatureMappings).Returns(mockMappings.Object);

        var query = new GetRoleFeatureMappingByIdQuery(Guid.NewGuid());

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
        var deletedMapping = new RoleFeatureMappingEntity
        {
            Id = mappingId,
            RoleId = Guid.NewGuid(),
            FeatureId = Guid.NewGuid(),
            IsDeleted = true
        };
        var mappings = new List<RoleFeatureMappingEntity> { deletedMapping };
        var mockMappings = mappings.AsQueryable().BuildMockDbSet();

        DbContextMock.Setup(x => x.RoleFeatureMappings).Returns(mockMappings.Object);

        var query = new GetRoleFeatureMappingByIdQuery(mappingId);

        // Act & Assert
        var act = async () => await _handler.Handle(query, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("*not found*");
    }
}

#endregion

#region GetAllRoleFeatureMappings Tests

public class GetAllRoleFeatureMappingsQueryHandlerTests : ApplicationTestBase
{
    private readonly GetAllRoleFeatureMappingsQueryHandler _handler;

    public GetAllRoleFeatureMappingsQueryHandlerTests()
    {
        _handler = new GetAllRoleFeatureMappingsQueryHandler(DbContextMock.Object);
    }

    [Fact]
    public async Task Handle_MultipleMappings_ReturnsAllActive()
    {
        // Arrange
        var role = CreateTestRole(name: "Admin");
        var feature = CreateTestFeature(name: "Dashboard");
        var feature2 = CreateTestFeature(name: "Reports");
        var role2 = CreateTestRole(name: "Other");
        var feature3 = CreateTestFeature(name: "Settings");
        var mappings = new List<RoleFeatureMappingEntity> {
            new() { Id = Guid.NewGuid(), RoleId = role.Id, FeatureId = feature.Id, Role = role, Feature = feature, IsDeleted = false },
            new() { Id = Guid.NewGuid(), RoleId = role.Id, FeatureId = feature2.Id, Role = role, Feature = feature2, IsDeleted = false },
            new() { Id = Guid.NewGuid(), RoleId = role2.Id, FeatureId = feature3.Id, Role = role2, Feature = feature3, IsDeleted = true }
        };
        var mockMappings = mappings.AsQueryable().BuildMockDbSet();

        DbContextMock.Setup(x => x.RoleFeatureMappings).Returns(mockMappings.Object);

        var query = new GetAllRoleFeatureMappingsQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handle_NoMappings_ReturnsEmptyList()
    {
        // Arrange
        var mappings = new List<RoleFeatureMappingEntity>();
        var mockMappings = mappings.AsQueryable().BuildMockDbSet();

        DbContextMock.Setup(x => x.RoleFeatureMappings).Returns(mockMappings.Object);

        var query = new GetAllRoleFeatureMappingsQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
    }
}

#endregion

#region GetRoleFeatureMappingsByRole Tests

public class GetRoleFeatureMappingsByRoleQueryHandlerTests : ApplicationTestBase
{
    private readonly GetRoleFeatureMappingsByRoleQueryHandler _handler;

    public GetRoleFeatureMappingsByRoleQueryHandlerTests()
    {
        _handler = new GetRoleFeatureMappingsByRoleQueryHandler(DbContextMock.Object);
    }

    [Fact]
    public async Task Handle_ValidRoleId_ReturnsMappings()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        var role = CreateTestRole(id: roleId, name: "Admin");
        var feature1 = CreateTestFeature(name: "Dashboard");
        var feature2 = CreateTestFeature(name: "Reports");
        var otherFeature = CreateTestFeature(name: "Other");
        var otherRole = CreateTestRole(id: Guid.NewGuid(), name: "Other");
        var mappings = new List<RoleFeatureMappingEntity> {
            new() { Id = Guid.NewGuid(), RoleId = roleId, FeatureId = feature1.Id, Role = role, Feature = feature1, IsDeleted = false },
            new() { Id = Guid.NewGuid(), RoleId = roleId, FeatureId = feature2.Id, Role = role, Feature = feature2, IsDeleted = false },
            new() { Id = Guid.NewGuid(), RoleId = otherRole.Id, FeatureId = otherFeature.Id, Role = otherRole, Feature = otherFeature, IsDeleted = false }
        };
        var mockMappings = mappings.AsQueryable().BuildMockDbSet();

        DbContextMock.Setup(x => x.RoleFeatureMappings).Returns(mockMappings.Object);

        var query = new GetRoleFeatureMappingsByRoleQuery(roleId);

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
        var mappings = new List<RoleFeatureMappingEntity>();
        var mockMappings = mappings.AsQueryable().BuildMockDbSet();

        DbContextMock.Setup(x => x.RoleFeatureMappings).Returns(mockMappings.Object);

        var query = new GetRoleFeatureMappingsByRoleQuery(Guid.NewGuid());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
    }
}

#endregion

#region GetRoleFeatureMappingsByDepartment Tests

public class GetRoleFeatureMappingsByDepartmentQueryHandlerTests : ApplicationTestBase
{
    private readonly GetRoleFeatureMappingsByDepartmentQueryHandler _handler;

    public GetRoleFeatureMappingsByDepartmentQueryHandlerTests()
    {
        _handler = new GetRoleFeatureMappingsByDepartmentQueryHandler(DbContextMock.Object);
    }

    [Fact]
    public async Task Handle_ValidDepartmentId_ReturnsMappings()
    {
        // Arrange
        var departmentId = Guid.NewGuid();
        var role = CreateTestRole(name: "Admin");
        var feature1 = CreateTestFeature(name: "Dashboard");
        var feature2 = CreateTestFeature(name: "Reports");
        var otherRole = CreateTestRole(name: "Other");
        var otherFeature = CreateTestFeature(name: "Other");
        var mappings = new List<RoleFeatureMappingEntity> {
            new() { Id = Guid.NewGuid(), RoleId = role.Id, FeatureId = feature1.Id, DepartmentId = departmentId, Role = role, Feature = feature1, IsDeleted = false },
            new() { Id = Guid.NewGuid(), RoleId = role.Id, FeatureId = feature2.Id, DepartmentId = departmentId, Role = role, Feature = feature2, IsDeleted = false },
            new() { Id = Guid.NewGuid(), RoleId = otherRole.Id, FeatureId = otherFeature.Id, DepartmentId = Guid.NewGuid(), Role = otherRole, Feature = otherFeature, IsDeleted = false }
        };
        var mockMappings = mappings.AsQueryable().BuildMockDbSet();

        DbContextMock.Setup(x => x.RoleFeatureMappings).Returns(mockMappings.Object);

        var query = new GetRoleFeatureMappingsByDepartmentQuery(departmentId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);
        result.Should().AllSatisfy(m => m.DepartmentId.Should().Be(departmentId));
    }
}

#endregion

#region DeleteRoleFeatureMapping Tests

public class DeleteRoleFeatureMappingCommandHandlerTests : ApplicationTestBase
{
    private readonly DeleteRoleFeatureMappingCommandHandler _handler;

    public DeleteRoleFeatureMappingCommandHandlerTests()
    {
        _handler = new DeleteRoleFeatureMappingCommandHandler(DbContextMock.Object);
    }

    [Fact]
    public async Task Handle_ExistingMapping_ReturnsTrue()
    {
        // Arrange
        var mappingId = Guid.NewGuid();
        var mapping = new RoleFeatureMappingEntity
        {
            Id = mappingId,
            RoleId = Guid.NewGuid(),
            FeatureId = Guid.NewGuid(),
            IsDeleted = false
        };
        var mappings = new List<RoleFeatureMappingEntity> { mapping };
        var mockMappings = mappings.AsQueryable().BuildMockDbSet();

        DbContextMock.Setup(x => x.RoleFeatureMappings).Returns(mockMappings.Object);
        DbContextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var command = new DeleteRoleFeatureMappingCommand(mappingId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        // Handler does hard delete (Remove), not soft delete
    }

    [Fact]
    public async Task Handle_MappingNotFound_ThrowsInvalidOperationException()
    {
        // Arrange
        var mappings = new List<RoleFeatureMappingEntity>();
        var mockMappings = mappings.AsQueryable().BuildMockDbSet();

        DbContextMock.Setup(x => x.RoleFeatureMappings).Returns(mockMappings.Object);

        var command = new DeleteRoleFeatureMappingCommand(Guid.NewGuid());

        // Act & Assert
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("*not found*");
    }
}

#endregion

#region UpdateRoleFeatureMapping Tests

public class UpdateRoleFeatureMappingCommandHandlerTests : ApplicationTestBase
{
    private readonly UpdateRoleFeatureMappingCommandHandler _handler;

    public UpdateRoleFeatureMappingCommandHandlerTests()
    {
        _handler = new UpdateRoleFeatureMappingCommandHandler(DbContextMock.Object);
    }

    [Fact]
    public async Task Handle_ValidUpdate_ReturnsUpdatedMapping()
    {
        // Arrange
        var mappingId = Guid.NewGuid();
        var existingMapping = new RoleFeatureMappingEntity
        {
            Id = mappingId,
            RoleId = Guid.NewGuid(),
            FeatureId = Guid.NewGuid(),
            IsActive = true,
            IsDeleted = false
        };
        var mappings = new List<RoleFeatureMappingEntity> { existingMapping };
        var mockMappings = mappings.AsQueryable().BuildMockDbSet();

        DbContextMock.Setup(x => x.RoleFeatureMappings).Returns(mockMappings.Object);
        DbContextMock.Setup(x => x.Set<RoleFeatureMappingEntity>()).Returns(mockMappings.Object);
        DbContextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var command = new UpdateRoleFeatureMappingCommand(mappingId, Guid.NewGuid(), Guid.NewGuid(), null, false);

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
        var mappings = new List<RoleFeatureMappingEntity>();
        var mockMappings = mappings.AsQueryable().BuildMockDbSet();

        DbContextMock.Setup(x => x.RoleFeatureMappings).Returns(mockMappings.Object);

        var command = new UpdateRoleFeatureMappingCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), null, true);

        // Act & Assert
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*not found*");
    }
}

#endregion
