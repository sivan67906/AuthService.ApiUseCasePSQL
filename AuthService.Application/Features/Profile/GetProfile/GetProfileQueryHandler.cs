using Microsoft.AspNetCore.Identity;

namespace AuthService.Application.Features.Profile.GetProfile;
public class GetProfileQueryHandler : IRequestHandler<GetProfileQuery, ProfileDto>
{
    private readonly UserManager<ApplicationUser> _userManager;
    public GetProfileQueryHandler(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }
    public async Task<ProfileDto> Handle(GetProfileQuery request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(request.UserId, out var userId))
        {
            throw new ArgumentException("Invalid user id");
        }
        var user = await _userManager.FindByIdAsync(userId.ToString())
            ?? throw new InvalidOperationException("User not found");
        var twoFactorType = user.TwoFactorEnabled 
            ? (user.AuthenticatorEnabled ? "Authenticator" : "Email") 
            : "None";
        return new ProfileDto
        {
            Id = user.Id.ToString(),
            Email = user.Email ?? string.Empty,
            FirstName = user.FirstName,
            LastName = user.LastName,
            EmailConfirmed = user.EmailConfirmed,
            TwoFactorEnabled = user.TwoFactorEnabled,
            AuthenticatorEnabled = user.AuthenticatorEnabled,
            TwoFactorType = twoFactorType
        };
}
}
