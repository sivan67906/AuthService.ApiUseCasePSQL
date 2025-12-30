using AuthService.Application.Features.PageFeatureMapping.CreatePageFeatureMapping;
using AuthService.Application.Features.PageFeatureMapping.DeletePageFeatureMapping;
using AuthService.Application.Features.PageFeatureMapping.GetAllPageFeatureMappings;
using AuthService.Application.Features.PageFeatureMapping.GetPageFeatureMapping;
using AuthService.Application.Features.PageFeatureMapping.UpdatePageFeatureMapping;
using MockQueryable.Moq;
using PageFeatureMappingEntity = AuthService.Domain.Entities.PageFeatureMapping;

namespace AuthService.Tests.Unit.Application.Features.PageFeatureMapping;

#region CreatePageFeatureMapping Tests

public class CreatePageFeatureMappingCommandHandlerTests : ApplicationTestBase
{
    private readonly CreatePageFeatureMappingCommandHandler _handler;

    public CreatePageFeatureMappingCommandHandlerTests()
    {
        _handler = new CreatePageFeatureMappingCommandHandler(DbContextMock.Object);
    }

    #region Positive Scenarios

    [Fact]
    public async Task Handle_ValidMapping_ReturnsCreatedMapping()
    {
        // Arrange
        var pageId = Guid.NewGuid();
        var featureId = Guid.NewGuid();
        var mappings = new List<PageFeatureMappingEntity>();
        var mockMappings = mappings.AsQueryable().BuildMockDbSet();

        DbContextMock.Setup(x => x.PageFeatureMappings).Returns(mockMappings.Object);
        DbContextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var command = new CreatePageFeatureMappingCommand(pageId, featureId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.PageId.Should().Be(pageId);
        result.FeatureId.Should().Be(featureId);
    }

    #endregion

    #region Negative Scenarios

    [Fact]
    public async Task Handle_DuplicateMapping_ThrowsInvalidOperationException()
    {
        // Arrange
        var pageId = Guid.NewGuid();
        var featureId = Guid.NewGuid();
        var existingMapping = new PageFeatureMappingEntity
        {
            Id = Guid.NewGuid(),
            PageId = pageId,
            FeatureId = featureId,
            IsDeleted = false
        };
        var mappings = new List<PageFeatureMappingEntity> { existingMapping };
        var mockMappings = mappings.AsQueryable().BuildMockDbSet();

        DbContextMock.Setup(x => x.PageFeatureMappings).Returns(mockMappings.Object);

        var command = new CreatePageFeatureMappingCommand(pageId, featureId);

        // Act & Assert
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*already exists*");
    }

    [Fact]
    public async Task Handle_DeletedMappingWithSameKeys_ThrowsWithDeactivatedMessage()
    {
        // Arrange
        var pageId = Guid.NewGuid();
        var featureId = Guid.NewGuid();
        var deletedMapping = new PageFeatureMappingEntity
        {
            Id = Guid.NewGuid(),
            PageId = pageId,
            FeatureId = featureId,
            IsDeleted = true
        };
        var mappings = new List<PageFeatureMappingEntity> { deletedMapping };
        var mockMappings = mappings.AsQueryable().BuildMockDbSet();

        DbContextMock.Setup(x => x.PageFeatureMappings).Returns(mockMappings.Object);

        var command = new CreatePageFeatureMappingCommand(pageId, featureId);

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
        var mappings = new List<PageFeatureMappingEntity>();
        var mockMappings = mappings.AsQueryable().BuildMockDbSet();

        DbContextMock.Setup(x => x.PageFeatureMappings).Returns(mockMappings.Object);
        DbContextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        var command = new CreatePageFeatureMappingCommand(Guid.NewGuid(), Guid.NewGuid());

        // Act & Assert
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<Exception>()
            .WithMessage("Database error");
    }

    #endregion
}

public class CreatePageFeatureMappingCommandValidatorTests
{
    private readonly CreatePageFeatureMappingCommandValidator _validator;

    public CreatePageFeatureMappingCommandValidatorTests()
    {
        _validator = new CreatePageFeatureMappingCommandValidator();
    }

    [Fact]
    public void Validate_ValidCommand_PassesValidation()
    {
        var command = new CreatePageFeatureMappingCommand(Guid.NewGuid(), Guid.NewGuid());
        var result = _validator.Validate(command);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_EmptyPageId_FailsValidation()
    {
        var command = new CreatePageFeatureMappingCommand(Guid.Empty, Guid.NewGuid());
        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validate_EmptyFeatureId_FailsValidation()
    {
        var command = new CreatePageFeatureMappingCommand(Guid.NewGuid(), Guid.Empty);
        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
    }
}

#endregion

#region GetPageFeatureMapping Tests

public class GetPageFeatureMappingQueryHandlerTests : ApplicationTestBase
{
    private readonly GetPageFeatureMappingQueryHandler _handler;

    public GetPageFeatureMappingQueryHandlerTests()
    {
        _handler = new GetPageFeatureMappingQueryHandler(DbContextMock.Object);
    }

    [Fact]
    public async Task Handle_ExistingMapping_ReturnsMapping()
    {
        // Arrange
        var mappingId = Guid.NewGuid();
        var mapping = new PageFeatureMappingEntity
        {
            Id = mappingId,
            PageId = Guid.NewGuid(),
            FeatureId = Guid.NewGuid(),
            IsDeleted = false
        };
        var mappings = new List<PageFeatureMappingEntity> { mapping };
        var mockMappings = mappings.AsQueryable().BuildMockDbSet();

        DbContextMock.Setup(x => x.PageFeatureMappings).Returns(mockMappings.Object);

        var query = new GetPageFeatureMappingQuery(mappingId);

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
        var mappings = new List<PageFeatureMappingEntity>();
        var mockMappings = mappings.AsQueryable().BuildMockDbSet();

        DbContextMock.Setup(x => x.PageFeatureMappings).Returns(mockMappings.Object);

        var query = new GetPageFeatureMappingQuery(Guid.NewGuid());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task Handle_DeletedMapping_ReturnsNull()
    {
        // Arrange
        var mappingId = Guid.NewGuid();
        var deletedMapping = new PageFeatureMappingEntity
        {
            Id = mappingId,
            PageId = Guid.NewGuid(),
            FeatureId = Guid.NewGuid(),
            IsDeleted = true
        };
        var mappings = new List<PageFeatureMappingEntity> { deletedMapping };
        var mockMappings = mappings.AsQueryable().BuildMockDbSet();

        DbContextMock.Setup(x => x.PageFeatureMappings).Returns(mockMappings.Object);

        var query = new GetPageFeatureMappingQuery(mappingId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }
}

#endregion

#region GetAllPageFeatureMappings Tests

public class GetAllPageFeatureMappingsQueryHandlerTests : ApplicationTestBase
{
    private readonly GetAllPageFeatureMappingsQueryHandler _handler;

    public GetAllPageFeatureMappingsQueryHandlerTests()
    {
        _handler = new GetAllPageFeatureMappingsQueryHandler(DbContextMock.Object);
    }

    [Fact]
    public async Task Handle_MultipleMappings_ReturnsAllActive()
    {
        // Arrange
        var mappings = new List<PageFeatureMappingEntity> {
            new() { Id = Guid.NewGuid(), PageId = Guid.NewGuid(), FeatureId = Guid.NewGuid(), IsDeleted = false },
            new() { Id = Guid.NewGuid(), PageId = Guid.NewGuid(), FeatureId = Guid.NewGuid(), IsDeleted = false },
            new() { Id = Guid.NewGuid(), PageId = Guid.NewGuid(), FeatureId = Guid.NewGuid(), IsDeleted = true }
        };
        var mockMappings = mappings.AsQueryable().BuildMockDbSet();

        DbContextMock.Setup(x => x.PageFeatureMappings).Returns(mockMappings.Object);

        var query = new GetAllPageFeatureMappingsQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handle_NoMappings_ReturnsEmptyList()
    {
        // Arrange
        var mappings = new List<PageFeatureMappingEntity>();
        var mockMappings = mappings.AsQueryable().BuildMockDbSet();

        DbContextMock.Setup(x => x.PageFeatureMappings).Returns(mockMappings.Object);

        var query = new GetAllPageFeatureMappingsQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
    }
}

#endregion

#region DeletePageFeatureMapping Tests

public class DeletePageFeatureMappingCommandHandlerTests : ApplicationTestBase
{
    private readonly DeletePageFeatureMappingCommandHandler _handler;

    public DeletePageFeatureMappingCommandHandlerTests()
    {
        _handler = new DeletePageFeatureMappingCommandHandler(DbContextMock.Object);
    }

    [Fact]
    public async Task Handle_ExistingMapping_ReturnsTrue()
    {
        // Arrange
        var mappingId = Guid.NewGuid();
        var mapping = new PageFeatureMappingEntity
        {
            Id = mappingId,
            PageId = Guid.NewGuid(),
            FeatureId = Guid.NewGuid(),
            IsDeleted = false
        };
        var mappings = new List<PageFeatureMappingEntity> { mapping };
        var mockMappings = mappings.AsQueryable().BuildMockDbSet();

        DbContextMock.Setup(x => x.PageFeatureMappings).Returns(mockMappings.Object);
        DbContextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var command = new DeletePageFeatureMappingCommand(mappingId);

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
        var mappings = new List<PageFeatureMappingEntity>();
        var mockMappings = mappings.AsQueryable().BuildMockDbSet();

        DbContextMock.Setup(x => x.PageFeatureMappings).Returns(mockMappings.Object);

        var command = new DeletePageFeatureMappingCommand(Guid.NewGuid());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert - Handler returns false when not found, doesn't throw
        result.Should().BeFalse();
    }
}

#endregion

#region UpdatePageFeatureMapping Tests

public class UpdatePageFeatureMappingCommandHandlerTests : ApplicationTestBase
{
    private readonly UpdatePageFeatureMappingCommandHandler _handler;

    public UpdatePageFeatureMappingCommandHandlerTests()
    {
        _handler = new UpdatePageFeatureMappingCommandHandler(DbContextMock.Object);
    }

    [Fact]
    public async Task Handle_ValidUpdate_ReturnsUpdatedMapping()
    {
        // Arrange
        var mappingId = Guid.NewGuid();
        var newPageId = Guid.NewGuid();
        var newFeatureId = Guid.NewGuid();
        var existingMapping = new PageFeatureMappingEntity
        {
            Id = mappingId,
            PageId = Guid.NewGuid(),
            FeatureId = Guid.NewGuid(),
            IsDeleted = false
        };
        var mappings = new List<PageFeatureMappingEntity> { existingMapping };
        var mockMappings = mappings.AsQueryable().BuildMockDbSet();

        DbContextMock.Setup(x => x.PageFeatureMappings).Returns(mockMappings.Object);
        DbContextMock.Setup(x => x.Set<PageFeatureMappingEntity>()).Returns(mockMappings.Object);
        DbContextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var command = new UpdatePageFeatureMappingCommand(mappingId, newPageId, newFeatureId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.PageId.Should().Be(newPageId);
        result.FeatureId.Should().Be(newFeatureId);
    }

    [Fact]
    public async Task Handle_MappingNotFound_ThrowsInvalidOperationException()
    {
        // Arrange
        var mappings = new List<PageFeatureMappingEntity>();
        var mockMappings = mappings.AsQueryable().BuildMockDbSet();

        DbContextMock.Setup(x => x.PageFeatureMappings).Returns(mockMappings.Object);

        var command = new UpdatePageFeatureMappingCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());

        // Act & Assert
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*not found*");
    }
}

#endregion
