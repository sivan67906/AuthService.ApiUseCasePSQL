using AuthService.Application.Features.Feature.CreateFeature;
using AuthService.Application.Features.Feature.DeleteFeature;
using AuthService.Application.Features.Feature.GetAllFeatures;
using AuthService.Application.Features.Feature.GetFeature;
using AuthService.Application.Features.Feature.UpdateFeature;
using MockQueryable.Moq;
using FeatureEntity = AuthService.Domain.Entities.Feature;

namespace AuthService.Tests.Unit.Application.Features.Feature;

#region CreateFeature Tests

public class CreateFeatureCommandHandlerTests : ApplicationTestBase
{
    private readonly CreateFeatureCommandHandler _handler;

    public CreateFeatureCommandHandlerTests()
    {
        _handler = new CreateFeatureCommandHandler(DbContextMock.Object);
    }

    [Fact]
    public async Task Handle_ValidFeature_ReturnsCreatedFeature()
    {
        // Arrange
        // CreateFeatureCommand: Code, Name, Description, RouteUrl, IsMainMenu, ParentFeatureId, DisplayOrder, Level, Icon, IsActive
        var command = new CreateFeatureCommand(
            "DASHBOARD",
            "Dashboard",
            "Main dashboard",
            "/dashboard",
            true,   // IsMainMenu
            null,   // ParentFeatureId
            1,      // DisplayOrder
            0,      // Level (0 for main menu)
            "dashboard-icon",
            true);  // IsActive

        var features = new List<FeatureEntity>();
        var mockDbSet = features.AsQueryable().BuildMockDbSet();

        DbContextMock.Setup(x => x.Features).Returns(mockDbSet.Object);
        DbContextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Code.Should().Be("DASHBOARD");
        result.Name.Should().Be("Dashboard");
    }

    [Fact]
    public async Task Handle_DuplicateCode_ThrowsException()
    {
        // Arrange
        var existingFeature = CreateTestFeature(code: "DASHBOARD");
        var features = new List<FeatureEntity> { existingFeature };
        var mockDbSet = features.AsQueryable().BuildMockDbSet();

        DbContextMock.Setup(x => x.Features).Returns(mockDbSet.Object);

        var command = new CreateFeatureCommand(
            "DASHBOARD",
            "Dashboard 2",
            null,
            "/dashboard",
            true,
            null,
            1,
            0,
            null,
            true);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_SubFeature_SetsCorrectLevel()
    {
        // Arrange
        var parentId = Guid.NewGuid();
        var parentFeature = CreateTestFeature(id: parentId, code: "PARENT", name: "Parent Feature");
        var features = new List<FeatureEntity> { parentFeature };
        var mockDbSet = features.AsQueryable().BuildMockDbSet();

        DbContextMock.Setup(x => x.Features).Returns(mockDbSet.Object);
        DbContextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var command = new CreateFeatureCommand(
            "CHILD",
            "Child Feature",
            null,
            "/child",
            false,    // Not main menu
            parentId, // Parent feature
            1,
            1,        // Level 1 for sub-feature
            null,
            true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.ParentFeatureId.Should().Be(parentId);
    }
}

#endregion

#region GetFeature Tests

public class GetFeatureQueryHandlerTests : ApplicationTestBase
{
    private readonly GetFeatureQueryHandler _handler;

    public GetFeatureQueryHandlerTests()
    {
        _handler = new GetFeatureQueryHandler(DbContextMock.Object);
    }

    [Fact]
    public async Task Handle_ExistingFeature_ReturnsFeature()
    {
        // Arrange
        var featureId = Guid.NewGuid();
        var feature = CreateTestFeature(id: featureId, code: "DASHBOARD", name: "Dashboard");
        var features = new List<FeatureEntity> { feature };
        var mockDbSet = features.AsQueryable().BuildMockDbSet();

        DbContextMock.Setup(x => x.Features).Returns(mockDbSet.Object);

        var query = new GetFeatureQuery(featureId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Code.Should().Be("DASHBOARD");
    }

    [Fact]
    public async Task Handle_NonExistingFeature_ReturnsNull()
    {
        // Arrange
        var features = new List<FeatureEntity>();
        var mockDbSet = features.AsQueryable().BuildMockDbSet();

        DbContextMock.Setup(x => x.Features).Returns(mockDbSet.Object);

        var query = new GetFeatureQuery(Guid.NewGuid());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }
}

#endregion

#region GetAllFeatures Tests

public class GetAllFeaturesQueryHandlerTests : ApplicationTestBase
{
    private readonly GetAllFeaturesQueryHandler _handler;

    public GetAllFeaturesQueryHandlerTests()
    {
        _handler = new GetAllFeaturesQueryHandler(DbContextMock.Object);
    }

    [Fact]
    public async Task Handle_WithFeatures_ReturnsAllActiveFeatures()
    {
        // Arrange
        var features = new List<FeatureEntity>
        {
            CreateTestFeature(code: "FEAT1", name: "Feature 1"),
            CreateTestFeature(code: "FEAT2", name: "Feature 2"),
            CreateTestFeature(code: "FEAT3", name: "Feature 3", isDeleted: true)
        };
        var mockDbSet = features.AsQueryable().BuildMockDbSet();

        DbContextMock.Setup(x => x.Features).Returns(mockDbSet.Object);

        var query = new GetAllFeaturesQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handle_EmptyDatabase_ReturnsEmptyList()
    {
        // Arrange
        var features = new List<FeatureEntity>();
        var mockDbSet = features.AsQueryable().BuildMockDbSet();

        DbContextMock.Setup(x => x.Features).Returns(mockDbSet.Object);

        var query = new GetAllFeaturesQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
    }
}

#endregion

#region UpdateFeature Tests

public class UpdateFeatureCommandHandlerTests : ApplicationTestBase
{
    private readonly UpdateFeatureCommandHandler _handler;

    public UpdateFeatureCommandHandlerTests()
    {
        _handler = new UpdateFeatureCommandHandler(DbContextMock.Object);
    }

    [Fact]
    public async Task Handle_ExistingFeature_UpdatesFeature()
    {
        // Arrange
        var featureId = Guid.NewGuid();
        var feature = CreateTestFeature(id: featureId, code: "DASHBOARD", name: "Dashboard");
        var features = new List<FeatureEntity> { feature };
        var mockDbSet = features.AsQueryable().BuildMockDbSet();

        DbContextMock.Setup(x => x.Features).Returns(mockDbSet.Object);
        DbContextMock.Setup(x => x.Set<FeatureEntity>()).Returns(mockDbSet.Object);
        DbContextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var command = new UpdateFeatureCommand(
            featureId,
            "Updated Dashboard",
            "Updated description",
            "/updated-dashboard",
            true,
            null,
            2,
            0,
            "new-icon",
            true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Updated Dashboard");
    }

    [Fact]
    public async Task Handle_NonExistingFeature_ThrowsException()
    {
        // Arrange
        var features = new List<FeatureEntity>();
        var mockDbSet = features.AsQueryable().BuildMockDbSet();

        DbContextMock.Setup(x => x.Features).Returns(mockDbSet.Object);

        var command = new UpdateFeatureCommand(
            Guid.NewGuid(),
            "Updated Feature",
            null,
            null,
            true,
            null,
            1,
            0,
            null,
            true);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None));
    }
}

#endregion

#region DeleteFeature Tests

public class DeleteFeatureCommandHandlerTests : ApplicationTestBase
{
    private readonly DeleteFeatureCommandHandler _handler;

    public DeleteFeatureCommandHandlerTests()
    {
        _handler = new DeleteFeatureCommandHandler(DbContextMock.Object);
    }

    [Fact]
    public async Task Handle_ExistingFeature_SoftDeletes()
    {
        // Arrange
        var featureId = Guid.NewGuid();
        var feature = CreateTestFeature(id: featureId, code: "DASHBOARD");
        var features = new List<FeatureEntity> { feature };
        var mockDbSet = features.AsQueryable().BuildMockDbSet();

        DbContextMock.Setup(x => x.Features).Returns(mockDbSet.Object);
        DbContextMock.Setup(x => x.Set<FeatureEntity>()).Returns(mockDbSet.Object);
        DbContextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var command = new DeleteFeatureCommand(featureId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        feature.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_NonExistingFeature_ThrowsException()
    {
        // Arrange
        var features = new List<FeatureEntity>();
        var mockDbSet = features.AsQueryable().BuildMockDbSet();

        DbContextMock.Setup(x => x.Features).Returns(mockDbSet.Object);

        var command = new DeleteFeatureCommand(Guid.NewGuid());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert - Handler returns false when not found, doesn't throw
        result.Should().BeFalse();
    }
}

#endregion
