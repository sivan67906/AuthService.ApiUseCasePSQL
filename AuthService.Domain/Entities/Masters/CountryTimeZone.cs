using System;
using AuthService.Domain.Entities;

namespace AuthService.Domain.Entities.Masters;

public class CountryTimeZone : BaseEntity
{
    public Guid CountryId { get; set; }
    public Country Country { get; set; } = null!;
    
    public Guid TimeZoneId { get; set; }
    public TimeZoneMaster TimeZoneEntity { get; set; } = null!;
    
    public bool IsDefault { get; set; }
}
