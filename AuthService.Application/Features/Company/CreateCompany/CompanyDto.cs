using AuthService.Domain.Entities;

namespace AuthService.Application.Features.Company.CreateCompany;

/// <summary>
/// Data transfer object for Company entity
/// </summary>
public sealed class CompanyDto
{
    // Identity
    public Guid Id { get; set; }
    public string CompanyCode { get; set; } = string.Empty;
    public string LegalName { get; set; } = string.Empty;
    public string? TradeName { get; set; }
    public string? ShortName { get; set; }
    public LegalStructure LegalStructure { get; set; }
    public string LegalStructureName => LegalStructure.ToString();
    public DateTime? IncorporationDate { get; set; }
    public Guid? ParentCompanyId { get; set; }
    public string? ParentCompanyName { get; set; }
    public CompanyStatus Status { get; set; }
    public string StatusName => Status.ToString();

    // Registration & Compliance
    public string? RegistrationNumber { get; set; }
    public string? PANNumber { get; set; }
    public string? GSTIN { get; set; }
    public string? TANNumber { get; set; }
    public string? OtherTaxId { get; set; }
    public Guid RegistrationCountryId { get; set; }
    public string? RegistrationCountryName { get; set; }
    public Guid? RegistrationStateId { get; set; }
    public string? RegistrationStateName { get; set; }

    // Address
    public string AddressLine1 { get; set; } = string.Empty;
    public string? AddressLine2 { get; set; }
    public Guid CityId { get; set; }
    public string? CityName { get; set; }
    public Guid StateId { get; set; }
    public string? StateName { get; set; }
    public string PostalCode { get; set; } = string.Empty;
    public Guid CountryId { get; set; }
    public string? CountryName { get; set; }
    public Guid TimeZoneId { get; set; }
    public string? TimeZoneName { get; set; }

    // Contact & Branding
    public string? PrimaryContactName { get; set; }
    public string? PrimaryEmail { get; set; }
    public string? PrimaryPhone { get; set; }
    public string? WebsiteUrl { get; set; }
    public string? LogoFileUrl { get; set; }

    // Financial Settings
    public Guid BaseCurrencyId { get; set; }
    public string? BaseCurrencyName { get; set; }
    public string? BaseCurrencyCode { get; set; }
    public Guid? ReportingCurrencyId { get; set; }
    public string? ReportingCurrencyName { get; set; }
    public string? ReportingCurrencyCode { get; set; }
    public byte FiscalYearStartMonth { get; set; }
    public DateTime? BooksStartDate { get; set; }
    public bool EnableMultiCurrency { get; set; }
    public byte RoundingPrecision { get; set; }
    public RoundingMode? RoundingMode { get; set; }
    public string? RoundingModeName => RoundingMode?.ToString();

    // System & Posting Controls
    public DateTime? AllowPostingFromDate { get; set; }
    public DateTime? AllowPostingToDate { get; set; }
    public bool LockBackDatedPosting { get; set; }
    public string? Notes { get; set; }

    // Audit
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? ModifiedBy { get; set; }
    public bool IsDeleted { get; set; }
}
