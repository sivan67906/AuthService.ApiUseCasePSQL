namespace AuthService.Domain.Entities;

public sealed class PagePermissionMapping : BaseEntity
{
    public Guid PageId { get; set; }
    public Guid PermissionId { get; set; }
    public bool IsActive { get; set; } = true;
    // Navigation properties
    public Page Page { get; set; } = null!;
    public Permission Permission { get; set; } = null!;
}
