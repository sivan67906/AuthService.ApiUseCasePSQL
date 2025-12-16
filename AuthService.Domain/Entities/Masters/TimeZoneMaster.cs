namespace AuthService.Domain.Entities.Masters;

/// <summary>
/// TimeZone Master - Reference data for time zones
/// </summary>
public sealed class TimeZoneMaster : BaseEntity
{
    public required string Name { get; set; } // e.g., "India Standard Time"
    public required string Identifier { get; set; } // IANA identifier (e.g., "Asia/Kolkata")
    public required string Offset { get; set; } // e.g., "UTC+05:30"
    public string? DisplayName { get; set; } // e.g., "(UTC+05:30) Chennai, Kolkata, Mumbai, New Delhi"
    public bool IsActive { get; set; } = true;
    public int DisplayOrder { get; set; }

    // Navigation properties
    public ICollection<Company> Companies { get; init; } = [];
}
