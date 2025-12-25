namespace AuthService.Application.Common.Interfaces;

public interface IUserAuthorizationService
{
    Task<bool> UserHasPermissionAsync(Guid userId, string permissionName);
    Task<bool> UserHasAccessToPageAsync(Guid userId, string pageName);
    Task<bool> UserHasAccessToDepartmentAsync(Guid userId, Guid? departmentId);
    Task<List<MenuItemDto>> GetUserMenusAsync(Guid userId);
    Task<List<string>> GetUserRolesAsync(Guid userId);
    Task<Guid?> GetUserDepartmentAsync(Guid userId);
    Task<List<string>> GetUserPagePermissionsAsync(Guid userId, string pageName);
}

// DTOs used by the service interface
public class MenuItemDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Icon { get; set; }
    public int DisplayOrder { get; set; }
    public int Level { get; set; }
    public List<MenuItemDto> SubMenus { get; set; } = [];
    public List<MenuPageItemDto> Pages { get; set; } = [];
}

public class MenuPageItemDto
{
    public Guid Id { get; set; }
    public Guid PageId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int DisplayOrder { get; set; }
    public string? ApiEndpoint { get; set; }
    public string? HttpMethod { get; set; }
}
