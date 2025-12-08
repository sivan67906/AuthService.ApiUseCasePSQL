using System.IO;
using AuthService.Application.Common.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AuthService.Application.Features.Auth.TwoFactor;

public class DisableTwoFactorCommandHandler : IRequestHandler<DisableTwoFactorCommand, bool>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IEmailService _emailService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<DisableTwoFactorCommandHandler> _logger;

    public DisableTwoFactorCommandHandler(
        UserManager<ApplicationUser> userManager,
        IEmailService emailService,
        IConfiguration configuration,
        ILogger<DisableTwoFactorCommandHandler> logger)
    {
        _userManager = userManager;
        _emailService = emailService;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<bool> Handle(DisableTwoFactorCommand request, CancellationToken cancellationToken)
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

        if (user.TwoFactorEnabled)
        {
            user.TwoFactorEnabled = false;
            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                throw new InvalidOperationException("Failed to disable two-factor authentication.");
            }

            // Send email notification
            await SendTwoFactorDisabledEmailAsync(user, request.IpAddress, cancellationToken);
        }

        return true;
    }

    private async Task SendTwoFactorDisabledEmailAsync(
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

            var deactivationDateTime = DateTime.UtcNow;
            var deactivationDate = deactivationDateTime.ToString("MMMM dd, yyyy");
            var deactivationTime = deactivationDateTime.ToString("hh:mm tt") + " UTC";
            var ipText = string.IsNullOrWhiteSpace(ipAddress) ? "Unknown" : ipAddress;

            var securitySettingsUrl = _configuration["App:SecuritySettingsUrl"]
                ?? "https://localhost:22500/settings/security";

            var baseDir = AppContext.BaseDirectory;
            var templatePath = Path.Combine(baseDir, "EmailTemplates", "mail-twofactor-disabled.html");

            if (!File.Exists(templatePath))
            {
                _logger.LogWarning("Two-factor disabled template not found at {TemplatePath}", templatePath);
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
            html = html.Replace("{{DeactivationDate}}", deactivationDate);
            html = html.Replace("{{DeactivationTime}}", deactivationTime);
            html = html.Replace("{{IpAddress}}", ipText);
            html = html.Replace("{{SupportUrl}}", "mailto:support@example.com");
            html = html.Replace("{{SecuritySettingsUrl}}", securitySettingsUrl);

            var emailDomain = user.Email?.Split('@').LastOrDefault() ?? "example.com";
            html = html.Replace("support@example.com", $"support@{emailDomain}");

            await _emailService.SendAsync(
                user.Email!,
                "Two-Factor Authentication Disabled - Security Alert",
                html,
                cancellationToken);

            _logger.LogInformation(
                "Two-factor disabled notification sent to {Email} for user {UserId}",
                user.Email,
                user.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to send two-factor disabled notification to {Email} for user {UserId}",
                user.Email,
                user.Id);
        }
    }
}