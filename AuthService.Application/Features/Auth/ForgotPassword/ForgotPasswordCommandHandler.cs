using System.IO;
using System.Net;
using AuthService.Application.Common.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace AuthService.Application.Features.Auth.ForgotPassword;
public class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand, bool>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IEmailService _email;
    public ForgotPasswordCommandHandler(UserManager<ApplicationUser> userManager, IEmailService email)
    {
        _userManager = userManager;
        _email = email;
    }
    public async Task<bool> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null)
        {
            // don't reveal user existence
            return true;
        }
        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var encodedToken = WebUtility.UrlEncode(token);
        var callbackUrl = $"{request.CallbackBaseUrl}?email={WebUtility.UrlEncode(user.Email)}&token={encodedToken}";
        var fullName = $"{user.FirstName} {user.LastName}".Trim();
        if (string.IsNullOrWhiteSpace(fullName))
        {
            fullName = user.Email ?? "User";
        }
        var ipText = string.IsNullOrWhiteSpace(request.IpAddress) ? "an unknown IP address" : request.IpAddress;
        // Load HTML template
        var baseDir = AppContext.BaseDirectory;
        var templatePath = Path.Combine(baseDir, "EmailTemplates", "mail-reset-password.html");
        var html = await File.ReadAllTextAsync(templatePath, cancellationToken);
        // Replace placeholders
        html = html.Replace("https://pixinvent.com?reset_password_url", callbackUrl);
        html = html.Replace("John Doe", fullName);
        html = html.Replace("john@example.com", user.Email ?? string.Empty);
        html = html.Replace("49.34.185.199", ipText);
        html = html.Replace("(ID: 8632698)", string.Empty);
        await _email.SendAsync(user.Email!, "Reset your password", html, cancellationToken);
        return true;
    }
}
