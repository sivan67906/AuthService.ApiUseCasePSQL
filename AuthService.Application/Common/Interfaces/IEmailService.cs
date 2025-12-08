namespace AuthService.Application.Common.Interfaces;

public interface IEmailService
{
    Task SendAsync(string to, string subject, string body, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Send a two-factor authentication code to the user's email
    /// </summary>
    Task SendTwoFactorCodeAsync(string to, string code, CancellationToken cancellationToken = default);
}
