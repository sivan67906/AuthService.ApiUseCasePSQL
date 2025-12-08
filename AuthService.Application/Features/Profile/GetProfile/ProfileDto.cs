namespace AuthService.Application.Features.Profile.GetProfile;

public class ProfileDto
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public bool EmailConfirmed { get; set; }
    public bool TwoFactorEnabled { get; set; }
    
    /// <summary>
    /// Indicates if authenticator app is enabled for 2FA
    /// </summary>
    public bool AuthenticatorEnabled { get; set; }
    /// The type of 2FA configured: "None", "Email", or "Authenticator"
    public string TwoFactorType { get; set; } = "None";
}
