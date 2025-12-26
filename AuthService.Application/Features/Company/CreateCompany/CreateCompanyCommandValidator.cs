using System.Text.RegularExpressions;

namespace AuthService.Application.Features.Company.CreateCompany;

public sealed class CreateCompanyCommandValidator : AbstractValidator<CreateCompanyCommand>
{
    public CreateCompanyCommandValidator()
    {
        // Company Identity
        RuleFor(x => x.CompanyCode)
            .NotEmpty().WithMessage("Company code is required.")
            .MaximumLength(10).WithMessage("Company code cannot exceed 10 characters.")
            .Matches(@"^[A-Z0-9\-_]+$", RegexOptions.IgnoreCase)
            .WithMessage("Company code can only contain uppercase letters, numbers, hyphens, and underscores.");

        RuleFor(x => x.LegalName)
            .NotEmpty().WithMessage("Legal name is required.")
            .MaximumLength(200).WithMessage("Legal name cannot exceed 200 characters.");

        RuleFor(x => x.TradeName)
            .MaximumLength(150).WithMessage("Trade name cannot exceed 150 characters.")
            .When(x => !string.IsNullOrEmpty(x.TradeName));

        RuleFor(x => x.ShortName)
            .MaximumLength(50).WithMessage("Short name cannot exceed 50 characters.")
            .When(x => !string.IsNullOrEmpty(x.ShortName));

        RuleFor(x => x.LegalStructure)
            .IsInEnum().WithMessage("Please select a valid legal structure.");

        RuleFor(x => x.IncorporationDate)
            .LessThanOrEqualTo(DateTime.UtcNow.Date).WithMessage("Incorporation date cannot be in the future.")
            .Must(d => d == null || d.Value.Year >= 1900).WithMessage("Incorporation date must be after 1900.")
            .When(x => x.IncorporationDate.HasValue);

        // Registration & Compliance
        RuleFor(x => x.RegistrationNumber)
            .MaximumLength(50).WithMessage("Registration number cannot exceed 50 characters.")
            .When(x => !string.IsNullOrEmpty(x.RegistrationNumber));

        RuleFor(x => x.PANNumber)
            .Length(10).WithMessage("PAN number must be exactly 10 characters.")
            .Matches(@"^[A-Z]{5}[0-9]{4}[A-Z]$", RegexOptions.IgnoreCase)
            .WithMessage("Invalid PAN format. Expected format: ABCDE1234F")
            .When(x => !string.IsNullOrEmpty(x.PANNumber));

        RuleFor(x => x.GSTIN)
            .Length(15).WithMessage("GSTIN must be exactly 15 characters.")
            .Matches(@"^[0-9]{2}[A-Z]{5}[0-9]{4}[A-Z]{1}[1-9A-Z]{1}Z[0-9A-Z]{1}$", RegexOptions.IgnoreCase)
            .WithMessage("Invalid GSTIN format.")
            .When(x => !string.IsNullOrEmpty(x.GSTIN));

        RuleFor(x => x.TANNumber)
            .Length(10).WithMessage("TAN number must be exactly 10 characters.")
            .Matches(@"^[A-Z]{4}[0-9]{5}[A-Z]$", RegexOptions.IgnoreCase)
            .WithMessage("Invalid TAN format.")
            .When(x => !string.IsNullOrEmpty(x.TANNumber));

        RuleFor(x => x.OtherTaxId)
            .MaximumLength(50).WithMessage("Other tax ID cannot exceed 50 characters.")
            .When(x => !string.IsNullOrEmpty(x.OtherTaxId));

        RuleFor(x => x.RegistrationCountryId)
            .NotEmpty().WithMessage("Registration country is required.");

        // Registered Address
        RuleFor(x => x.AddressLine1)
            .NotEmpty().WithMessage("Address line 1 is required.")
            .MaximumLength(200).WithMessage("Address line 1 cannot exceed 200 characters.");

        RuleFor(x => x.AddressLine2)
            .MaximumLength(200).WithMessage("Address line 2 cannot exceed 200 characters.")
            .When(x => !string.IsNullOrEmpty(x.AddressLine2));

        RuleFor(x => x.CityId)
            .NotEmpty().WithMessage("City is required.");

        RuleFor(x => x.StateId)
            .NotEmpty().WithMessage("State/Province is required.");

        RuleFor(x => x.PostalCode)
            .NotEmpty().WithMessage("Postal code is required.")
            .MaximumLength(20).WithMessage("Postal code cannot exceed 20 characters.");

        RuleFor(x => x.CountryId)
            .NotEmpty().WithMessage("Country is required.");

        RuleFor(x => x.TimeZoneId)
            .NotEmpty().WithMessage("Time zone is required.");

        // Contact & Branding
        RuleFor(x => x.PrimaryContactName)
            .MaximumLength(100).WithMessage("Primary contact name cannot exceed 100 characters.")
            .When(x => !string.IsNullOrEmpty(x.PrimaryContactName));

        RuleFor(x => x.PrimaryEmail)
            .EmailAddress().WithMessage("Invalid email format.")
            .MaximumLength(150).WithMessage("Email cannot exceed 150 characters.")
            .When(x => !string.IsNullOrEmpty(x.PrimaryEmail));

        RuleFor(x => x.PrimaryPhone)
            .MaximumLength(30).WithMessage("Phone number cannot exceed 30 characters.")
            .Matches(@"^[\d\s\+\-\(\)]+$").WithMessage("Phone number contains invalid characters.")
            .When(x => !string.IsNullOrEmpty(x.PrimaryPhone));

        RuleFor(x => x.WebsiteUrl)
            .MaximumLength(200).WithMessage("Website URL cannot exceed 200 characters.")
            .Must(BeAValidUrl).WithMessage("Invalid website URL format.")
            .When(x => !string.IsNullOrEmpty(x.WebsiteUrl));

        // LogoFileUrl - No length restriction to support base64 encoded images

        // Financial Settings
        RuleFor(x => x.BaseCurrencyId)
            .NotEmpty().WithMessage("Base currency is required.");

        RuleFor(x => x.FiscalYearStartMonth)
            .InclusiveBetween((byte)1, (byte)12).WithMessage("Fiscal year start month must be between 1 and 12.");

        RuleFor(x => x.BooksStartDate)
            .NotEmpty().WithMessage("Books start date is required.")
            .GreaterThanOrEqualTo(x => x.IncorporationDate ?? DateTime.MinValue)
            .WithMessage("Books start date must be on or after incorporation date.")
            .When(x => x.IncorporationDate.HasValue);

        RuleFor(x => x.RoundingPrecision)
            .InclusiveBetween((byte)0, (byte)4).WithMessage("Rounding precision must be between 0 and 4.");

        // System & Posting Controls
        RuleFor(x => x.AllowPostingToDate)
            .GreaterThan(x => x.AllowPostingFromDate)
            .WithMessage("Allow posting to date must be after allow posting from date.")
            .When(x => x.AllowPostingFromDate.HasValue && x.AllowPostingToDate.HasValue);

        RuleFor(x => x.Notes)
            .MaximumLength(1000).WithMessage("Notes cannot exceed 1000 characters.")
            .When(x => !string.IsNullOrEmpty(x.Notes));
    }

    private static bool BeAValidUrl(string? url)
    {
        if (string.IsNullOrEmpty(url)) return true;
        return Uri.TryCreate(url, UriKind.Absolute, out var uriResult)
               && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
    }
}
