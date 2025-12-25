namespace AuthService.Application.Features.Admin.GetAdminStats;

public class GetAdminStatsQueryHandler : IRequestHandler<GetAdminStatsQuery, AdminStatsDto>
{
    public Task<AdminStatsDto> Handle(GetAdminStatsQuery request, CancellationToken cancellationToken)
    {
        var stats = new AdminStatsDto
        {
            UsersOnline = 0,
            GeneratedAtUtc = DateTime.UtcNow
        };

        return Task.FromResult(stats);
    }
}
