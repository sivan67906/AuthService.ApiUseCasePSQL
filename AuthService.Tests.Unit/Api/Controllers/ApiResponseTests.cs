namespace AuthService.Tests.Unit.Api.Controllers;

public class ApiResponseTests
{
    [Fact]
    public void SuccessResponse_SetsSuccessTrue_AndPopulatesDataAndMessage()
    {
        var response = ApiResponse<string>.SuccessResponse("ok", "done");

        response.Success.Should().BeTrue();
        response.Data.Should().Be("ok");
        response.Message.Should().Be("done");
        response.Errors.Should().BeNull();
    }

    [Fact]
    public void FailResponse_SetsSuccessFalse_AndPopulatesMessageErrorsAndOptionalData()
    {
        var errors = new List<string> { "e1", "e2" };
        var response = ApiResponse<int>.FailResponse("bad", errors, data: 123);

        response.Success.Should().BeFalse();
        response.Message.Should().Be("bad");
        response.Errors.Should().BeEquivalentTo(errors);
        response.Data.Should().Be(123);
    }

    [Fact]
    public void ErrorResponse_WithNullErrors_CreatesEmptyErrorsList()
    {
        var response = ApiResponse<object>.ErrorResponse("oops");

        response.Success.Should().BeFalse();
        response.Message.Should().Be("oops");
        response.Errors.Should().NotBeNull();
        response.Errors!.Should().BeEmpty();
        response.Data.Should().BeNull();
    }

    [Fact]
    public void ErrorResponse_WithSingleError_PutsErrorIntoErrorsList()
    {
        var response = ApiResponse<object>.ErrorResponse("oops", "one");

        response.Success.Should().BeFalse();
        response.Errors.Should().BeEquivalentTo(new[] { "one" });
    }

    [Fact]
    public void FailFromException_SplitsErrorsByTriplePipes_WithPrefixRemoval()
    {
        var ex = new Exception("Unable to register user: e1||| e2 |||e3");
        var response = ApiResponse<object>.FailFromException("failed", ex);

        response.Success.Should().BeFalse();
        response.Message.Should().Be("failed");
        response.Errors.Should().BeEquivalentTo(new[] { "e1", "e2", "e3" });
    }

    [Fact]
    public void FailFromException_SplitsErrorsBySemicolon_WhenNoTriplePipes()
    {
        var ex = new Exception("Validation failed: a; b ;c");
        var response = ApiResponse<object>.FailFromException("failed", ex);

        response.Errors.Should().BeEquivalentTo(new[] { "a", "b", "c" });
    }

    [Fact]
    public void FailFromException_SplitsErrorsByNewlines_WhenNoOtherDelimiter()
    {
        var ex = new Exception("Registration failed: x\n y\n\n z");
        var response = ApiResponse<object>.FailFromException("failed", ex);

        response.Errors.Should().BeEquivalentTo(new[] { "x", "y", "z" });
    }

    [Fact]
    public void FailFromException_WhenSingleMessage_UsesSingleError()
    {
        var ex = new Exception("plain error");
        var response = ApiResponse<object>.FailFromException("failed", ex);

        response.Errors.Should().BeEquivalentTo(new[] { "plain error" });
    }
}
