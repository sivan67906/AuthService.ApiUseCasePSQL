using System.IO;
using AuthService.Application.Common.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AuthService.Application.Features.Auth.Authenticator;

public sealed class DisableAuthenticatorCommandHandler : IRequestHandler<DisableAuthenticatorCommand, bool>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IEmailService _emailService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<DisableAuthenticatorCommandHandler> _logger;

    public DisableAuthenticatorCommandHandler(
        UserManager<ApplicationUser> userManager,
        IEmailService emailService,
        IConfiguration configuration,
        ILogger<DisableAuthenticatorCommandHandler> logger)
    {
        _userManager = userManager;
        _emailService = emailService;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<bool> Handle(DisableAuthenticatorCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(request.UserId)
            ?? throw new InvalidOperationException("User not found.");

        // Disable authenticator app
        user.AuthenticatorEnabled = false;
        user.AuthenticatorSecretKey = null;

        // Also disable 2FA if authenticator was the only method
        user.TwoFactorEnabled = false;

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            throw new InvalidOperationException("Failed to disable authenticator app.");
        }

        // Send email notification
        await SendAuthenticatorDisabledEmailAsync(user, request.IpAddress, cancellationToken);

        return true;
    }

    private async Task SendAuthenticatorDisabledEmailAsync(
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

            var removalDateTime = DateTime.UtcNow;
            var removalDate = removalDateTime.ToString("MMMM dd, yyyy");
            var removalTime = removalDateTime.ToString("hh:mm tt") + " UTC";
            var ipText = string.IsNullOrWhiteSpace(ipAddress) ? "Unknown" : ipAddress;

            var securitySettingsUrl = _configuration["App:SecuritySettingsUrl"]
                ?? "https://localhost:22500/settings/security";

            var baseDir = AppContext.BaseDirectory;
            var templatePath = Path.Combine(baseDir, "EmailTemplates", "mail-authenticator-disabled.html");

            if (!File.Exists(templatePath))
            {
                _logger.LogWarning("Authenticator disabled template not found at {TemplatePath}", templatePath);
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
            html = html.Replace("{{RemovalDate}}", removalDate);
            html = html.Replace("{{RemovalTime}}", removalTime);
            html = html.Replace("{{IpAddress}}", ipText);
            html = html.Replace("{{SupportUrl}}", "mailto:support@example.com");
            html = html.Replace("{{SecuritySettingsUrl}}", securitySettingsUrl);

            var emailDomain = user.Email?.Split('@').LastOrDefault() ?? "example.com";
            html = html.Replace("support@example.com", $"support@{emailDomain}");

            await _emailService.SendAsync(
                user.Email!,
                "Authenticator App Removed from Your Account - Security Alert",
                html,
                cancellationToken);

            _logger.LogInformation(
                "Authenticator disabled notification sent to {Email} for user {UserId}",
                user.Email,
                user.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to send authenticator disabled notification to {Email} for user {UserId}",
                user.Email,
                user.Id);
        }
    }
}