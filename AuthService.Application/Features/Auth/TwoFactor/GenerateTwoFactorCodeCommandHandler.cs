using System.IO;
using AuthService.Application.Common.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace AuthService.Application.Features.Auth.TwoFactor;

public class GenerateTwoFactorCodeCommandHandler : IRequestHandler<GenerateTwoFactorCodeCommand, bool>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IEmailService _email;
    private readonly ILogger<GenerateTwoFactorCodeCommandHandler> _logger;

    public GenerateTwoFactorCodeCommandHandler(
        UserManager<ApplicationUser> userManager,
        IEmailService email,
        ILogger<GenerateTwoFactorCodeCommandHandler> logger)
    {
        _userManager = userManager;
        _email = email;
        _logger = logger;
    }

    public async Task<bool> Handle(GenerateTwoFactorCodeCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(request.UserId)
            ?? throw new InvalidOperationException("User not found.");

        if (!user.TwoFactorEnabled)
        {
            user.TwoFactorEnabled = true;
            await _userManager.UpdateAsync(user);
        }

        var code = await _userManager.GenerateTwoFactorTokenAsync(user, TokenOptions.DefaultEmailProvider);

        // Send two-factor code email using template
        await SendTwoFactorCodeEmailAsync(user, code, cancellationToken);

        return true;
    }

    private async Task SendTwoFactorCodeEmailAsync(
        ApplicationUser user,
        string code,
        CancellationToken cancellationToken)
    {
        try
        {
            // Prepare user's full name
            var fullName = $"{user.FirstName} {user.LastName}".Trim();
            if (string.IsNullOrWhiteSpace(fullName))
            {
                fullName = user.Email ?? "User";
            }

            // Get current date and time
            var requestDateTime = DateTime.UtcNow;
            var requestTime = requestDateTime.ToString("MMMM dd, yyyy hh:mm tt") + " UTC";

            // Load email template
            var baseDir = AppContext.BaseDirectory;
            var templatePath = Path.Combine(baseDir, "EmailTemplates", "mail-twofactor-code.html");

            if (!File.Exists(templatePath))
            {
                _logger.LogWarning("Two-factor code email template not found at {TemplatePath}. Sending plain email instead.", templatePath);

                // Fallback to plain email
                await _email.SendAsync(
                    user.Email!,
                    "Your Two-Factor Authentication Code",
                    $"Your security code is: {code}. This code will expire in 5 minutes.",
                    cancellationToken);

                return;
            }

            var html = await File.ReadAllTextAsync(templatePath, cancellationToken);

            // Replace placeholders in template
            html = html.Replace("John Doe", fullName);
            html = html.Replace("{{TwoFactorCode}}", code);
            html = html.Replace("{{UserEmail}}", user.Email ?? "");
            html = html.Replace("{{RequestTime}}", requestTime);
            html = html.Replace("{{IpAddress}}", "System Generated");
            html = html.Replace("{{SupportUrl}}", "mailto:support@example.com");

            // Extract domain from user email for support email
            var emailDomain = user.Email?.Split('@').LastOrDefault() ?? "example.com";
            html = html.Replace("support@example.com", $"support@{emailDomain}");

            // Send the email
            await _email.SendAsync(
                user.Email!,
                "Your Two-Factor Authentication Code",
                html,
                cancellationToken);

            _logger.LogInformation(
                "Two-factor code email sent successfully to {Email} for user {UserId}",
                user.Email,
                user.Id);
        }
        catch (Exception ex)
        {
            // Log the error but don't fail the code generation operation
            _logger.LogError(
                ex,
                "Failed to send two-factor code email to {Email} for user {UserId}",
                user.Email,
                user.Id);

            // Re-throw to ensure the user knows the email failed
            throw;
        }
    }
}