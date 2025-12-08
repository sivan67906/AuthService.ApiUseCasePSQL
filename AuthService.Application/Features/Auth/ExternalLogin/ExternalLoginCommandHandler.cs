using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using AuthService.Application.Common.Interfaces;
using AuthService.Application.Features.Auth.Login;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace AuthService.Application.Features.Auth.ExternalLogin;

public class ExternalLoginCommandHandler : IRequestHandler<ExternalLoginCommand, LoginResultDto>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IConfiguration _config;
    private readonly IAppDbContext _db;

    public ExternalLoginCommandHandler(
        UserManager<ApplicationUser> userManager,
        IConfiguration config,
        IAppDbContext db)
    {
        _userManager = userManager;
        _config = config;
        _db = db;
    }

    public async Task<LoginResultDto> Handle(ExternalLoginCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);

        if (user is null)
        {
            user = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                Email = request.Email,
                UserName = request.Email,
                EmailConfirmed = true
            };

            var createResult = await _userManager.CreateAsync(user);
            if (!createResult.Succeeded)
            {
                var msg = string.Join(";", createResult.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"External sign-up failed: {msg}");
            }

            // default role
            await _userManager.AddToRoleAsync(user, Domain.Constants.Roles.User);
        }

        var roles = await _userManager.GetRolesAsync(user);
        var handler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_config["Jwt:Key"]!);
        var expires = DateTime.UtcNow.AddMinutes(15);

        var fullName = string.Join(" ", new[]
        {
            user.FirstName,
            user.LastName
        }.Where(s => !string.IsNullOrWhiteSpace(s)));

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

        var token = handler.CreateToken(new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = expires,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature),
            Issuer = _config["Jwt:Issuer"],
            Audience = _config["Jwt:Audience"]
        });

        var accessToken = handler.WriteToken(token);

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
