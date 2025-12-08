namespace AuthService.Application.Features.Auth.Register;

/// <summary>
/// Command to register a new user
/// </summary>
public sealed record RegisterCommand(
    string Email,
    string Password,
    string FirstName,
    string LastName,
    string PhoneNumber
) : IRequest<RegisterResultDto>;