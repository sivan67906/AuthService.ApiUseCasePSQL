using AuthService.Application.Common.Interfaces;
using MenuItemDto = AuthService.Application.Common.Interfaces.MenuItemDto;
using Microsoft.Extensions.Logging;

namespace AuthService.Application.Features.Menu.GetUserMenus;

public class GetUserMenusQueryHandler : IRequestHandler<GetUserMenusQuery, List<MenuItemDto>>
{
    private readonly IUserAuthorizationService _authorizationService;
    private readonly ILogger<GetUserMenusQueryHandler> _logger;

    public GetUserMenusQueryHandler(
        IUserAuthorizationService authorizationService,
        ILogger<GetUserMenusQueryHandler> logger)
    {
        _authorizationService = authorizationService;
        _logger = logger;
    }

    public async Task<List<MenuItemDto>> Handle(GetUserMenusQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var menus = await _authorizationService.GetUserMenusAsync(request.UserId);
            return menus;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user menus for user {UserId}", request.UserId);
            throw new InvalidOperationException("An error occurred while retrieving menus");
        }
    }
}
