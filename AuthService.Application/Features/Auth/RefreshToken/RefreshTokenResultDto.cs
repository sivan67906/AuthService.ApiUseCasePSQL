namespace AuthService.Application.Features.Auth.RefreshToken;

public class RefreshTokenResultDto
{
    public string AccessToken { get; set; } = string.Empty;
    public int ExpiresInSeconds { get; set; }
    public string NewRefreshToken { get; set; } = string.Empty;
}
