using Microsoft.AspNetCore.Identity;

namespace AuthService.Application.Features.Auth.EmailConfirmation;
public class ConfirmEmailCommandHandler : IRequestHandler<ConfirmEmailCommand, bool>
{
    private readonly UserManager<ApplicationUser> _userManager;
    public ConfirmEmailCommandHandler(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }
    public async Task<bool> Handle(ConfirmEmailCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(request.Email)
            ?? throw new InvalidOperationException("User not found.");
        var result = await _userManager.ConfirmEmailAsync(user, request.Token);
        if (!result.Succeeded)
        {
            var msg = string.Join(";", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Confirm email failed: {msg}");
        }
        return true;
}
}
