using System.IO;
using AuthService.Application.Common.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace AuthService.Application.Features.Auth.TwoFactor;

public class EnableTwoFactorCommandHandler : IRequestHandler<EnableTwoFactorCommand, bool>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IEmailService _emailService;
    private readonly ILogger<EnableTwoFactorCommandHandler> _logger;

    public EnableTwoFactorCommandHandler(
        UserManager<ApplicationUser> userManager,
        IEmailService emailService,
        ILogger<EnableTwoFactorCommandHandler> logger)
    {
        _userManager = userManager;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<bool> Handle(EnableTwoFactorCommand request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(request.UserId, out var userGuid))
        {
            throw new InvalidOperationException("Invalid user identifier.");
        }

        var user = await _userManager.FindByIdAsync(request.UserId);
        if (user is null)
        {
            throw new InvalidOperationException("User not found.");
        }

        if (!user.TwoFactorEnabled)
        {
            user.TwoFactorEnabled = true;
            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                throw new InvalidOperationException("Failed to enable two-factor authentication.");
            }

            // Send email notification
            await SendTwoFactorEnabledEmailAsync(user, request.IpAddress, cancellationToken);
        }

        return true;
    }

    private async Task SendTwoFactorEnabledEmailAsync(
        ApplicationUser user,
        string? ipAddress,
        CancellationToken cancellationToken)
    {
        try
        {
            var fullName = $"{user.FirstName} {user.LastName}".Trim();
            if (string.IsNullOrWhiteSpace(fullName))
            {
                fullName = user.Email ?? "User";
            }

            var activationDateTime = DateTime.UtcNow;
            var activationDate = activationDateTime.ToString("MMMM dd, yyyy");
            var activationTime = activationDateTime.ToString("hh:mm tt") + " UTC";
            var ipText = string.IsNullOrWhiteSpace(ipAddress) ? "Unknown" : ipAddress;

            var baseDir = AppContext.BaseDirectory;
            var templatePath = Path.Combine(baseDir, "EmailTemplates", "mail-twofactor-enabled.html");

            if (!File.Exists(templatePath))
            {
                _logger.LogWarning("Two-factor enabled template not found at {TemplatePath}", templatePath);
                return;
            }

            var html = await File.ReadAllTextAsync(templatePath, cancellationToken);

            // Replace logo placeholder with your actual hosted logo URL
            html = html.Replace(
                "https://via.placeholder.com/140x40/667eea/ffffff?text=YOUR+LOGO",
                "https://yourdomain.com/assets/logo.png");

            // Replace placeholders
            html = html.Replace("John Doe", fullName);
            html = html.Replace("{{UserEmail}}", user.Email ?? "");
            html = html.Replace("{{ActivationDate}}", activationDate);
            html = html.Replace("{{ActivationTime}}", activationTime);
            html = html.Replace("{{IpAddress}}", ipText);
            html = html.Replace("{{SupportUrl}}", "mailto:support@example.com");

            var emailDomain = user.Email?.Split('@').LastOrDefault() ?? "example.com";
            html = html.Replace("support@example.com", $"support@{emailDomain}");

            await _emailService.SendAsync(
                user.Email!,
                "Two-Factor Authentication Enabled",
                html,
                cancellationToken);

            _logger.LogInformation(
                "Two-factor enabled notification sent to {Email} for user {UserId}",
                user.Email,
                user.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to send two-factor enabled notification to {Email} for user {UserId}",
                user.Email,
                user.Id);
        }
    }
}