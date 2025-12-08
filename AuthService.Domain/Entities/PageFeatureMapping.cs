namespace AuthService.Domain.Entities;

public sealed class PageFeatureMapping : BaseEntity
{
    public Guid PageId { get; set; }
    public Guid FeatureId { get; set; }
    public bool IsActive { get; set; } = true;
    // Navigation properties
    public Page Page { get; set; } = null!;
    public Feature Feature { get; set; } = null!;
}
