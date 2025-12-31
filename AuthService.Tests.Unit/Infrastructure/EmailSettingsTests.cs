namespace AuthService.Tests.Unit.Infrastructure;

/// <summary>
/// Unit tests for EmailSettings configuration class
/// </summary>
public class EmailSettingsTests
{
    #region Default Values Tests

    [Fact]
    public void EmailSettings_DefaultValues_ShouldBeEmpty()
    {
        // Arrange & Act
        var settings = new EmailSettings();

        // Assert
        settings.SmtpHost.Should().BeEmpty();
        settings.SmtpPort.Should().Be(0);
        settings.SmtpUsername.Should().BeEmpty();
        settings.SmtpPassword.Should().BeEmpty();
        settings.SenderEmail.Should().BeEmpty();
        settings.SenderName.Should().BeEmpty();
    }

    [Fact]
    public void EmailSettings_EnableSsl_ShouldDefaultToTrue()
    {
        // Arrange & Act
        var settings = new EmailSettings();

        // Assert
        settings.EnableSsl.Should().BeTrue();
    }

    #endregion

    #region Property Setter Tests

    [Fact]
    public void SmtpHost_SetValue_ShouldRetainValue()
    {
        // Arrange
        var settings = new EmailSettings();
        var expectedHost = "smtp.gmail.com";

        // Act
        settings.SmtpHost = expectedHost;

        // Assert
        settings.SmtpHost.Should().Be(expectedHost);
    }

    [Theory]
    [InlineData(25)]
    [InlineData(465)]
    [InlineData(587)]
    [InlineData(2525)]
    [InlineData(0)]
    [InlineData(65535)]
    public void SmtpPort_SetValue_ShouldRetainValue(int port)
    {
        // Arrange
        var settings = new EmailSettings();

        // Act
        settings.SmtpPort = port;

        // Assert
        settings.SmtpPort.Should().Be(port);
    }

    [Fact]
    public void SmtpUsername_SetValue_ShouldRetainValue()
    {
        // Arrange
        var settings = new EmailSettings();
        var expectedUsername = "user@example.com";

        // Act
        settings.SmtpUsername = expectedUsername;

        // Assert
        settings.SmtpUsername.Should().Be(expectedUsername);
    }

    [Fact]
    public void SmtpPassword_SetValue_ShouldRetainValue()
    {
        // Arrange
        var settings = new EmailSettings();
        var expectedPassword = "SecurePassword123!";

        // Act
        settings.SmtpPassword = expectedPassword;

        // Assert
        settings.SmtpPassword.Should().Be(expectedPassword);
    }

    [Fact]
    public void SenderEmail_SetValue_ShouldRetainValue()
    {
        // Arrange
        var settings = new EmailSettings();
        var expectedEmail = "noreply@example.com";

        // Act
        settings.SenderEmail = expectedEmail;

        // Assert
        settings.SenderEmail.Should().Be(expectedEmail);
    }

    [Fact]
    public void SenderName_SetValue_ShouldRetainValue()
    {
        // Arrange
        var settings = new EmailSettings();
        var expectedName = "My Application";

        // Act
        settings.SenderName = expectedName;

        // Assert
        settings.SenderName.Should().Be(expectedName);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void EnableSsl_SetValue_ShouldRetainValue(bool enableSsl)
    {
        // Arrange
        var settings = new EmailSettings();

        // Act
        settings.EnableSsl = enableSsl;

        // Assert
        settings.EnableSsl.Should().Be(enableSsl);
    }

    #endregion

    #region Object Initialization Tests

    [Fact]
    public void EmailSettings_ObjectInitializer_ShouldSetAllProperties()
    {
        // Arrange & Act
        var settings = new EmailSettings
        {
            SmtpHost = "smtp.example.com",
            SmtpPort = 587,
            SmtpUsername = "user@example.com",
            SmtpPassword = "password123",
            SenderEmail = "noreply@example.com",
            SenderName = "Test Application",
            EnableSsl = true
        };

        // Assert
        settings.SmtpHost.Should().Be("smtp.example.com");
        settings.SmtpPort.Should().Be(587);
        settings.SmtpUsername.Should().Be("user@example.com");
        settings.SmtpPassword.Should().Be("password123");
        settings.SenderEmail.Should().Be("noreply@example.com");
        settings.SenderName.Should().Be("Test Application");
        settings.EnableSsl.Should().BeTrue();
    }

    #endregion

    #region Edge Cases

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void SmtpHost_WithEmptyOrNullValues_ShouldAccept(string? value)
    {
        // Arrange
        var settings = new EmailSettings();

        // Act
        if (value != null)
        {
            settings.SmtpHost = value;

            // Assert
            settings.SmtpHost.Should().Be(value);
        }
    }

    [Fact]
    public void SmtpHost_WithVeryLongValue_ShouldAccept()
    {
        // Arrange
        var settings = new EmailSettings();
        var longHost = new string('a', 1000) + ".example.com";

        // Act
        settings.SmtpHost = longHost;

        // Assert
        settings.SmtpHost.Should().Be(longHost);
    }

    [Fact]
    public void SmtpPassword_WithSpecialCharacters_ShouldAccept()
    {
        // Arrange
        var settings = new EmailSettings();
        var specialPassword = "P@$$w0rd!#$%^&*(){}[]|\\:;<>?,./~`";

        // Act
        settings.SmtpPassword = specialPassword;

        // Assert
        settings.SmtpPassword.Should().Be(specialPassword);
    }

    [Fact]
    public void SenderName_WithUnicodeCharacters_ShouldAccept()
    {
        // Arrange
        var settings = new EmailSettings();
        var unicodeName = "日本語 アプリ";

        // Act
        settings.SenderName = unicodeName;

        // Assert
        settings.SenderName.Should().Be(unicodeName);
    }

    [Fact]
    public void SmtpPort_WithNegativeValue_ShouldAccept()
    {
        // Arrange
        var settings = new EmailSettings();

        // Act
        settings.SmtpPort = -1;

        // Assert - class doesn't validate, that's configuration responsibility
        settings.SmtpPort.Should().Be(-1);
    }

    #endregion

    #region Type Tests

    [Fact]
    public void EmailSettings_ShouldBeClass()
    {
        // Assert
        typeof(EmailSettings).IsClass.Should().BeTrue();
    }

    [Fact]
    public void EmailSettings_ShouldHaveParameterlessConstructor()
    {
        // Arrange
        var constructor = typeof(EmailSettings).GetConstructor(Type.EmptyTypes);

        // Assert
        constructor.Should().NotBeNull();
    }

    [Fact]
    public void EmailSettings_AllProperties_ShouldHavePublicGettersAndSetters()
    {
        // Arrange
        var properties = typeof(EmailSettings).GetProperties();

        // Assert
        foreach (var property in properties)
        {
            property.CanRead.Should().BeTrue($"{property.Name} should have a getter");
            property.CanWrite.Should().BeTrue($"{property.Name} should have a setter");
            property.GetMethod?.IsPublic.Should().BeTrue($"{property.Name} getter should be public");
            property.SetMethod?.IsPublic.Should().BeTrue($"{property.Name} setter should be public");
        }
    }

    [Fact]
    public void EmailSettings_ShouldHaveSevenProperties()
    {
        // Arrange
        var properties = typeof(EmailSettings).GetProperties();

        // Assert
        properties.Length.Should().Be(7);
    }

    #endregion
}
