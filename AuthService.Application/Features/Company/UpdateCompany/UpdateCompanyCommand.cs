using AuthService.Domain.Entities;
using AuthService.Application.Features.Company.CreateCompany;

namespace AuthService.Application.Features.Company.UpdateCompany;

/// <summary>
/// Command to update an existing company
/// </summary>
public sealed record UpdateCompanyCommand(
    Guid Id,
    
    // Identity (CompanyCode cannot be changed after creation)
    string LegalName,
    string? TradeName,
    string? ShortName,
    LegalStructure LegalStructure,
    DateTime? IncorporationDate,
    Guid? ParentCompanyId,
    CompanyStatus Status,

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

    // Financial Settings (BaseCurrency cannot be changed after postings)
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
