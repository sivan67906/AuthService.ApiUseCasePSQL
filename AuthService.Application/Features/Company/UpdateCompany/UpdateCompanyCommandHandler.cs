using AuthService.Application.Common.Interfaces;
using AuthService.Application.Features.Company.CreateCompany;
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

        // Validate GSTIN uniqueness if provided (excluding self)
        if (!string.IsNullOrWhiteSpace(request.GSTIN))
        {
            var existingByGSTIN = await _db.Companies
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.GSTIN != null && x.GSTIN.ToUpper() == request.GSTIN.ToUpper() && x.Id != request.Id, cancellationToken);

            if (existingByGSTIN != null)
            {
                throw new InvalidOperationException($"Another company with GSTIN '{request.GSTIN}' already exists.");
            }
        }

        // Validate PAN uniqueness if provided (excluding self)
        if (!string.IsNullOrWhiteSpace(request.PANNumber))
        {
            var existingByPAN = await _db.Companies
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.PANNumber != null && x.PANNumber.ToUpper() == request.PANNumber.ToUpper() && x.Id != request.Id, cancellationToken);

            if (existingByPAN != null)
            {
                throw new InvalidOperationException($"Another company with PAN '{request.PANNumber}' already exists.");
            }
        }

        // Validate TAN uniqueness if provided (excluding self)
        if (!string.IsNullOrWhiteSpace(request.TANNumber))
        {
            var existingByTAN = await _db.Companies
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.TANNumber != null && x.TANNumber.ToUpper() == request.TANNumber.ToUpper() && x.Id != request.Id, cancellationToken);

            if (existingByTAN != null)
            {
                throw new InvalidOperationException($"Another company with TAN '{request.TANNumber}' already exists.");
            }
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
        // Only validate if parent is actually being changed to avoid false positives
        if (request.ParentCompanyId.HasValue && entity.ParentCompanyId != request.ParentCompanyId)
        {
            Console.WriteLine($"[UpdateCompany] Parent changing from {entity.ParentCompanyId} to {request.ParentCompanyId}, validating for circular reference...");
            await ValidateNoCircularReference(request.Id, request.ParentCompanyId.Value, cancellationToken);
        }
        else if (request.ParentCompanyId.HasValue)
        {
            Console.WriteLine($"[UpdateCompany] Parent unchanged ({request.ParentCompanyId}), skipping circular reference validation");
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
            // Log current and new values for debugging
            Console.WriteLine($"[UpdateCompany] Updating Company {entity.Id}");
            Console.WriteLine($"[UpdateCompany] BooksStartDate: {entity.BooksStartDate:yyyy-MM-dd} -> {request.BooksStartDate:yyyy-MM-dd}");
            Console.WriteLine($"[UpdateCompany] Status: {entity.Status} -> {request.Status}");
            Console.WriteLine($"[UpdateCompany] LegalName: {entity.LegalName} -> {request.LegalName}");

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

            // Explicitly mark entity as modified to ensure EF Core tracks the changes
            _db.Entry(entity).State = EntityState.Modified;
            Console.WriteLine($"[UpdateCompany] Entity state explicitly set to Modified");
        }

        var savedCount = await _db.SaveChangesAsync(cancellationToken);

        Console.WriteLine($"[UpdateCompany] Company {entity.Id} saved successfully, {savedCount} record(s) affected");
        Console.WriteLine($"[UpdateCompany] Final BooksStartDate in entity: {entity.BooksStartDate:yyyy-MM-dd}");

        // CRITICAL: Detach the entity from change tracker to ensure fresh data on reload
        _db.Entry(entity).State = EntityState.Detached;
        Console.WriteLine($"[UpdateCompany] Entity detached from change tracker");

        // Reload with navigation properties using AsNoTracking for fresh data from database
        var result = await _db.Companies
            .AsNoTracking()
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

        Console.WriteLine($"[UpdateCompany] Reloaded entity from DB - BooksStartDate: {result.BooksStartDate:yyyy-MM-dd}");
        Console.WriteLine($"[UpdateCompany] Reloaded entity from DB - Status: {result.Status}");
        Console.WriteLine($"[UpdateCompany] Reloaded entity from DB - LegalName: {result.LegalName}");

        var dto = MapToDto(result);
        Console.WriteLine($"[UpdateCompany] DTO BooksStartDate: {dto.BooksStartDate:yyyy-MM-dd}");
        Console.WriteLine($"[UpdateCompany] DTO Status: {dto.Status}");

        return dto;
    }

    private async Task ValidateNoCircularReference(Guid companyId, Guid parentId, CancellationToken cancellationToken)
    {
        // Get company names for better error messages
        var companyNames = await _db.Companies
            .Where(c => c.Id == companyId || c.Id == parentId)
            .Select(c => new { c.Id, c.LegalName })
            .ToListAsync(cancellationToken);

        var companyName = companyNames.FirstOrDefault(c => c.Id == companyId)?.LegalName ?? companyId.ToString();
        var parentName = companyNames.FirstOrDefault(c => c.Id == parentId)?.LegalName ?? parentId.ToString();

        Console.WriteLine($"[ValidateCircular] Checking if setting '{companyName}' parent to '{parentName}' creates circular reference");

        var visited = new HashSet<Guid> { companyId };
        var hierarchyPath = new List<string> { companyName };
        var currentParentId = parentId;

        while (currentParentId != Guid.Empty)
        {
            if (visited.Contains(currentParentId))
            {
                // Build the circular path for error message
                var circularCompany = await _db.Companies
                    .Where(c => c.Id == currentParentId)
                    .Select(c => c.LegalName)
                    .FirstOrDefaultAsync(cancellationToken);

                hierarchyPath.Add(circularCompany ?? currentParentId.ToString());
                var path = string.Join(" → ", hierarchyPath);

                Console.WriteLine($"[ValidateCircular] CIRCULAR REFERENCE DETECTED: {path}");
                throw new InvalidOperationException(
                    $"Circular reference detected in parent company hierarchy. " +
                    $"Setting '{companyName}' as a child of '{parentName}' would create a circular reference: {path}");
            }

            visited.Add(currentParentId);

            var parentInfo = await _db.Companies
                .Where(c => c.Id == currentParentId)
                .Select(c => new { c.ParentCompanyId, c.LegalName })
                .FirstOrDefaultAsync(cancellationToken);

            if (parentInfo != null)
            {
                hierarchyPath.Add(parentInfo.LegalName);
                currentParentId = parentInfo.ParentCompanyId ?? Guid.Empty;
                Console.WriteLine($"[ValidateCircular] Current hierarchy path: {string.Join(" → ", hierarchyPath)}");
            }
            else
            {
                currentParentId = Guid.Empty;
            }
        }

        Console.WriteLine($"[ValidateCircular] No circular reference detected. Final path: {string.Join(" → ", hierarchyPath)}");
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
