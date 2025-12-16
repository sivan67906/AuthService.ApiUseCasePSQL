namespace AuthService.Domain.Entities.Masters;

/// <summary>
/// City Master - Reference data for cities
/// </summary>
public sealed class City : BaseEntity
{
    public required string Name { get; set; }
    public required Guid StateId { get; set; }
    public string? PostalCode { get; set; } // Optional postal code for smaller locations
    public bool IsActive { get; set; } = true;
    public int DisplayOrder { get; set; }

    // Navigation properties
    public State State { get; set; } = null!;
    public ICollection<Company> AddressCompanies { get; init; } = [];
}
