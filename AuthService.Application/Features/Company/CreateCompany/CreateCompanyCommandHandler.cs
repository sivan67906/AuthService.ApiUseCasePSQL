using AuthService.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Application.Features.Company.CreateCompany;

public sealed class CreateCompanyCommandHandler : IRequestHandler<CreateCompanyCommand, CompanyDto>
{
    private readonly IAppDbContext _db;

    public CreateCompanyCommandHandler(IAppDbContext db)
    {
        _db = db;
    }

    public async Task<CompanyDto> Handle(CreateCompanyCommand request, CancellationToken cancellationToken)
    {
        // Validate CompanyCode uniqueness (including soft-deleted)
        var existingByCode = await _db.Companies
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.CompanyCode.ToUpper() == request.CompanyCode.ToUpper(), cancellationToken);

        if (existingByCode != null)
        {
            if (existingByCode.IsDeleted)
            {
                throw new InvalidOperationException($"A company with code '{request.CompanyCode}' already exists in deactivated mode. Please use a different code.");
            }
            throw new InvalidOperationException($"Company with code '{request.CompanyCode}' already exists.");
        }

        // Validate LegalName uniqueness
        var existingByName = await _db.Companies
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.LegalName.ToLower() == request.LegalName.ToLower(), cancellationToken);

        if (existingByName != null)
        {
            if (existingByName.IsDeleted)
            {
                throw new InvalidOperationException($"A company with name '{request.LegalName}' already exists in deactivated mode. Please use a different name.");
            }
            throw new InvalidOperationException($"Company with name '{request.LegalName}' already exists.");
        }

        // Validate IncorporationDate if provided
        if (request.IncorporationDate.HasValue)
        {
            if (request.IncorporationDate.Value > DateTime.UtcNow.Date)
            {
                throw new InvalidOperationException("Incorporation date cannot be in the future.");
            }
            if (request.IncorporationDate.Value.Year < 1900)
            {
                throw new InvalidOperationException("Incorporation date must be after 1900.");
            }
        }

        // Validate BooksStartDate >= IncorporationDate
        if (request.IncorporationDate.HasValue && request.BooksStartDate < request.IncorporationDate.Value)
        {
            throw new InvalidOperationException("Books start date must be on or after the incorporation date.");
        }

        // Validate FiscalYearStartMonth
        if (request.FiscalYearStartMonth < 1 || request.FiscalYearStartMonth > 12)
        {
            throw new InvalidOperationException("Fiscal year start month must be between 1 and 12.");
        }

        // Validate RoundingPrecision
        if (request.RoundingPrecision > 4)
        {
            throw new InvalidOperationException("Rounding precision must be between 0 and 4.");
        }

        // Validate parent company exists if specified
        if (request.ParentCompanyId.HasValue)
        {
            var parentExists = await _db.Companies
                .AnyAsync(x => x.Id == request.ParentCompanyId.Value, cancellationToken);
            if (!parentExists)
            {
                throw new InvalidOperationException("Parent company not found.");
            }
        }

        // Create the company entity
        var entity = new Domain.Entities.Company
        {
            CompanyCode = request.CompanyCode.ToUpper().Trim(),
            LegalName = request.LegalName.Trim(),
            TradeName = request.TradeName?.Trim(),
            ShortName = request.ShortName?.Trim(),
            LegalStructure = request.LegalStructure,
            IncorporationDate = request.IncorporationDate?.Date,
            ParentCompanyId = request.ParentCompanyId,
            Status = Domain.Entities.CompanyStatus.Draft,

            // Registration
            RegistrationNumber = request.RegistrationNumber?.Trim(),
            PANNumber = request.PANNumber?.ToUpper().Trim(),
            GSTIN = request.GSTIN?.ToUpper().Trim(),
            TANNumber = request.TANNumber?.ToUpper().Trim(),
            OtherTaxId = request.OtherTaxId?.Trim(),
            RegistrationCountryId = request.RegistrationCountryId,
            RegistrationStateId = request.RegistrationStateId,

            // Address
            AddressLine1 = request.AddressLine1.Trim(),
            AddressLine2 = request.AddressLine2?.Trim(),
            CityId = request.CityId,
            StateId = request.StateId,
            PostalCode = request.PostalCode.Trim(),
            CountryId = request.CountryId,
            TimeZoneId = request.TimeZoneId,

            // Contact
            PrimaryContactName = request.PrimaryContactName?.Trim(),
            PrimaryEmail = request.PrimaryEmail?.ToLower().Trim(),
            PrimaryPhone = request.PrimaryPhone?.Trim(),
            WebsiteUrl = request.WebsiteUrl?.Trim(),
            LogoFileUrl = request.LogoFileUrl?.Trim(),

            // Financial
            BaseCurrencyId = request.BaseCurrencyId,
            ReportingCurrencyId = request.ReportingCurrencyId,
            FiscalYearStartMonth = request.FiscalYearStartMonth,
            BooksStartDate = DateTime.SpecifyKind(request.BooksStartDate.Date, DateTimeKind.Utc),
            EnableMultiCurrency = request.EnableMultiCurrency,
            RoundingPrecision = request.RoundingPrecision,
            RoundingMode = request.RoundingMode,

            // Posting Controls
            AllowPostingFromDate = request.AllowPostingFromDate?.Date,
            AllowPostingToDate = request.AllowPostingToDate?.Date,
            LockBackDatedPosting = request.LockBackDatedPosting,
            Notes = request.Notes?.Trim()
        };

        _db.Companies.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);

        // Fetch with navigation properties for DTO mapping
        var result = await _db.Companies
            .Include(c => c.ParentCompany)
            .Include(c => c.RegistrationCountry)
            .Include(c => c.RegistrationState)
            .Include(c => c.AddressCountry)
            .Include(c => c.AddressState)
            .Include(c => c.City)
            .Include(c => c.TimeZone)
            .Include(c => c.BaseCurrency)
            .Include(c => c.ReportingCurrency)
            .FirstAsync(c => c.Id == entity.Id, cancellationToken);

        return MapToDto(result);
    }

    private static CompanyDto MapToDto(Domain.Entities.Company entity)
    {
        return new CompanyDto
        {
            Id = entity.Id,
            CompanyCode = entity.CompanyCode,
            LegalName = entity.LegalName,
            TradeName = entity.TradeName,
            ShortName = entity.ShortName,
            LegalStructure = entity.LegalStructure,
            IncorporationDate = entity.IncorporationDate,
            ParentCompanyId = entity.ParentCompanyId,
            ParentCompanyName = entity.ParentCompany?.LegalName,
            Status = entity.Status,

            RegistrationNumber = entity.RegistrationNumber,
            PANNumber = entity.PANNumber,
            GSTIN = entity.GSTIN,
            TANNumber = entity.TANNumber,
            OtherTaxId = entity.OtherTaxId,
            RegistrationCountryId = entity.RegistrationCountryId,
            RegistrationCountryName = entity.RegistrationCountry?.Name,
            RegistrationStateId = entity.RegistrationStateId,
            RegistrationStateName = entity.RegistrationState?.Name,

            AddressLine1 = entity.AddressLine1,
            AddressLine2 = entity.AddressLine2,
            CityId = entity.CityId,
            CityName = entity.City?.Name,
            StateId = entity.StateId,
            StateName = entity.AddressState?.Name,
            PostalCode = entity.PostalCode,
            CountryId = entity.CountryId,
            CountryName = entity.AddressCountry?.Name,
            TimeZoneId = entity.TimeZoneId,
            TimeZoneName = entity.TimeZone?.DisplayName ?? entity.TimeZone?.Name,

            PrimaryContactName = entity.PrimaryContactName,
            PrimaryEmail = entity.PrimaryEmail,
            PrimaryPhone = entity.PrimaryPhone,
            WebsiteUrl = entity.WebsiteUrl,
            LogoFileUrl = entity.LogoFileUrl,

            BaseCurrencyId = entity.BaseCurrencyId,
            BaseCurrencyName = entity.BaseCurrency?.Name,
            BaseCurrencyCode = entity.BaseCurrency?.Code,
            ReportingCurrencyId = entity.ReportingCurrencyId,
            ReportingCurrencyName = entity.ReportingCurrency?.Name,
            ReportingCurrencyCode = entity.ReportingCurrency?.Code,
            FiscalYearStartMonth = entity.FiscalYearStartMonth,
            BooksStartDate = entity.BooksStartDate,
            EnableMultiCurrency = entity.EnableMultiCurrency,
            RoundingPrecision = entity.RoundingPrecision,
            RoundingMode = entity.RoundingMode,

            AllowPostingFromDate = entity.AllowPostingFromDate,
            AllowPostingToDate = entity.AllowPostingToDate,
            LockBackDatedPosting = entity.LockBackDatedPosting,
            Notes = entity.Notes,

            CreatedAt = entity.CreatedAt,
            CreatedBy = entity.CreatedBy,
            UpdatedAt = entity.UpdatedAt,
            ModifiedBy = entity.ModifiedBy,
            IsDeleted = entity.IsDeleted
        };
    }
}
