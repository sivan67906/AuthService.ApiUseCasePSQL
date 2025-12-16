namespace AuthService.Domain.Entities.Masters;

/// <summary>
/// Country Master - Reference data for countries
/// </summary>
public sealed class Country : BaseEntity
{
    public required string Name { get; set; }
    public required string Code { get; set; } // ISO 3166-1 alpha-2 (e.g., "IN", "US")
    public string? Code3 { get; set; } // ISO 3166-1 alpha-3 (e.g., "IND", "USA")
    public string? NumericCode { get; set; } // ISO 3166-1 numeric
    public string? PhoneCode { get; set; } // e.g., "+91", "+1"
    public string? CurrencyCode { get; set; } // e.g., "INR", "USD"
    public bool IsActive { get; set; } = true;
    public int DisplayOrder { get; set; }

    // Navigation properties
    public ICollection<State> States { get; init; } = [];
    public ICollection<Company> RegisteredCompanies { get; init; } = [];
    public ICollection<Company> AddressCompanies { get; init; } = [];
}
