namespace AuthService.Application.Features.UserAccess.GetUserPages;

public sealed record GetUserPagesQuery(string UserId) : IRequest<List<UserPageDto>>;
public sealed class UserPageDto
{
    public Guid PageId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int DisplayOrder { get; set; }
    public List<string> RequiredPermissions { get; set; } = new();
    public List<string> Features { get; set; } = new();
}
