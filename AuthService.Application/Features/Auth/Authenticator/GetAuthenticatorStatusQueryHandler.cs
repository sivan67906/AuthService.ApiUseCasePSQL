using Microsoft.AspNetCore.Identity;

namespace AuthService.Application.Features.Auth.Authenticator;
public sealed class GetAuthenticatorStatusQueryHandler : IRequestHandler<GetAuthenticatorStatusQuery, AuthenticatorStatusDto>
{
    private readonly UserManager<ApplicationUser> _userManager;
    public GetAuthenticatorStatusQueryHandler(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }
    public async Task<AuthenticatorStatusDto> Handle(GetAuthenticatorStatusQuery request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(request.UserId)
            ?? throw new InvalidOperationException("User not found.");
        var twoFactorType = user.TwoFactorEnabled 
            ? (user.AuthenticatorEnabled ? "Authenticator" : "Email") 
            : "None";
        return new AuthenticatorStatusDto
        {
            IsEnabled = user.AuthenticatorEnabled,
            TwoFactorEnabled = user.TwoFactorEnabled,
            TwoFactorType = twoFactorType
        };
}
}
