using AuthService.Domain.Entities;

namespace AuthService.Application.Features.Company.CreateCompany;

/// <summary>
/// Command to create a new company
/// </summary>
public sealed record CreateCompanyCommand(
    // Identity
    string CompanyCode,
    string LegalName,
    string? TradeName,
    string? ShortName,
    LegalStructure LegalStructure,
    DateTime? IncorporationDate,
    Guid? ParentCompanyId,

    // Registration & Compliance
    string? RegistrationNumber,
    string? PANNumber,
    string? GSTIN,
    string? TANNumber,
    string? OtherTaxId,
    Guid RegistrationCountryId,
    Guid? RegistrationStateId,

    // Address
    string AddressLine1,
    string? AddressLine2,
    Guid CityId,
    Guid StateId,
    string PostalCode,
    Guid CountryId,
    Guid TimeZoneId,

    // Contact & Branding
    string? PrimaryContactName,
    string? PrimaryEmail,
    string? PrimaryPhone,
    string? WebsiteUrl,
    string? LogoFileUrl,

    // Financial Settings
    Guid BaseCurrencyId,
    Guid? ReportingCurrencyId,
    byte FiscalYearStartMonth,
    DateTime BooksStartDate,
    bool EnableMultiCurrency,
    byte RoundingPrecision,
    RoundingMode? RoundingMode,

    // System & Posting Controls
    DateTime? AllowPostingFromDate,
    DateTime? AllowPostingToDate,
    bool LockBackDatedPosting,
    string? Notes
) : IRequest<CompanyDto>;
