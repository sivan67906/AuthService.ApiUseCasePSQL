namespace AuthService.Domain.Entities.Masters;

/// <summary>
/// State/Province Master - Reference data for states/provinces
/// </summary>
public sealed class State : BaseEntity
{
    public required string Name { get; set; }
    public required string Code { get; set; } // State code (e.g., "KA", "MH", "CA", "NY")
    public required Guid CountryId { get; set; }
    public bool IsActive { get; set; } = true;
    public int DisplayOrder { get; set; }

    // Navigation properties
    public Country Country { get; set; } = null!;
    public ICollection<City> Cities { get; init; } = [];
    public ICollection<Company> RegisteredCompanies { get; init; } = [];
    public ICollection<Company> AddressCompanies { get; init; } = [];
}
