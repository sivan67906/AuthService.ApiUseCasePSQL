using AuthService.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace AuthService.Application.Features.Menu.GetUserRoles;

public class GetUserRolesQueryHandler : IRequestHandler<GetUserRolesQuery, List<string>>
{
    private readonly IUserAuthorizationService _authorizationService;
    private readonly ILogger<GetUserRolesQueryHandler> _logger;

    public GetUserRolesQueryHandler(
        IUserAuthorizationService authorizationService,
        ILogger<GetUserRolesQueryHandler> logger)
    {
        _authorizationService = authorizationService;
        _logger = logger;
    }

    public async Task<List<string>> Handle(GetUserRolesQuery request, CancellationToken cancellationToken)
    {
        try
        {
            return await _authorizationService.GetUserRolesAsync(request.UserId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user roles for user: {UserId}", request.UserId);
            throw new InvalidOperationException("An error occurred while retrieving user roles");
        }
    }
}
