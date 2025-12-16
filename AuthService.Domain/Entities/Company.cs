using AuthService.Domain.Entities.Masters;

namespace AuthService.Domain.Entities;

/// <summary>
/// Company entity status enumeration
/// </summary>
public enum CompanyStatus : short
{
    Draft = 1,
    Active = 2,
    Inactive = 3
}

/// <summary>
/// Legal structure enumeration
/// </summary>
public enum LegalStructure : short
{
    PrivateLimited = 1,
    PublicLimited = 2,
    LLP = 3,
    Partnership = 4,
    SoleProprietor = 5,
    Proprietorship = 6,
    Trust = 7,
    HUF = 8,
    Society = 9,
    Government = 10,
    Other = 11
}

/// <summary>
/// Rounding mode enumeration
/// </summary>
public enum RoundingMode : short
{
    HalfUp = 1,
    HalfDown = 2,
    Ceiling = 3,
    Floor = 4,
    HalfEven = 5,
    Bankers = 6
}

/// <summary>
/// Company Master - Legal, financial, compliance, and system properties of an organization
/// Following professional ERP standards with single-table design for atomicity
/// </summary>
public sealed class Company : BaseEntity
{
    // ========================
    // A. Company Identity
    // ========================
    
    /// <summary>
    /// Unique company code - uppercase, immutable after postings
    /// </summary>
    public required string CompanyCode { get; set; }
    
    /// <summary>
    /// Legal name of the company - required, cannot be blank
    /// </summary>
    public required string LegalName { get; set; }
    
    /// <summary>
    /// Trade name - if empty, UI shows LegalName
    /// </summary>
    public string? TradeName { get; set; }
    
    /// <summary>
    /// Short name for display purposes
    /// </summary>
    public string? ShortName { get; set; }
    
    /// <summary>
    /// Legal structure of the company
    /// </summary>
    public LegalStructure LegalStructure { get; set; }
    
    /// <summary>
    /// Date of incorporation - must be ≤ Today and ≥ 1900
    /// </summary>
    public DateTime? IncorporationDate { get; set; }
    
    /// <summary>
    /// Parent company reference (self-referencing)
    /// </summary>
    public Guid? ParentCompanyId { get; set; }
    
    /// <summary>
    /// Company status - Draft, Active, or Inactive
    /// </summary>
    public CompanyStatus Status { get; set; } = CompanyStatus.Draft;

    // ========================
    // B. Registration & Compliance
    // ========================
    
    /// <summary>
    /// Company registration number (per country rules)
    /// </summary>
    public string? RegistrationNumber { get; set; }
    
    /// <summary>
    /// PAN Number - Indian format [A-Z]{5}[0-9]{4}[A-Z]
    /// </summary>
    public string? PANNumber { get; set; }
    
    /// <summary>
    /// GSTIN - 15-char GST format
    /// </summary>
    public string? GSTIN { get; set; }
    
    /// <summary>
    /// TAN Number - Basic alphanumeric
    /// </summary>
    public string? TANNumber { get; set; }
    
    /// <summary>
    /// Other tax identification number
    /// </summary>
    public string? OtherTaxId { get; set; }
    
    /// <summary>
    /// Country of registration - required
    /// </summary>
    public Guid RegistrationCountryId { get; set; }
    
    /// <summary>
    /// State of registration - conditional based on country
    /// </summary>
    public Guid? RegistrationStateId { get; set; }

    // ========================
    // C. Registered Address
    // ========================
    
    /// <summary>
    /// Address Line 1 - required
    /// </summary>
    public required string AddressLine1 { get; set; }
    
    /// <summary>
    /// Address Line 2 - optional
    /// </summary>
    public string? AddressLine2 { get; set; }
    
    /// <summary>
    /// City - required
    /// </summary>
    public Guid CityId { get; set; }
    
    /// <summary>
    /// State/Province - required
    /// </summary>
    public Guid StateId { get; set; }
    
    /// <summary>
    /// Postal code - required, country-specific format
    /// </summary>
    public required string PostalCode { get; set; }
    
    /// <summary>
    /// Country - required
    /// </summary>
    public Guid CountryId { get; set; }
    
    /// <summary>
    /// Time zone - required
    /// </summary>
    public Guid TimeZoneId { get; set; }

    // ========================
    // D. Contact & Branding
    // ========================
    
    /// <summary>
    /// Primary contact person name
    /// </summary>
    public string? PrimaryContactName { get; set; }
    
    /// <summary>
    /// Primary email address - email format validation
    /// </summary>
    public string? PrimaryEmail { get; set; }
    
    /// <summary>
    /// Primary phone number - digits, +, -, spaces
    /// </summary>
    public string? PrimaryPhone { get; set; }
    
    /// <summary>
    /// Company website URL
    /// </summary>
    public string? WebsiteUrl { get; set; }
    
    /// <summary>
    /// Path to company logo file
    /// </summary>
    public string? LogoFileUrl { get; set; }

    // ========================
    // E. Financial Settings
    // ========================
    
    /// <summary>
    /// Base currency - required, immutable after postings
    /// </summary>
    public Guid BaseCurrencyId { get; set; }
    
    /// <summary>
    /// Reporting currency - if null, uses BaseCurrency
    /// </summary>
    public Guid? ReportingCurrencyId { get; set; }
    
    /// <summary>
    /// Fiscal year start month (1-12, default India = 4)
    /// </summary>
    public byte FiscalYearStartMonth { get; set; } = 4;
    
    /// <summary>
    /// Books start date - must be ≥ IncorporationDate
    /// </summary>
    public DateTime BooksStartDate { get; set; }
    
    /// <summary>
    /// Enable multi-currency transactions
    /// </summary>
    public bool EnableMultiCurrency { get; set; }
    
    /// <summary>
    /// Rounding precision (0-4, default = 2)
    /// </summary>
    public byte RoundingPrecision { get; set; } = 2;
    
    /// <summary>
    /// Rounding mode for calculations
    /// </summary>
    public RoundingMode? RoundingMode { get; set; }

    // ========================
    // F. System & Posting Controls
    // ========================
    
    /// <summary>
    /// Allow postings from this date
    /// </summary>
    public DateTime? AllowPostingFromDate { get; set; }
    
    /// <summary>
    /// Soft limit for postings - no postings after this date
    /// </summary>
    public DateTime? AllowPostingToDate { get; set; }
    
    /// <summary>
    /// Restrict postings before current month
    /// </summary>
    public bool LockBackDatedPosting { get; set; }
    
    /// <summary>
    /// Audit comments and notes
    /// </summary>
    public string? Notes { get; set; }

    // ========================
    // Navigation Properties
    // ========================
    
    public Company? ParentCompany { get; set; }
    public ICollection<Company> ChildCompanies { get; init; } = [];
    
    public Country RegistrationCountry { get; set; } = null!;
    public State? RegistrationState { get; set; }
    public Country AddressCountry { get; set; } = null!;
    public State AddressState { get; set; } = null!;
    public City City { get; set; } = null!;
    public TimeZoneMaster TimeZone { get; set; } = null!;
    public Currency BaseCurrency { get; set; } = null!;
    public Currency? ReportingCurrency { get; set; }
}
