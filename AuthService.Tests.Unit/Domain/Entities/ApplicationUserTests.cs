namespace AuthService.Tests.Unit.Domain.Entities;

/// <summary>
/// Unit tests for ApplicationUser entity
/// </summary>
public class ApplicationUserTests
{
    #region Constructor and Default Values Tests

    [Fact]
    public void ApplicationUser_WhenCreated_ShouldHaveDefaultValues()
    {
        // Act
        var user = new ApplicationUser();

        // Assert
        user.IsActive.Should().BeTrue();
        user.IsDeleted.Should().BeFalse();
        user.AuthenticatorEnabled.Should().BeFalse();
        user.AuthenticatorSecretKey.Should().BeNull();
        user.FirstName.Should().BeNull();
        user.LastName.Should().BeNull();
    }

    [Fact]
    public void ApplicationUser_WhenCreated_ShouldHaveCreatedAtSetToUtcNow()
    {
        // Arrange
        var beforeCreation = DateTime.UtcNow;

        // Act
        var user = new ApplicationUser();

        // Assert
        var afterCreation = DateTime.UtcNow;
        user.CreatedAt.Should().BeOnOrAfter(beforeCreation);
        user.CreatedAt.Should().BeOnOrBefore(afterCreation);
    }

    [Fact]
    public void ApplicationUser_WhenCreated_ShouldHaveEmptyAddressesCollection()
    {
        // Act
        var user = new ApplicationUser();

        // Assert
        user.Addresses.Should().NotBeNull();
        user.Addresses.Should().BeEmpty();
    }

    [Fact]
    public void ApplicationUser_WhenCreated_ShouldHaveEmptyRefreshTokensCollection()
    {
        // Act
        var user = new ApplicationUser();

        // Assert
        user.RefreshTokens.Should().NotBeNull();
        user.RefreshTokens.Should().BeEmpty();
    }

    [Fact]
    public void ApplicationUser_WhenCreated_ShouldHaveEmptyUserRoleMappingsCollection()
    {
        // Act
        var user = new ApplicationUser();

        // Assert
        user.UserRoleMappings.Should().NotBeNull();
        user.UserRoleMappings.Should().BeEmpty();
    }

    #endregion

    #region Interface Implementation Tests

    [Fact]
    public void ApplicationUser_ShouldImplementIAuditableEntity()
    {
        // Arrange
        var user = new ApplicationUser();

        // Assert
        user.Should().BeAssignableTo<IAuditableEntity>();
    }

    [Fact]
    public void ApplicationUser_ShouldImplementISoftDeletable()
    {
        // Arrange
        var user = new ApplicationUser();

        // Assert
        user.Should().BeAssignableTo<ISoftDeletable>();
    }

    #endregion

    #region Property Assignment Tests - Positive

    [Fact]
    public void ApplicationUser_WhenFirstNameAssigned_ShouldRetainValue()
    {
        // Arrange
        var user = new ApplicationUser();
        const string firstName = "John";

        // Act
        user.FirstName = firstName;

        // Assert
        user.FirstName.Should().Be(firstName);
    }

    [Fact]
    public void ApplicationUser_WhenLastNameAssigned_ShouldRetainValue()
    {
        // Arrange
        var user = new ApplicationUser();
        const string lastName = "Doe";

        // Act
        user.LastName = lastName;

        // Assert
        user.LastName.Should().Be(lastName);
    }

    [Fact]
    public void ApplicationUser_WhenIsActiveSetToFalse_ShouldRetainValue()
    {
        // Arrange
        var user = new ApplicationUser();

        // Act
        user.IsActive = false;

        // Assert
        user.IsActive.Should().BeFalse();
    }

    [Fact]
    public void ApplicationUser_WhenAuthenticatorEnabledSetToTrue_ShouldRetainValue()
    {
        // Arrange
        var user = new ApplicationUser();

        // Act
        user.AuthenticatorEnabled = true;

        // Assert
        user.AuthenticatorEnabled.Should().BeTrue();
    }

    [Fact]
    public void ApplicationUser_WhenAuthenticatorSecretKeyAssigned_ShouldRetainValue()
    {
        // Arrange
        var user = new ApplicationUser();
        const string secretKey = "ABCDEFGHIJKLMNOP";

        // Act
        user.AuthenticatorSecretKey = secretKey;

        // Assert
        user.AuthenticatorSecretKey.Should().Be(secretKey);
    }

    [Fact]
    public void ApplicationUser_WhenEmailAssigned_ShouldRetainValue()
    {
        // Arrange
        var user = new ApplicationUser();
        const string email = "test@example.com";

        // Act
        user.Email = email;

        // Assert
        user.Email.Should().Be(email);
    }

    [Fact]
    public void ApplicationUser_WhenUserNameAssigned_ShouldRetainValue()
    {
        // Arrange
        var user = new ApplicationUser();
        const string userName = "testuser";

        // Act
        user.UserName = userName;

        // Assert
        user.UserName.Should().Be(userName);
    }

    [Fact]
    public void ApplicationUser_WhenPhoneNumberAssigned_ShouldRetainValue()
    {
        // Arrange
        var user = new ApplicationUser();
        const string phoneNumber = "+1234567890";

        // Act
        user.PhoneNumber = phoneNumber;

        // Assert
        user.PhoneNumber.Should().Be(phoneNumber);
    }

    #endregion

    #region Audit Fields Tests

    [Fact]
    public void ApplicationUser_WhenCreatedByAssigned_ShouldRetainValue()
    {
        // Arrange
        var user = new ApplicationUser();
        const string createdBy = "admin@example.com";

        // Act
        user.CreatedBy = createdBy;

        // Assert
        user.CreatedBy.Should().Be(createdBy);
    }

    [Fact]
    public void ApplicationUser_WhenUpdatedAtAssigned_ShouldRetainValue()
    {
        // Arrange
        var user = new ApplicationUser();
        var updatedAt = DateTime.UtcNow;

        // Act
        user.UpdatedAt = updatedAt;

        // Assert
        user.UpdatedAt.Should().Be(updatedAt);
    }

    [Fact]
    public void ApplicationUser_WhenModifiedByAssigned_ShouldRetainValue()
    {
        // Arrange
        var user = new ApplicationUser();
        const string modifiedBy = "user@example.com";

        // Act
        user.ModifiedBy = modifiedBy;

        // Assert
        user.ModifiedBy.Should().Be(modifiedBy);
    }

    #endregion

    #region Navigation Properties Tests

    [Fact]
    public void ApplicationUser_WhenAddressAdded_ShouldContainAddress()
    {
        // Arrange
        var user = new ApplicationUser();
        var address = new UserAddress
        {
            UserId = user.Id,
            Line1 = "123 Main St",
            City = "New York",
            State = "NY",
            PostalCode = "10001",
            Country = "USA"
        };

        // Act
        user.Addresses.Add(address);

        // Assert
        user.Addresses.Should().HaveCount(1);
        user.Addresses.Should().Contain(address);
    }

    [Fact]
    public void ApplicationUser_WhenMultipleAddressesAdded_ShouldContainAllAddresses()
    {
        // Arrange
        var user = new ApplicationUser();
        var address1 = new UserAddress { Line1 = "Address 1", City = "City1", State = "State1", PostalCode = "11111", Country = "Country1" };
        var address2 = new UserAddress { Line1 = "Address 2", City = "City2", State = "State2", PostalCode = "22222", Country = "Country2" };

        // Act
        user.Addresses.Add(address1);
        user.Addresses.Add(address2);

        // Assert
        user.Addresses.Should().HaveCount(2);
    }

    [Fact]
    public void ApplicationUser_WhenRefreshTokenAdded_ShouldContainToken()
    {
        // Arrange
        var user = new ApplicationUser();
        var refreshToken = new UserRefreshToken
        {
            UserId = user.Id,
            Token = "test-token",
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        };

        // Act
        user.RefreshTokens.Add(refreshToken);

        // Assert
        user.RefreshTokens.Should().HaveCount(1);
        user.RefreshTokens.Should().Contain(refreshToken);
    }

    [Fact]
    public void ApplicationUser_WhenUserRoleMappingAdded_ShouldContainMapping()
    {
        // Arrange
        var user = new ApplicationUser();
        var roleMapping = new UserRoleMapping
        {
            UserId = user.Id,
            RoleId = Guid.NewGuid()
        };

        // Act
        user.UserRoleMappings.Add(roleMapping);

        // Assert
        user.UserRoleMappings.Should().HaveCount(1);
        user.UserRoleMappings.Should().Contain(roleMapping);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void ApplicationUser_WhenFirstNameSetToEmptyString_ShouldAcceptValue()
    {
        // Arrange
        var user = new ApplicationUser();

        // Act
        user.FirstName = string.Empty;

        // Assert
        user.FirstName.Should().BeEmpty();
    }

    [Fact]
    public void ApplicationUser_WhenFirstNameSetToWhitespace_ShouldAcceptValue()
    {
        // Arrange
        var user = new ApplicationUser();

        // Act
        user.FirstName = "   ";

        // Assert
        user.FirstName.Should().Be("   ");
    }

    [Fact]
    public void ApplicationUser_WhenFirstNameSetToNull_ShouldAcceptValue()
    {
        // Arrange
        var user = new ApplicationUser { FirstName = "John" };

        // Act
        user.FirstName = null;

        // Assert
        user.FirstName.Should().BeNull();
    }

    [Fact]
    public void ApplicationUser_WhenFirstNameHasSpecialCharacters_ShouldAcceptValue()
    {
        // Arrange
        var user = new ApplicationUser();
        const string firstName = "José María O'Brien-Smith";

        // Act
        user.FirstName = firstName;

        // Assert
        user.FirstName.Should().Be(firstName);
    }

    [Fact]
    public void ApplicationUser_WhenFirstNameHasUnicodeCharacters_ShouldAcceptValue()
    {
        // Arrange
        var user = new ApplicationUser();
        const string firstName = "田中太郎";

        // Act
        user.FirstName = firstName;

        // Assert
        user.FirstName.Should().Be(firstName);
    }

    [Fact]
    public void ApplicationUser_WhenAuthenticatorSecretKeySetToEmptyString_ShouldAcceptValue()
    {
        // Arrange
        var user = new ApplicationUser();

        // Act
        user.AuthenticatorSecretKey = string.Empty;

        // Assert
        user.AuthenticatorSecretKey.Should().BeEmpty();
    }

    #endregion

    #region Soft Delete Tests

    [Fact]
    public void ApplicationUser_WhenSoftDeleted_ShouldSetIsDeletedTrue()
    {
        // Arrange
        var user = new ApplicationUser();

        // Act
        user.IsDeleted = true;

        // Assert
        user.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public void ApplicationUser_WhenSoftDeletedAndRestored_ShouldSetIsDeletedFalse()
    {
        // Arrange
        var user = new ApplicationUser { IsDeleted = true };

        // Act
        user.IsDeleted = false;

        // Assert
        user.IsDeleted.Should().BeFalse();
    }

    [Fact]
    public void ApplicationUser_WhenSoftDeleted_OtherPropertiesShouldRemainIntact()
    {
        // Arrange
        var user = new ApplicationUser
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            IsActive = true
        };

        // Act
        user.IsDeleted = true;

        // Assert
        user.FirstName.Should().Be("John");
        user.LastName.Should().Be("Doe");
        user.Email.Should().Be("john.doe@example.com");
        user.IsActive.Should().BeTrue();
    }

    #endregion

    #region Two-Factor Authentication Tests

    [Fact]
    public void ApplicationUser_WhenTwoFactorEnabled_ShouldSetPropertyCorrectly()
    {
        // Arrange
        var user = new ApplicationUser();

        // Act
        user.TwoFactorEnabled = true;

        // Assert
        user.TwoFactorEnabled.Should().BeTrue();
    }

    [Fact]
    public void ApplicationUser_WhenAuthenticatorSetup_ShouldHaveBothPropertiesSet()
    {
        // Arrange
        var user = new ApplicationUser();
        const string secretKey = "ABCDEFGHIJKLMNOP";

        // Act
        user.AuthenticatorEnabled = true;
        user.AuthenticatorSecretKey = secretKey;

        // Assert
        user.AuthenticatorEnabled.Should().BeTrue();
        user.AuthenticatorSecretKey.Should().Be(secretKey);
    }

    [Fact]
    public void ApplicationUser_WhenAuthenticatorDisabled_SecretKeyShouldRemain()
    {
        // Arrange
        var user = new ApplicationUser
        {
            AuthenticatorEnabled = true,
            AuthenticatorSecretKey = "ABCDEFGHIJKLMNOP"
        };

        // Act
        user.AuthenticatorEnabled = false;

        // Assert
        user.AuthenticatorEnabled.Should().BeFalse();
        user.AuthenticatorSecretKey.Should().Be("ABCDEFGHIJKLMNOP"); // Key not cleared automatically
    }

    #endregion
}
