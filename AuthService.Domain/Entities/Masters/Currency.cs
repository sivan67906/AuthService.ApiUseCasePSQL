namespace AuthService.Domain.Entities.Masters;

/// <summary>
/// Currency Master - Reference data for currencies
/// </summary>
public sealed class Currency : BaseEntity
{
    public required string Code { get; set; } // ISO 4217 (e.g., "INR", "USD", "EUR")
    public required string Name { get; set; } // e.g., "Indian Rupee", "US Dollar"
    public required string Symbol { get; set; } // e.g., "₹", "$", "€"
    public int DecimalPlaces { get; set; } = 2;
    public bool IsActive { get; set; } = true;
    public int DisplayOrder { get; set; }

    // Navigation properties
    public ICollection<Company> BaseCurrencyCompanies { get; init; } = [];
    public ICollection<Company> ReportingCurrencyCompanies { get; init; } = [];
}
