using AuthService.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace AuthService.Application.Features.Menu.CheckPageAccess;

public class CheckPageAccessQueryHandler : IRequestHandler<CheckPageAccessQuery, bool>
{
    private readonly IUserAuthorizationService _authorizationService;
    private readonly ILogger<CheckPageAccessQueryHandler> _logger;

    public CheckPageAccessQueryHandler(
        IUserAuthorizationService authorizationService,
        ILogger<CheckPageAccessQueryHandler> logger)
    {
        _authorizationService = authorizationService;
        _logger = logger;
    }

    public async Task<bool> Handle(CheckPageAccessQuery request, CancellationToken cancellationToken)
    {
        try
        {
            return await _authorizationService.UserHasAccessToPageAsync(request.UserId, request.PageName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking page access for page: {PageName}, user: {UserId}", request.PageName, request.UserId);
            throw new InvalidOperationException("An error occurred while checking page access");
        }
    }
}
