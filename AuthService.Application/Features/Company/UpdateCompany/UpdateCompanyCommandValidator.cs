using System.Text.RegularExpressions;
using FluentValidation;

namespace AuthService.Application.Features.Company.UpdateCompany;

public sealed class UpdateCompanyCommandValidator : AbstractValidator<UpdateCompanyCommand>
{
    public UpdateCompanyCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Company ID is required.");

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

        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("Please select a valid status.");

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
            .WithMessage("Invalid PAN format.")
            .When(x => !string.IsNullOrEmpty(x.PANNumber));

        RuleFor(x => x.GSTIN)
            .Length(15).WithMessage("GSTIN must be exactly 15 characters.")
            .When(x => !string.IsNullOrEmpty(x.GSTIN));

        RuleFor(x => x.TANNumber)
            .Length(10).WithMessage("TAN number must be exactly 10 characters.")
            .When(x => !string.IsNullOrEmpty(x.TANNumber));

        RuleFor(x => x.RegistrationCountryId)
            .NotEmpty().WithMessage("Registration country is required.");

        // Address
        RuleFor(x => x.AddressLine1)
            .NotEmpty().WithMessage("Address line 1 is required.")
            .MaximumLength(200).WithMessage("Address line 1 cannot exceed 200 characters.");

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

        // Contact
        RuleFor(x => x.PrimaryEmail)
            .EmailAddress().WithMessage("Invalid email format.")
            .When(x => !string.IsNullOrEmpty(x.PrimaryEmail));

        // Financial
        RuleFor(x => x.BaseCurrencyId)
            .NotEmpty().WithMessage("Base currency is required.");

        RuleFor(x => x.FiscalYearStartMonth)
            .InclusiveBetween((byte)1, (byte)12).WithMessage("Fiscal year start month must be between 1 and 12.");

        RuleFor(x => x.BooksStartDate)
            .NotEmpty().WithMessage("Books start date is required.");

        RuleFor(x => x.RoundingPrecision)
            .InclusiveBetween((byte)0, (byte)4).WithMessage("Rounding precision must be between 0 and 4.");

        RuleFor(x => x.Notes)
            .MaximumLength(1000).WithMessage("Notes cannot exceed 1000 characters.")
            .When(x => !string.IsNullOrEmpty(x.Notes));
    }
}
