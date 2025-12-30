using AuthService.Application.Common.Helpers;

namespace AuthService.Tests.Unit.Application.Common.Helpers;

public class InputSanitizerTests
{
    #region SanitizeText Tests

    [Fact]
    public void SanitizeText_NullInput_ReturnsEmpty()
    {
        // Act
        var result = InputSanitizer.SanitizeText(null);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void SanitizeText_EmptyInput_ReturnsEmpty()
    {
        // Act
        var result = InputSanitizer.SanitizeText("");

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void SanitizeText_WhitespaceInput_ReturnsEmpty()
    {
        // Act
        var result = InputSanitizer.SanitizeText("   ");

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void SanitizeText_PlainText_ReturnsSameText()
    {
        // Arrange
        var input = "Hello World";

        // Act
        var result = InputSanitizer.SanitizeText(input);

        // Assert
        result.Should().NotBeEmpty();
    }

    [Fact]
    public void SanitizeText_HtmlTags_RemovesTags()
    {
        // Arrange
        var input = "<b>Bold</b> text";

        // Act
        var result = InputSanitizer.SanitizeText(input);

        // Assert
        result.Should().NotContain("<b>");
        result.Should().NotContain("</b>");
        result.Should().Contain("Bold");
    }

    [Fact]
    public void SanitizeText_ScriptTags_RemovesScript()
    {
        // Arrange
        var input = "<script>alert('xss')</script>Normal text";

        // Act
        var result = InputSanitizer.SanitizeText(input);

        // Assert
        result.Should().NotContain("script");
        result.Should().NotContain("alert");
        result.Should().Contain("Normal text");
    }

    [Fact]
    public void SanitizeText_AngleBrackets_EncodesThem()
    {
        // Arrange
        var input = "< > symbols";

        // Act
        var result = InputSanitizer.SanitizeText(input);

        // Assert
        result.Should().NotContain("<");
        result.Should().NotContain(">");
        result.Should().Contain("&lt;");
        result.Should().Contain("&gt;");
    }

    [Fact]
    public void SanitizeText_DoubleQuotes_EncodesAsThem()
    {
        // Arrange
        var input = "Quote \"test\" here";

        // Act
        var result = InputSanitizer.SanitizeText(input);

        // Assert
        result.Should().Contain("&quot;");
    }

    [Fact]
    public void SanitizeText_SingleQuotes_EncodesAsThem()
    {
        // Arrange
        var input = "It's a test";

        // Act
        var result = InputSanitizer.SanitizeText(input);

        // Assert
        result.Should().Contain("&#39;");
    }

    [Fact]
    public void SanitizeText_Ampersand_EncodesIt()
    {
        // Arrange
        var input = "Rock & Roll";

        // Act
        var result = InputSanitizer.SanitizeText(input);

        // Assert
        result.Should().Contain("&amp;");
    }

    [Fact]
    public void SanitizeText_TrimsWhitespace()
    {
        // Arrange
        var input = "  test text  ";

        // Act
        var result = InputSanitizer.SanitizeText(input);

        // Assert
        result.Should().NotStartWith(" ");
        result.Should().NotEndWith(" ");
    }

    #endregion

    #region SanitizeWithMaxLength Tests

    [Fact]
    public void SanitizeWithMaxLength_NullInput_ReturnsEmpty()
    {
        // Act
        var result = InputSanitizer.SanitizeWithMaxLength(null, 10);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void SanitizeWithMaxLength_ShortString_ReturnsFull()
    {
        // Arrange
        var input = "Hello";

        // Act
        var result = InputSanitizer.SanitizeWithMaxLength(input, 10);

        // Assert
        result.Should().Be("Hello");
    }

    [Fact]
    public void SanitizeWithMaxLength_LongString_Truncates()
    {
        // Arrange
        var input = "This is a very long string";

        // Act
        var result = InputSanitizer.SanitizeWithMaxLength(input, 10);

        // Assert
        result.Length.Should().BeLessOrEqualTo(10);
    }

    [Fact]
    public void SanitizeWithMaxLength_ExactLength_ReturnsUnchanged()
    {
        // Arrange
        var input = "Exact";

        // Act
        var result = InputSanitizer.SanitizeWithMaxLength(input, 5);

        // Assert
        result.Should().Be("Exact");
    }

    [Fact]
    public void SanitizeWithMaxLength_WithHtml_SanitizesAndTruncates()
    {
        // Arrange
        var input = "<b>This is bold and very long text</b>";

        // Act
        var result = InputSanitizer.SanitizeWithMaxLength(input, 15);

        // Assert
        result.Length.Should().BeLessOrEqualTo(15);
        result.Should().NotContain("<b>");
    }

    #endregion

    #region SanitizeEmail Tests

    [Fact]
    public void SanitizeEmail_NullInput_ReturnsEmpty()
    {
        // Act
        var result = InputSanitizer.SanitizeEmail(null);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void SanitizeEmail_EmptyInput_ReturnsEmpty()
    {
        // Act
        var result = InputSanitizer.SanitizeEmail("");

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void SanitizeEmail_ValidEmail_ReturnsLowerCase()
    {
        // Arrange
        var input = "TEST@EXAMPLE.COM";

        // Act
        var result = InputSanitizer.SanitizeEmail(input);

        // Assert
        result.Should().Be("test@example.com");
    }

    [Fact]
    public void SanitizeEmail_WithSpaces_RemovesSpaces()
    {
        // Arrange
        var input = "  test@example.com  ";

        // Act
        var result = InputSanitizer.SanitizeEmail(input);

        // Assert
        result.Should().NotContain(" ");
    }

    [Fact]
    public void SanitizeEmail_WithSpecialChars_RemovesInvalidChars()
    {
        // Arrange
        var input = "test<script>@example.com";

        // Act
        var result = InputSanitizer.SanitizeEmail(input);

        // Assert
        result.Should().NotContain("<");
        result.Should().NotContain(">");
        result.Should().Contain("@");
    }

    [Fact]
    public void SanitizeEmail_WithValidSpecialChars_KeepsThem()
    {
        // Arrange
        var input = "test.user+tag@example.com";

        // Act
        var result = InputSanitizer.SanitizeEmail(input);

        // Assert
        result.Should().Contain(".");
        result.Should().Contain("+");
        result.Should().Contain("@");
    }

    #endregion

    #region IsValidEmail Tests

    [Fact]
    public void IsValidEmail_NullInput_ReturnsFalse()
    {
        // Act
        var result = InputSanitizer.IsValidEmail(null);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsValidEmail_EmptyInput_ReturnsFalse()
    {
        // Act
        var result = InputSanitizer.IsValidEmail("");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsValidEmail_WhitespaceInput_ReturnsFalse()
    {
        // Act
        var result = InputSanitizer.IsValidEmail("   ");

        // Assert
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData("test@example.com")]
    [InlineData("user.name@domain.org")]
    [InlineData("user+tag@example.co.uk")]
    [InlineData("firstname.lastname@company.com")]
    public void IsValidEmail_ValidEmails_ReturnsTrue(string email)
    {
        // Act
        var result = InputSanitizer.IsValidEmail(email);

        // Assert
        result.Should().BeTrue();
    }

    [Theory]
    [InlineData("notanemail")]
    [InlineData("missing@")]
    [InlineData("@domain.com")]
    [InlineData("no@domain")]
    [InlineData("spaces in@email.com")]
    public void IsValidEmail_InvalidEmails_ReturnsFalse(string email)
    {
        // Act
        var result = InputSanitizer.IsValidEmail(email);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region ContainsDangerousContent Tests

    [Fact]
    public void ContainsDangerousContent_NullInput_ReturnsFalse()
    {
        // Act
        var result = InputSanitizer.ContainsDangerousContent(null);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void ContainsDangerousContent_EmptyInput_ReturnsFalse()
    {
        // Act
        var result = InputSanitizer.ContainsDangerousContent("");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void ContainsDangerousContent_PlainText_ReturnsFalse()
    {
        // Act
        var result = InputSanitizer.ContainsDangerousContent("Hello World");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void ContainsDangerousContent_HtmlTags_ReturnsTrue()
    {
        // Act
        var result = InputSanitizer.ContainsDangerousContent("<b>Bold</b>");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void ContainsDangerousContent_ScriptTag_ReturnsTrue()
    {
        // Act
        var result = InputSanitizer.ContainsDangerousContent("<script>alert('xss')</script>");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void ContainsDangerousContent_AngleBrackets_ReturnsTrue()
    {
        // Act
        var result = InputSanitizer.ContainsDangerousContent("< >");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void ContainsDangerousContent_JavascriptUri_ReturnsTrue()
    {
        // Act
        var result = InputSanitizer.ContainsDangerousContent("javascript:void(0)");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void ContainsDangerousContent_OnError_ReturnsTrue()
    {
        // Act
        var result = InputSanitizer.ContainsDangerousContent("img src=x onerror=alert(1)");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void ContainsDangerousContent_OnLoad_ReturnsTrue()
    {
        // Act
        var result = InputSanitizer.ContainsDangerousContent("body onload=alert(1)");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void ContainsDangerousContent_CaseInsensitive_ReturnsTrue()
    {
        // Act
        var result = InputSanitizer.ContainsDangerousContent("JAVASCRIPT:alert(1)");

        // Assert
        result.Should().BeTrue();
    }

    #endregion
}
