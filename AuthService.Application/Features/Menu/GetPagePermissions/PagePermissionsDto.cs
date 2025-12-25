namespace AuthService.Application.Features.Menu.GetPagePermissions;

public class PagePermissionsDto
{
    public string PageName { get; set; } = string.Empty;
    public List<string> Permissions { get; set; } = [];
    public bool CanCreate { get; set; }
    public bool CanView { get; set; }
    public bool CanUpdate { get; set; }
    public bool CanDelete { get; set; }
}
