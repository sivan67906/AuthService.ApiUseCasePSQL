using AuthService.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace AuthService.Application.Features.Menu.GetPagePermissions;

public class GetPagePermissionsQueryHandler : IRequestHandler<GetPagePermissionsQuery, PagePermissionsDto>
{
    private readonly IUserAuthorizationService _authorizationService;
    private readonly ILogger<GetPagePermissionsQueryHandler> _logger;

    public GetPagePermissionsQueryHandler(
        IUserAuthorizationService authorizationService,
        ILogger<GetPagePermissionsQueryHandler> logger)
    {
        _authorizationService = authorizationService;
        _logger = logger;
    }

    public async Task<PagePermissionsDto> Handle(GetPagePermissionsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var permissions = await _authorizationService.GetUserPagePermissionsAsync(request.UserId, request.PageName);
            
            var result = new PagePermissionsDto
            {
                PageName = request.PageName,
                Permissions = permissions,
                CanCreate = permissions.Contains("Create"),
                CanView = permissions.Contains("View"),
                CanUpdate = permissions.Contains("Update"),
                CanDelete = permissions.Contains("Delete")
            };

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving page permissions for page: {PageName}, user: {UserId}", request.PageName, request.UserId);
            throw new InvalidOperationException("An error occurred while retrieving page permissions");
        }
    }
}
