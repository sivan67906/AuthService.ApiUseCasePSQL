using AuthService.Application.Common.Interfaces;
using AuthService.Application.Features.Company.CreateCompany;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Application.Features.Company.GetAllCompanies;

public sealed record GetAllCompaniesQuery : IRequest<List<CompanyDto>>;

public sealed class GetAllCompaniesQueryHandler : IRequestHandler<GetAllCompaniesQuery, List<CompanyDto>>
{
    private readonly IAppDbContext _db;

    public GetAllCompaniesQueryHandler(IAppDbContext db)
    {
        _db = db;
    }

    public async Task<List<CompanyDto>> Handle(GetAllCompaniesQuery request, CancellationToken cancellationToken)
    {
        var entities = await _db.Companies
            .AsNoTracking()
            .Where(x => !x.IsDeleted)
            .Include(c => c.ParentCompany)
            .Include(c => c.RegistrationCountry)
            .Include(c => c.RegistrationState)
            .Include(c => c.AddressCountry)
            .Include(c => c.AddressState)
            .Include(c => c.City)
            .Include(c => c.TimeZone)
            .Include(c => c.BaseCurrency)
            .Include(c => c.ReportingCurrency)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

        return entities.Select(entity => new CompanyDto
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
        }).ToList();
    }
}
