using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using AuthService.Application.Common.Interfaces;
using AuthService.Application.Features.Auth.TwoFactor;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace AuthService.Application.Features.Auth.Login;

public class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResultDto>
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IConfiguration _config;
    private readonly IAppDbContext _db;
    private readonly IMediator _mediator;
    private readonly IEmailService _emailService;
    private readonly ILogger<LoginCommandHandler> _logger;

    // Token expiration settings
    private const int AccessTokenExpiryMinutes = 15;
    private const int RefreshTokenExpiryDays = 7;

    public LoginCommandHandler(
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager,
        IConfiguration config,
        IAppDbContext db,
        IMediator mediator,
        IEmailService emailService,
        ILogger<LoginCommandHandler> logger)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _config = config;
        _db = db;
        _mediator = mediator;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<LoginResultDto> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(request.Email)
            ?? throw new InvalidOperationException("Invalid credentials.");

        if (!user.EmailConfirmed)
        {
            throw new InvalidOperationException("Email not confirmed.");
        }

        var passwordResult = await _signInManager.CheckPasswordSignInAsync(user, request.Password, true);
        if (!passwordResult.Succeeded)
        {
            throw new InvalidOperationException("Invalid credentials.");
        }

        // Check if two-factor authentication is required
        if (user.TwoFactorEnabled)
        {
            // Generate a temporary token for 2FA verification
            var twoFactorToken = GenerateTwoFactorToken();

            // Store the token temporarily (could use cache or database)
            user.SecurityStamp = twoFactorToken; // Using security stamp temporarily
            await _userManager.UpdateAsync(user);

            var twoFactorType = user.AuthenticatorEnabled ? "Authenticator" : "Email";

            // If email-based 2FA, send the code
            if (!user.AuthenticatorEnabled)
            {
                var emailCode = await _userManager.GenerateTwoFactorTokenAsync(user, TokenOptions.DefaultEmailProvider);
                await SendTwoFactorCodeEmailAsync(user, emailCode, cancellationToken);
            }

            return new LoginResultDto
            {
                AccessToken = string.Empty,
                ExpiresInSeconds = 0,
                RefreshToken = null,
                RequiresTwoFactor = true,
                TwoFactorType = twoFactorType,
                TwoFactorToken = twoFactorToken
            };
        }

        // No 2FA required, proceed with normal login
        return await GenerateTokensAsync(user, cancellationToken);
    }

    private async Task<LoginResultDto> GenerateTokensAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        var roles = await _userManager.GetRolesAsync(user);

        var fullName = string.Join(" ", new[]
        {
            user.FirstName,
            user.LastName
        }.Where(s => !string.IsNullOrWhiteSpace(s)));

        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_config["Jwt:Key"]!);
        var expires = DateTime.UtcNow.AddMinutes(AccessTokenExpiryMinutes);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.GivenName, user.FirstName ?? string.Empty),
            new(ClaimTypes.Surname, user.LastName ?? string.Empty),
            new(ClaimTypes.Name, string.IsNullOrWhiteSpace(fullName)
                ? (user.Email ?? string.Empty)
                : fullName)
        };

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var token = tokenHandler.CreateToken(new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = expires,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature),
            Issuer = _config["Jwt:Issuer"],
            Audience = _config["Jwt:Audience"]
        });

        var accessToken = tokenHandler.WriteToken(token);

        // Create refresh token with fixed 7-day expiry
        var refresh = new UserRefreshToken
        {
            UserId = user.Id,
            Token = Guid.NewGuid().ToString("N"),
            ExpiresAt = DateTime.UtcNow.AddDays(RefreshTokenExpiryDays),
            IsRevoked = false
        };

        _db.Set<UserRefreshToken>().Add(refresh);
        await _db.SaveChangesAsync(cancellationToken);

        return new LoginResultDto
        {
            AccessToken = accessToken,
            ExpiresInSeconds = (int)(expires - DateTime.UtcNow).TotalSeconds,
            RefreshToken = refresh.Token,
            RequiresTwoFactor = false,
            TwoFactorType = null,
            TwoFactorToken = null
        };
    }

    private static string GenerateTwoFactorToken()
    {
        var bytes = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(bytes);
        return Convert.ToBase64String(bytes);
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
                await _emailService.SendAsync(
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
            await _emailService.SendAsync(
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