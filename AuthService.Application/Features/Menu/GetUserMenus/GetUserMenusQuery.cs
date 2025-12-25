using MenuItemDto = AuthService.Application.Common.Interfaces.MenuItemDto;

namespace AuthService.Application.Features.Menu.GetUserMenus;

public record GetUserMenusQuery(Guid UserId) : IRequest<List<MenuItemDto>>;
