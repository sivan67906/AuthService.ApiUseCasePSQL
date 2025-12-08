namespace AuthService.Application.Features.Auth.ChangePassword;

/// <summary>
/// Command to change a user's password
/// </summary>
public sealed record ChangePasswordCommand(
    string UserId,
    string CurrentPassword,
    string NewPassword,
    string? IpAddress = null
) : IRequest<bool>;