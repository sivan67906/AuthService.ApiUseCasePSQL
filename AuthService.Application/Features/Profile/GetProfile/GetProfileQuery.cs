namespace AuthService.Application.Features.Profile.GetProfile;

public record GetProfileQuery(string UserId) : IRequest<ProfileDto>;
