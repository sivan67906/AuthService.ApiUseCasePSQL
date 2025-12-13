using System.IO;
using System.Net;
using AuthService.Application.Common.Interfaces;
using AuthService.Domain.Constants;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AuthService.Application.Features.Auth.Register;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, RegisterResultDto>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IAppDbContext _db;
    private readonly IEmailService _emailService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<RegisterCommandHandler> _logger;
    private readonly IEmailConfirmationTokenTracker _tokenTracker;

    public RegisterCommandHandler(
        UserManager<ApplicationUser> userManager,
        IAppDbContext db,
        IEmailService emailService,
        IConfiguration configuration,
        ILogger<RegisterCommandHandler> logger,
        IEmailConfirmationTokenTracker tokenTracker)
    {
        _userManager = userManager;
        _db = db;
        _emailService = emailService;
        _configuration = configuration;
        _logger = logger;
        _tokenTracker = tokenTracker;
    }

    public async Task<RegisterResultDto> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            UserName = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            PhoneNumber = request.PhoneNumber,
            EmailConfirmed = false
        };

        // Create user via UserManager (handles all database operations)
        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            var errors = string.Join(";", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Unable to register user: {errors}");
        }

        _logger.LogInformation("User {UserId} ({Email}) registered successfully", user.Id, user.Email);

        // Assign PendingUser role by default
        // await _userManager.AddToRoleAsync(user, SystemRoles.PendingUser);

        // Send confirmation email automatically
        await SendConfirmationEmailAsync(user, cancellationToken);

        return new RegisterResultDto
        {
            UserId = user.Id.ToString(),
            Email = user.Email!
        };
    }

    private async Task SendConfirmationEmailAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        try
        {
            // Generate email confirmation token with tracking
            var standardToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var tokenTimestamp = DateTime.UtcNow;
            var expiryTime = tokenTimestamp.AddHours(1);
            
            // Track this as the latest token for this user
            _tokenTracker.StoreLatestToken(user.Email!, tokenTimestamp);
            
            // Create custom token with timestamp: userId|tokenTimestamp|expiryTimestamp|standardToken
            var customToken = $"{user.Id}|{tokenTimestamp:O}|{expiryTime:O}|{standardToken}";
            var encodedToken = WebUtility.UrlEncode(customToken);

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
            var templatePath = Path.Combine(baseDir, "EmailTemplates", "confirmmail-verify-email.html");

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