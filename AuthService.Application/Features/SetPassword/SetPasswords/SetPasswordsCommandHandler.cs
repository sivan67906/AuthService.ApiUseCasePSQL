using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace AuthService.Application.Features.SetPassword.SetPasswords;

public class SetPasswordsCommandHandler : IRequestHandler<SetPasswordsCommand, string>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<SetPasswordsCommandHandler> _logger;

    public SetPasswordsCommandHandler(
        UserManager<ApplicationUser> userManager,
        ILogger<SetPasswordsCommandHandler> logger)
    {
        _userManager = userManager;
        _logger = logger;
    }

    public async Task<string> Handle(SetPasswordsCommand request, CancellationToken cancellationToken)
    {
        if (request.Emails == null || !request.Emails.Any())
        {
            throw new InvalidOperationException("No emails provided.");
        }

        foreach (var email in request.Emails)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user != null)
            {
                var removeResult = await _userManager.RemovePasswordAsync(user);
                if (!removeResult.Succeeded)
                {
                    _logger.LogError("Failed to remove password for {Email}", email);
                    throw new InvalidOperationException($"Failed to remove password for {email}");
                }

                var addResult = await _userManager.AddPasswordAsync(user, "Welcome@123");
                if (!addResult.Succeeded)
                {
                    _logger.LogError("Failed to add password for {Email}", email);
                    throw new InvalidOperationException($"Failed to add password for {email}");
                }
                
                _logger.LogInformation("Password updated successfully for {Email}", email);
            }
        }

        return "Passwords updated successfully";
    }
}
