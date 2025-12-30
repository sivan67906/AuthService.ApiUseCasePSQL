namespace AuthService.Tests.Unit.Domain.Entities;

/// <summary>
/// Unit tests for BaseEntity class and related interfaces
/// </summary>
public class BaseEntityTests
{
    #region Test Helper Class

    /// <summary>
    /// Concrete implementation for testing abstract BaseEntity
    /// </summary>
    private sealed class TestEntity : BaseEntity
    {
        public string? TestProperty { get; set; }
    }

    #endregion

    #region Constructor and Default Values Tests

    [Fact]
    public void BaseEntity_WhenCreated_ShouldHaveNewGuidId()
    {
        // Act
        var entity = new TestEntity();

        // Assert
        entity.Id.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void BaseEntity_WhenCreated_ShouldHaveUniqueIds()
    {
        // Act
        var entity1 = new TestEntity();
        var entity2 = new TestEntity();

        // Assert
        entity1.Id.Should().NotBe(entity2.Id);
    }

    [Fact]
    public void BaseEntity_WhenCreated_ShouldHaveCreatedAtSetToUtcNow()
    {
        // Arrange
        var beforeCreation = DateTime.UtcNow;

        // Act
        var entity = new TestEntity();

        // Assert
        var afterCreation = DateTime.UtcNow;
        entity.CreatedAt.Should().BeOnOrAfter(beforeCreation);
        entity.CreatedAt.Should().BeOnOrBefore(afterCreation);
    }

    [Fact]
    public void BaseEntity_WhenCreated_ShouldHaveNullCreatedBy()
    {
        // Act
        var entity = new TestEntity();

        // Assert
        entity.CreatedBy.Should().BeNull();
    }

    [Fact]
    public void BaseEntity_WhenCreated_ShouldHaveNullUpdatedAt()
    {
        // Act
        var entity = new TestEntity();

        // Assert
        entity.UpdatedAt.Should().BeNull();
    }

    [Fact]
    public void BaseEntity_WhenCreated_ShouldHaveNullModifiedBy()
    {
        // Act
        var entity = new TestEntity();

        // Assert
        entity.ModifiedBy.Should().BeNull();
    }

    [Fact]
    public void BaseEntity_WhenCreated_ShouldHaveIsDeletedFalse()
    {
        // Act
        var entity = new TestEntity();

        // Assert
        entity.IsDeleted.Should().BeFalse();
    }

    #endregion

    #region Property Assignment Tests

    [Fact]
    public void BaseEntity_WhenIdAssigned_ShouldRetainValue()
    {
        // Arrange
        var entity = new TestEntity();
        var newId = Guid.NewGuid();

        // Act
        entity.Id = newId;

        // Assert
        entity.Id.Should().Be(newId);
    }

    [Fact]
    public void BaseEntity_WhenCreatedByAssigned_ShouldRetainValue()
    {
        // Arrange
        var entity = new TestEntity();
        const string createdBy = "admin@example.com";

        // Act
        entity.CreatedBy = createdBy;

        // Assert
        entity.CreatedBy.Should().Be(createdBy);
    }

    [Fact]
    public void BaseEntity_WhenUpdatedAtAssigned_ShouldRetainValue()
    {
        // Arrange
        var entity = new TestEntity();
        var updatedAt = DateTime.UtcNow;

        // Act
        entity.UpdatedAt = updatedAt;

        // Assert
        entity.UpdatedAt.Should().Be(updatedAt);
    }

    [Fact]
    public void BaseEntity_WhenModifiedByAssigned_ShouldRetainValue()
    {
        // Arrange
        var entity = new TestEntity();
        const string modifiedBy = "user@example.com";

        // Act
        entity.ModifiedBy = modifiedBy;

        // Assert
        entity.ModifiedBy.Should().Be(modifiedBy);
    }

    [Fact]
    public void BaseEntity_WhenIsDeletedSetToTrue_ShouldRetainValue()
    {
        // Arrange
        var entity = new TestEntity();

        // Act
        entity.IsDeleted = true;

        // Assert
        entity.IsDeleted.Should().BeTrue();
    }

    #endregion

    #region Interface Implementation Tests

    [Fact]
    public void BaseEntity_ShouldImplementIAuditableEntity()
    {
        // Arrange
        var entity = new TestEntity();

        // Assert
        entity.Should().BeAssignableTo<IAuditableEntity>();
    }

    [Fact]
    public void BaseEntity_ShouldImplementISoftDeletable()
    {
        // Arrange
        var entity = new TestEntity();

        // Assert
        entity.Should().BeAssignableTo<ISoftDeletable>();
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void BaseEntity_WhenIdSetToEmptyGuid_ShouldAcceptValue()
    {
        // Arrange
        var entity = new TestEntity();

        // Act
        entity.Id = Guid.Empty;

        // Assert
        entity.Id.Should().Be(Guid.Empty);
    }

    [Fact]
    public void BaseEntity_WhenCreatedBySetToEmptyString_ShouldAcceptValue()
    {
        // Arrange
        var entity = new TestEntity();

        // Act
        entity.CreatedBy = string.Empty;

        // Assert
        entity.CreatedBy.Should().BeEmpty();
    }

    [Fact]
    public void BaseEntity_WhenCreatedBySetToWhitespace_ShouldAcceptValue()
    {
        // Arrange
        var entity = new TestEntity();

        // Act
        entity.CreatedBy = "   ";

        // Assert
        entity.CreatedBy.Should().Be("   ");
    }

    [Fact]
    public void BaseEntity_WhenCreatedAtSetToMinValue_ShouldAcceptValue()
    {
        // Arrange
        var entity = new TestEntity();

        // Act
        entity.CreatedAt = DateTime.MinValue;

        // Assert
        entity.CreatedAt.Should().Be(DateTime.MinValue);
    }

    [Fact]
    public void BaseEntity_WhenCreatedAtSetToMaxValue_ShouldAcceptValue()
    {
        // Arrange
        var entity = new TestEntity();

        // Act
        entity.CreatedAt = DateTime.MaxValue;

        // Assert
        entity.CreatedAt.Should().Be(DateTime.MaxValue);
    }

    [Fact]
    public void BaseEntity_WhenUpdatedAtSetToNull_ShouldAcceptValue()
    {
        // Arrange
        var entity = new TestEntity { UpdatedAt = DateTime.UtcNow };

        // Act
        entity.UpdatedAt = null;

        // Assert
        entity.UpdatedAt.Should().BeNull();
    }

    #endregion

    #region Multiple Updates Tests

    [Fact]
    public void BaseEntity_WhenPropertiesUpdatedMultipleTimes_ShouldRetainLatestValues()
    {
        // Arrange
        var entity = new TestEntity();

        // Act
        entity.CreatedBy = "user1@example.com";
        entity.CreatedBy = "user2@example.com";
        entity.ModifiedBy = "modifier1@example.com";
        entity.ModifiedBy = "modifier2@example.com";

        // Assert
        entity.CreatedBy.Should().Be("user2@example.com");
        entity.ModifiedBy.Should().Be("modifier2@example.com");
    }

    [Fact]
    public void BaseEntity_WhenIsDeletedToggledMultipleTimes_ShouldRetainLatestValue()
    {
        // Arrange
        var entity = new TestEntity();

        // Act
        entity.IsDeleted = true;
        entity.IsDeleted = false;
        entity.IsDeleted = true;

        // Assert
        entity.IsDeleted.Should().BeTrue();
    }

    #endregion
}
