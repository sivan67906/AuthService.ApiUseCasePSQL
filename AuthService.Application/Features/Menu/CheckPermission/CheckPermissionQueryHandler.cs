using AuthService.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace AuthService.Application.Features.Menu.CheckPermission;

public class CheckPermissionQueryHandler : IRequestHandler<CheckPermissionQuery, bool>
{
    private readonly IUserAuthorizationService _authorizationService;
    private readonly ILogger<CheckPermissionQueryHandler> _logger;

    public CheckPermissionQueryHandler(
        IUserAuthorizationService authorizationService,
        ILogger<CheckPermissionQueryHandler> logger)
    {
        _authorizationService = authorizationService;
        _logger = logger;
    }

    public async Task<bool> Handle(CheckPermissionQuery request, CancellationToken cancellationToken)
    {
        try
        {
            return await _authorizationService.UserHasPermissionAsync(request.UserId, request.PermissionName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking permission: {PermissionName}, user: {UserId}", request.PermissionName, request.UserId);
            throw new InvalidOperationException("An error occurred while checking permission");
        }
    }
}
