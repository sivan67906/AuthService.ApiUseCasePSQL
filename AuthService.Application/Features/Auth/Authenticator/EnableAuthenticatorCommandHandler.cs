using System.IO;
using AuthService.Application.Common.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using OtpNet;

namespace AuthService.Application.Features.Auth.Authenticator;

public sealed class EnableAuthenticatorCommandHandler : IRequestHandler<EnableAuthenticatorCommand, bool>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IEmailService _emailService;
    private readonly ILogger<EnableAuthenticatorCommandHandler> _logger;

    public EnableAuthenticatorCommandHandler(
        UserManager<ApplicationUser> userManager,
        IEmailService emailService,
        ILogger<EnableAuthenticatorCommandHandler> logger)
    {
        _userManager = userManager;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<bool> Handle(EnableAuthenticatorCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(request.UserId)
            ?? throw new InvalidOperationException("User not found.");

        if (string.IsNullOrEmpty(user.AuthenticatorSecretKey))
        {
            throw new InvalidOperationException("Authenticator has not been set up. Please run setup first.");
        }

        // Verify the code
        var secretBytes = Base32Encoding.ToBytes(user.AuthenticatorSecretKey);
        var totp = new Totp(secretBytes);

        var isValid = totp.VerifyTotp(request.VerificationCode, out _, VerificationWindow.RfcSpecifiedNetworkDelay);
        if (!isValid)
        {
            throw new InvalidOperationException("Invalid verification code. Please try again.");
        }

        // Enable authenticator app and two-factor authentication
        user.AuthenticatorEnabled = true;
        user.TwoFactorEnabled = true;

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            throw new InvalidOperationException("Failed to enable authenticator app.");
        }

        // Send email notification
        await SendAuthenticatorEnabledEmailAsync(user, request.IpAddress, cancellationToken);

        return true;
    }

    private async Task SendAuthenticatorEnabledEmailAsync(
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

            var setupDateTime = DateTime.UtcNow;
            var setupDate = setupDateTime.ToString("MMMM dd, yyyy");
            var setupTime = setupDateTime.ToString("hh:mm tt") + " UTC";
            var ipText = string.IsNullOrWhiteSpace(ipAddress) ? "Unknown" : ipAddress;

            var baseDir = AppContext.BaseDirectory;
            var templatePath = Path.Combine(baseDir, "EmailTemplates", "mail-authenticator-enabled.html");

            if (!File.Exists(templatePath))
            {
                _logger.LogWarning("Authenticator enabled template not found at {TemplatePath}", templatePath);
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
            html = html.Replace("{{SetupDate}}", setupDate);
            html = html.Replace("{{SetupTime}}", setupTime);
            html = html.Replace("{{IpAddress}}", ipText);
            html = html.Replace("{{SupportUrl}}", "mailto:support@example.com");

            var emailDomain = user.Email?.Split('@').LastOrDefault() ?? "example.com";
            html = html.Replace("support@example.com", $"support@{emailDomain}");

            await _emailService.SendAsync(
                user.Email!,
                "Authenticator App Connected to Your Account",
                html,
                cancellationToken);

            _logger.LogInformation(
                "Authenticator enabled notification sent to {Email} for user {UserId}",
                user.Email,
                user.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to send authenticator enabled notification to {Email} for user {UserId}",
                user.Email,
                user.Id);
        }
    }
}