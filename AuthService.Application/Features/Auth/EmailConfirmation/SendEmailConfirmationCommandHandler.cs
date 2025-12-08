using System.IO;
using System.Net;
using AuthService.Application.Common.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace AuthService.Application.Features.Auth.EmailConfirmation;
public class SendEmailConfirmationCommandHandler : IRequestHandler<SendEmailConfirmationCommand, bool>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IEmailService _email;
    public SendEmailConfirmationCommandHandler(UserManager<ApplicationUser> userManager, IEmailService email)
    {
        _userManager = userManager;
        _email = email;
    }
    public async Task<bool> Handle(SendEmailConfirmationCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null)
        {
            // don't reveal user existence
            return true;
        }
        if (user.EmailConfirmed)
        {
            // signal already confirmed to caller
            return false;
        }
        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        var encodedToken = WebUtility.UrlEncode(token);
        var callbackUrl = $"{request.CallbackBaseUrl}?email={WebUtility.UrlEncode(user.Email)}&token={encodedToken}";
        var fullName = $"{user.FirstName} {user.LastName}".Trim();
        if (string.IsNullOrWhiteSpace(fullName))
        {
            fullName = user.Email ?? "User";
        }
        var baseDir = AppContext.BaseDirectory;
        var templatePath = Path.Combine(baseDir, "EmailTemplates", "confirmmail-verify-email.html");
        var html = await File.ReadAllTextAsync(templatePath, cancellationToken);
        // Replace placeholders
        html = html.Replace("https://pixinvent.com?verification_url", callbackUrl);
        html = html.Replace("John Doe", fullName);
        // keep whatever support email placeholder, replace with from-address
        html = html.Replace("support@example.com", "support@" + (user.Email?.Split('@')[1] ?? "example.com"));
        await _email.SendAsync(user.Email!, "Confirm your email", html, cancellationToken);
        return true;
    }
}
