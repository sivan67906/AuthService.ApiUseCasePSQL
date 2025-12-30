using AuthService.Application.Common.Behaviors;
using FluentValidation.Results;

namespace AuthService.Tests.Unit.Application.Common.Behaviors;

public class ValidationBehaviorTests
{
    public sealed record TestRequest(string Value) : IRequest<string>;

    [Fact]
    public async Task Handle_WhenNoValidators_ShouldCallNext()
    {
        var validators = Array.Empty<IValidator<TestRequest>>();
        var behavior = new ValidationBehavior<TestRequest, string>(validators);

        var nextCalled = false;

        RequestHandlerDelegate<string> next = _ =>
        {
            nextCalled = true;
            return Task.FromResult("ok");
        };

        var result = await behavior.Handle(new TestRequest("x"), next, CancellationToken.None);

        nextCalled.Should().BeTrue();
        result.Should().Be("ok");
    }

    [Fact]
    public async Task Handle_WhenValidatorFails_ShouldThrowValidationExceptionWithDelimitedMessages()
    {
        var validator = new Mock<IValidator<TestRequest>>();
        validator
            .Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<TestRequest>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(new[]
            {
                new ValidationFailure("Value", "Error 1"),
                new ValidationFailure("Value", "Error 2")
            }));

        var behavior = new ValidationBehavior<TestRequest, string>(new[] { validator.Object });

        RequestHandlerDelegate<string> next = _ => Task.FromResult("ok");

        var act = () => behavior.Handle(new TestRequest(""), next, CancellationToken.None);

        var ex = await act.Should().ThrowAsync<ValidationException>();
        ex.Which.Message.Should().Be("Error 1|||Error 2");
    }
}
