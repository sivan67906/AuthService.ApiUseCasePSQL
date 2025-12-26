using AuthService.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Application.Features.Lookups;

// DTOs
public sealed class CountryLookupDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? PhoneCode { get; set; }
}

public sealed class StateLookupDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public Guid CountryId { get; set; }
}

public sealed class CityLookupDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public Guid StateId { get; set; }
    public string? PostalCode { get; set; }
}

public sealed class CurrencyLookupDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Symbol { get; set; } = string.Empty;
}

public sealed class TimeZoneLookupDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Identifier { get; set; } = string.Empty;
    public string Offset { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
    public bool IsDefault { get; set; }
}

public sealed class CompanyLookupDto
{
    public Guid Id { get; set; }
    public string CompanyCode { get; set; } = string.Empty;
    public string LegalName { get; set; } = string.Empty;
}

// Queries
public sealed record GetCountriesQuery : IRequest<List<CountryLookupDto>>;
public sealed record GetStatesByCountryQuery(Guid CountryId) : IRequest<List<StateLookupDto>>;
public sealed record GetCitiesByStateQuery(Guid StateId) : IRequest<List<CityLookupDto>>;
public sealed record GetCurrenciesQuery : IRequest<List<CurrencyLookupDto>>;
public sealed record GetTimeZonesQuery : IRequest<List<TimeZoneLookupDto>>;
public sealed record GetTimezonesByCountryQuery(Guid CountryId) : IRequest<List<TimeZoneLookupDto>>;
public sealed record GetCompaniesLookupQuery : IRequest<List<CompanyLookupDto>>;

// Handlers
public sealed class GetCountriesQueryHandler : IRequestHandler<GetCountriesQuery, List<CountryLookupDto>>
{
    private readonly IAppDbContext _db;
    public GetCountriesQueryHandler(IAppDbContext db)
    {
        _db = db;
    }

    public async Task<List<CountryLookupDto>> Handle(GetCountriesQuery request, CancellationToken cancellationToken)
    {
        return await _db.Countries
            .Where(x => x.IsActive)
            .OrderBy(x => x.DisplayOrder).ThenBy(x => x.Name)
            .Select(x => new CountryLookupDto
            {
                Id = x.Id,
                Name = x.Name,
                Code = x.Code,
                PhoneCode = x.PhoneCode
            })
            .ToListAsync(cancellationToken);
    }
}

public sealed class GetStatesByCountryQueryHandler : IRequestHandler<GetStatesByCountryQuery, List<StateLookupDto>>
{
    private readonly IAppDbContext _db;
    public GetStatesByCountryQueryHandler(IAppDbContext db)
    {
        _db = db;
    }

    public async Task<List<StateLookupDto>> Handle(GetStatesByCountryQuery request, CancellationToken cancellationToken)
    {
        return await _db.States
            .Where(x => x.CountryId == request.CountryId && x.IsActive)
            .OrderBy(x => x.DisplayOrder).ThenBy(x => x.Name)
            .Select(x => new StateLookupDto
            {
                Id = x.Id,
                Name = x.Name,
                Code = x.Code,
                CountryId = x.CountryId
            })
            .ToListAsync(cancellationToken);
    }
}

public sealed class GetCitiesByStateQueryHandler : IRequestHandler<GetCitiesByStateQuery, List<CityLookupDto>>
{
    private readonly IAppDbContext _db;
    public GetCitiesByStateQueryHandler(IAppDbContext db)
    {
        _db = db;
    }

    public async Task<List<CityLookupDto>> Handle(GetCitiesByStateQuery request, CancellationToken cancellationToken)
    {
        return await _db.Cities
            .Where(x => x.StateId == request.StateId && x.IsActive)
            .OrderBy(x => x.DisplayOrder).ThenBy(x => x.Name)
            .Select(x => new CityLookupDto
            {
                Id = x.Id,
                Name = x.Name,
                StateId = x.StateId,
                PostalCode = x.PostalCode
            })
            .ToListAsync(cancellationToken);
    }
}

public sealed class GetCurrenciesQueryHandler : IRequestHandler<GetCurrenciesQuery, List<CurrencyLookupDto>>
{
    private readonly IAppDbContext _db;
    public GetCurrenciesQueryHandler(IAppDbContext db)
    {
        _db = db;
    }

    public async Task<List<CurrencyLookupDto>> Handle(GetCurrenciesQuery request, CancellationToken cancellationToken)
    {
        return await _db.Currencies
            .Where(x => x.IsActive)
            .OrderBy(x => x.DisplayOrder).ThenBy(x => x.Name)
            .Select(x => new CurrencyLookupDto
            {
                Id = x.Id,
                Code = x.Code,
                Name = x.Name,
                Symbol = x.Symbol
            })
            .ToListAsync(cancellationToken);
    }
}

public sealed class GetTimeZonesQueryHandler : IRequestHandler<GetTimeZonesQuery, List<TimeZoneLookupDto>>
{
    private readonly IAppDbContext _db;
    public GetTimeZonesQueryHandler(IAppDbContext db)
    {
        _db = db;
    }

    public async Task<List<TimeZoneLookupDto>> Handle(GetTimeZonesQuery request, CancellationToken cancellationToken)
    {
        return await _db.TimeZones
            .Where(x => x.IsActive)
            .OrderBy(x => x.DisplayOrder).ThenBy(x => x.Offset)
            .Select(x => new TimeZoneLookupDto
            {
                Id = x.Id,
                Name = x.Name,
                Identifier = x.Identifier,
                Offset = x.Offset,
                DisplayName = x.DisplayName,
                IsDefault = false // Default is false for all timezones list
            })
            .ToListAsync(cancellationToken);
    }
}

public sealed class GetTimezonesByCountryQueryHandler : IRequestHandler<GetTimezonesByCountryQuery, List<TimeZoneLookupDto>>
{
    private readonly IAppDbContext _db;
    public GetTimezonesByCountryQueryHandler(IAppDbContext db)
    {
        _db = db;
    }

    public async Task<List<TimeZoneLookupDto>> Handle(GetTimezonesByCountryQuery request, CancellationToken cancellationToken)
    {
        return await _db.CountryTimeZones
            .Where(x => x.CountryId == request.CountryId)
            .Include(x => x.TimeZoneEntity)
            .OrderByDescending(x => x.IsDefault) // Default timezone first
            .ThenBy(x => x.TimeZoneEntity.DisplayName)
            .Select(x => new TimeZoneLookupDto
            {
                Id = x.TimeZoneEntity.Id,
                Name = x.TimeZoneEntity.Name,
                Identifier = x.TimeZoneEntity.Identifier,
                Offset = x.TimeZoneEntity.Offset,
                DisplayName = x.TimeZoneEntity.DisplayName,
                IsDefault = x.IsDefault
            })
            .ToListAsync(cancellationToken);
    }
}

public sealed class GetCompaniesLookupQueryHandler : IRequestHandler<GetCompaniesLookupQuery, List<CompanyLookupDto>>
{
    private readonly IAppDbContext _db;
    public GetCompaniesLookupQueryHandler(IAppDbContext db)
    {
        _db = db;
    }

    public async Task<List<CompanyLookupDto>> Handle(GetCompaniesLookupQuery request, CancellationToken cancellationToken)
    {
        return await _db.Companies
            .OrderBy(x => x.LegalName)
            .Select(x => new CompanyLookupDto
            {
                Id = x.Id,
                CompanyCode = x.CompanyCode,
                LegalName = x.LegalName
            })
            .ToListAsync(cancellationToken);
    }
}
