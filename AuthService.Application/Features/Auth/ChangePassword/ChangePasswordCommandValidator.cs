using System.IO;
using AuthService.Application.Common.Interfaces;
using AuthService.Application.Features.Auth.ForgotPassword;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace AuthService.Application.Features.Auth.ChangePassword;

public class ChangePasswordCommandValidator : AbstractValidator<ChangePasswordCommand>
{
    public ChangePasswordCommandValidator()
    {
        RuleFor(x => x.CurrentPassword).NotEmpty().WithMessage("Current Password is required.").MinimumLength(6);
        RuleFor(x => x.NewPassword).NotEmpty().WithMessage("New Password is required.").MinimumLength(6);
    }
}
