using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using AuthService.Application.Common.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using OtpNet;

namespace AuthService.Application.Features.Auth.Login;

public sealed class VerifyTwoFactorLoginCommandHandler : IRequestHandler<VerifyTwoFactorLoginCommand, LoginResultDto>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IConfiguration _config;
    private readonly IAppDbContext _db;

    public VerifyTwoFactorLoginCommandHandler(
        UserManager<ApplicationUser> userManager,
        IConfiguration config,
        IAppDbContext db)
    {
        _userManager = userManager;
        _config = config;
        _db = db;
    }

    public async Task<LoginResultDto> Handle(VerifyTwoFactorLoginCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(request.Email)
            ?? throw new InvalidOperationException("User not found.");

        // Verify the two-factor token matches
        if (user.SecurityStamp != request.TwoFactorToken)
        {
            throw new InvalidOperationException("Invalid or expired two-factor session.");
        }

        bool isValid;
        if (request.TwoFactorType.Equals("Authenticator", StringComparison.OrdinalIgnoreCase))
        {
            // Verify authenticator app code (TOTP)
            if (string.IsNullOrEmpty(user.AuthenticatorSecretKey))
            {
                throw new InvalidOperationException("Authenticator not configured.");
            }

            var secretBytes = Base32Encoding.ToBytes(user.AuthenticatorSecretKey);
            var totp = new Totp(secretBytes);
            isValid = totp.VerifyTotp(request.Code, out _, VerificationWindow.RfcSpecifiedNetworkDelay);
        }
        else
        {
            // Verify email-based 2FA code
            isValid = await _userManager.VerifyTwoFactorTokenAsync(
                user,
                TokenOptions.DefaultEmailProvider,
                request.Code);
        }

        if (!isValid)
        {
            throw new InvalidOperationException("Invalid verification code.");
        }

        // Reset the security stamp after successful verification
        await _userManager.UpdateSecurityStampAsync(user);

        // Generate tokens and complete login
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
        var expires = DateTime.UtcNow.AddMinutes(15);

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

        var refresh = new UserRefreshToken
        {
            UserId = user.Id,
            Token = Guid.NewGuid().ToString("N"),
            ExpiresAt = DateTime.UtcNow.AddDays(7),
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
}

