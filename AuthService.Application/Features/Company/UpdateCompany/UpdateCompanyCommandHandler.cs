using AuthService.Application.Common.Interfaces;
using AuthService.Application.Features.Company.CreateCompany;
using AuthService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Application.Features.Company.UpdateCompany;

public sealed class UpdateCompanyCommandHandler : IRequestHandler<UpdateCompanyCommand, CompanyDto>
{
    private readonly IAppDbContext _db;

    public UpdateCompanyCommandHandler(IAppDbContext db)
    {
        _db = db;
    }

    public async Task<CompanyDto> Handle(UpdateCompanyCommand request, CancellationToken cancellationToken)
    {
        var entity = await _db.Companies
            .Include(c => c.ParentCompany)
            .Include(c => c.RegistrationCountry)
            .Include(c => c.RegistrationState)
            .Include(c => c.AddressCountry)
            .Include(c => c.AddressState)
            .Include(c => c.City)
            .Include(c => c.TimeZone)
            .Include(c => c.BaseCurrency)
            .Include(c => c.ReportingCurrency)
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (entity == null)
        {
            throw new InvalidOperationException("Company not found.");
        }

        // Validate LegalName uniqueness (excluding self)
        var existingByName = await _db.Companies
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.LegalName.ToLower() == request.LegalName.ToLower() && x.Id != request.Id, cancellationToken);

        if (existingByName != null)
        {
            throw new InvalidOperationException($"Another company with name '{request.LegalName}' already exists.");
        }

        // Validate IncorporationDate
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

        // Validate parent company if specified
        if (request.ParentCompanyId.HasValue)
        {
            if (request.ParentCompanyId.Value == request.Id)
            {
                throw new InvalidOperationException("Company cannot be its own parent.");
            }

            var parentExists = await _db.Companies
                .AnyAsync(x => x.Id == request.ParentCompanyId.Value, cancellationToken);
            if (!parentExists)
            {
                throw new InvalidOperationException("Parent company not found.");
            }
        }

        // Check for circular reference in parent hierarchy
        if (request.ParentCompanyId.HasValue)
        {
            await ValidateNoCircularReference(request.Id, request.ParentCompanyId.Value, cancellationToken);
        }

        // Business rule: Only Notes can be edited in inactive mode
        if (entity.Status == CompanyStatus.Inactive)
        {
            if (IsAnyFieldChangedBesidesNotes(entity, request))
            {
                throw new InvalidOperationException("Only Notes can be edited when company is inactive.");
            }
            entity.Notes = request.Notes?.Trim();
        }
        else
        {
            // Update all fields
            entity.LegalName = request.LegalName.Trim();
            entity.TradeName = request.TradeName?.Trim();
            entity.ShortName = request.ShortName?.Trim();
            entity.LegalStructure = request.LegalStructure;
            entity.IncorporationDate = request.IncorporationDate?.Date;
            entity.ParentCompanyId = request.ParentCompanyId;
            entity.Status = request.Status;

            entity.RegistrationNumber = request.RegistrationNumber?.Trim();
            entity.PANNumber = request.PANNumber?.ToUpper().Trim();
            entity.GSTIN = request.GSTIN?.ToUpper().Trim();
            entity.TANNumber = request.TANNumber?.ToUpper().Trim();
            entity.OtherTaxId = request.OtherTaxId?.Trim();
            entity.RegistrationCountryId = request.RegistrationCountryId;
            entity.RegistrationStateId = request.RegistrationStateId;

            entity.AddressLine1 = request.AddressLine1.Trim();
            entity.AddressLine2 = request.AddressLine2?.Trim();
            entity.CityId = request.CityId;
            entity.StateId = request.StateId;
            entity.PostalCode = request.PostalCode.Trim();
            entity.CountryId = request.CountryId;
            entity.TimeZoneId = request.TimeZoneId;

            entity.PrimaryContactName = request.PrimaryContactName?.Trim();
            entity.PrimaryEmail = request.PrimaryEmail?.ToLower().Trim();
            entity.PrimaryPhone = request.PrimaryPhone?.Trim();
            entity.WebsiteUrl = request.WebsiteUrl?.Trim();
            entity.LogoFileUrl = request.LogoFileUrl?.Trim();

            entity.BaseCurrencyId = request.BaseCurrencyId;
            entity.ReportingCurrencyId = request.ReportingCurrencyId;
            entity.FiscalYearStartMonth = request.FiscalYearStartMonth;
            entity.BooksStartDate = DateTime.SpecifyKind(request.BooksStartDate.Date, DateTimeKind.Utc);
            entity.EnableMultiCurrency = request.EnableMultiCurrency;
            entity.RoundingPrecision = request.RoundingPrecision;
            entity.RoundingMode = request.RoundingMode;

            entity.AllowPostingFromDate = request.AllowPostingFromDate?.Date;
            entity.AllowPostingToDate = request.AllowPostingToDate?.Date;
            entity.LockBackDatedPosting = request.LockBackDatedPosting;
            entity.Notes = request.Notes?.Trim();
        }

        await _db.SaveChangesAsync(cancellationToken);

        // Reload with navigation properties
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

    private async Task ValidateNoCircularReference(Guid companyId, Guid parentId, CancellationToken cancellationToken)
    {
        var visited = new HashSet<Guid> { companyId };
        var currentParentId = parentId;

        while (currentParentId != Guid.Empty)
        {
            if (visited.Contains(currentParentId))
            {
                throw new InvalidOperationException("Circular reference detected in parent company hierarchy.");
            }

            visited.Add(currentParentId);

            var parent = await _db.Companies
                .Where(c => c.Id == currentParentId)
                .Select(c => c.ParentCompanyId)
                .FirstOrDefaultAsync(cancellationToken);

            currentParentId = parent ?? Guid.Empty;
        }
    }

    private static bool IsAnyFieldChangedBesidesNotes(Domain.Entities.Company entity, UpdateCompanyCommand request)
    {
        return entity.LegalName != request.LegalName.Trim() ||
               entity.TradeName != request.TradeName?.Trim() ||
               entity.ShortName != request.ShortName?.Trim() ||
               entity.LegalStructure != request.LegalStructure ||
               entity.IncorporationDate != request.IncorporationDate ||
               entity.ParentCompanyId != request.ParentCompanyId ||
               entity.Status != request.Status;
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
