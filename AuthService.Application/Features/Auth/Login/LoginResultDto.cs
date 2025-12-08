namespace AuthService.Application.Features.Auth.Login;

public class LoginResultDto
{
    public string AccessToken { get; set; } = string.Empty;
    public int ExpiresInSeconds { get; set; }
    public string? RefreshToken { get; set; }
    
    /// <summary>
    /// Indicates if the user requires two-factor authentication to complete login
    /// </summary>
    public bool RequiresTwoFactor { get; set; }
    /// The type of two-factor authentication required: "Email" or "Authenticator"
    public string? TwoFactorType { get; set; }
    /// Temporary token used for two-factor verification (when RequiresTwoFactor is true)
    public string? TwoFactorToken { get; set; }
}
