using System.IO;
using System.Net;
using AuthService.Application.Common.Interfaces;
using AuthService.Application.Features.Auth.Register;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AuthService.Application.Features.Auth.ResetPassword;

public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, bool>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IEmailService _emailService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<RegisterCommandHandler> _logger;

    public ResetPasswordCommandHandler(
        UserManager<ApplicationUser> userManager,
        IEmailService emailService,
        IConfiguration configuration,
        ILogger<RegisterCommandHandler> logger)
    {
        _userManager = userManager;
        _emailService = emailService;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<bool> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(request.Email)
            ?? throw new InvalidOperationException("User not found.");

        var result = await _userManager.ResetPasswordAsync(user, request.Token, request.NewPassword);
        if (!result.Succeeded)
        {
            var msg = string.Join(";", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Reset password failed: {msg}");
        }

        await SendConfirmationEmailAsync(user, cancellationToken);
        return true;
    }

    private async Task SendConfirmationEmailAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        try
        {
            // Generate email confirmation token
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var encodedToken = WebUtility.UrlEncode(token);

            // Get callback URL from configuration
            var callbackBaseUrl = _configuration["Email:ConfirmationCallbackUrl"]
                ?? "https://localhost:22500/confirm-email";
            var callbackUrl = $"{callbackBaseUrl}?email={WebUtility.UrlEncode(user.Email)}&token={encodedToken}";

            // Prepare user's full name
            var fullName = $"{user.FirstName} {user.LastName}".Trim();
            if (string.IsNullOrWhiteSpace(fullName))
            {
                fullName = user.Email ?? "User";
            }

            // Load email template
            var baseDir = AppContext.BaseDirectory;
            var templatePath = Path.Combine(baseDir, "EmailTemplates", "mail-reset-password.html");

            if (!File.Exists(templatePath))
            {
                _logger.LogWarning("Email template not found at {TemplatePath}. Email not sent.", templatePath);
                return;
            }

            var html = await File.ReadAllTextAsync(templatePath, cancellationToken);

            // Replace placeholders in template
            html = html.Replace("https://pixinvent.com?verification_url", callbackUrl);
            html = html.Replace("John Doe", fullName);

            // Extract domain from user email for support email
            var emailDomain = user.Email?.Split('@').LastOrDefault() ?? "example.com";
            html = html.Replace("support@example.com", $"support@{emailDomain}");

            // Send the email
            await _emailService.SendAsync(
                user.Email!,
                "Confirm your email",
                html,
                cancellationToken);

            _logger.LogInformation(
                "Confirmation email sent successfully to {Email} for user {UserId}",
                user.Email,
                user.Id);
        }
        catch (Exception ex)
        {
            // Log the error but don't fail the registration
            // User can request a new confirmation email later
            _logger.LogError(
                ex,
                "Failed to send confirmation email to {Email} for user {UserId}. User can request a new confirmation email.",
                user.Email,
                user.Id);
        }
    }
}