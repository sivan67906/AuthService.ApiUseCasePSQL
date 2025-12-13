using System.IO;
using AuthService.Application.Common.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace AuthService.Application.Features.Auth.ChangePassword;

public class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommand, bool>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IEmailService _emailService;
    private readonly ILogger<ChangePasswordCommandHandler> _logger;

    public ChangePasswordCommandHandler(
        UserManager<ApplicationUser> userManager,
        IEmailService emailService,
        ILogger<ChangePasswordCommandHandler> logger)
    {
        _userManager = userManager;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<bool> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(request.UserId)
            ?? throw new InvalidOperationException("User not found.");

        // Issue #9: Validate that new password is different from current password
        if (request.NewPassword == request.CurrentPassword)
        {
            throw new InvalidOperationException("New password must be different from current password.");
        }

        var result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
        if (!result.Succeeded)
        {
            var msg = string.Join(";", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Change password failed: {msg}");
        }

        // Send password change confirmation email
        await SendPasswordChangedEmailAsync(user, request.IpAddress, cancellationToken);
        return true;
    }

    private async Task SendPasswordChangedEmailAsync(
        ApplicationUser user,
        string? ipAddress,
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
            var changeDateTime = DateTime.UtcNow;
            var changeDate = changeDateTime.ToString("MMMM dd, yyyy");
            var changeTime = changeDateTime.ToString("hh:mm tt") + " UTC";

            // Handle IP address
            var ipText = string.IsNullOrWhiteSpace(ipAddress) ? "Unknown" : ipAddress;

            // Load email template
            var baseDir = AppContext.BaseDirectory;
            var templatePath = Path.Combine(baseDir, "EmailTemplates", "mail-password-changed.html");

            if (!File.Exists(templatePath))
            {
                _logger.LogWarning("Password changed email template not found at {TemplatePath}. Email not sent.", templatePath);
                return;
            }

            var html = await File.ReadAllTextAsync(templatePath, cancellationToken);

            // Replace placeholders in template
            html = html.Replace("John Doe", fullName);
            html = html.Replace("{{ChangeDate}}", changeDate);
            html = html.Replace("{{ChangeTime}}", changeTime);
            html = html.Replace("{{IpAddress}}", ipText);
            html = html.Replace("{{UserEmail}}", user.Email ?? "");
            html = html.Replace("{{SupportUrl}}", "mailto:support@example.com");

            // Extract domain from user email for support email
            var emailDomain = user.Email?.Split('@').LastOrDefault() ?? "example.com";
            html = html.Replace("support@example.com", $"support@{emailDomain}");

            // Send the email
            await _emailService.SendAsync(
                user.Email!,
                "Password Changed Successfully",
                html,
                cancellationToken);

            _logger.LogInformation(
                "Password changed confirmation email sent successfully to {Email} for user {UserId}",
                user.Email,
                user.Id);
        }
        catch (Exception ex)
        {
            // Log the error but don't fail the password change operation
            _logger.LogError(
                ex,
                "Failed to send password changed confirmation email to {Email} for user {UserId}",
                user.Email,
                user.Id);
        }
    }
}
