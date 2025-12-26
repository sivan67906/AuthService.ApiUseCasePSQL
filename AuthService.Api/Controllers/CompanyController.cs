using AuthService.Application.Features.Company.CreateCompany;
using AuthService.Application.Features.Company.DeleteCompany;
using AuthService.Application.Features.Company.GetAllCompanies;
using AuthService.Application.Features.Company.GetCompany;
using AuthService.Application.Features.Company.UpdateCompany;
using AuthService.Application.Features.Lookups;

namespace AuthService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CompanyController : ControllerBase
{
    private readonly IMediator _mediator;

    public CompanyController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get all companies
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<CompanyDto>>>> GetAll()
    {
        try
        {
            var result = await _mediator.Send(new GetAllCompaniesQuery());
            return Ok(ApiResponse<List<CompanyDto>>.SuccessResponse(result));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<List<CompanyDto>>.FailResponse(ex.Message));
        }
    }

    /// <summary>
    /// Get company by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<CompanyDto>>> Get(Guid id)
    {
        try
        {
            var result = await _mediator.Send(new GetCompanyQuery(id));
            if (result == null)
            {
                return NotFound(ApiResponse<CompanyDto>.FailResponse("Company not found"));
            }
            return Ok(ApiResponse<CompanyDto>.SuccessResponse(result));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<CompanyDto>.FailResponse(ex.Message));
        }
    }

    /// <summary>
    /// Create a new company
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<CompanyDto>>> Create([FromBody] CreateCompanyCommand command)
    {
        try
        {
            var result = await _mediator.Send(command);
            return Ok(ApiResponse<CompanyDto>.SuccessResponse(result, "Company created successfully"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<CompanyDto>.FailResponse(ex.Message));
        }
    }

    /// <summary>
    /// Update an existing company
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<CompanyDto>>> Update(Guid id, [FromBody] UpdateCompanyCommand command)
    {
        try
        {
            Console.WriteLine($"[CompanyController] Received Update request for Company {id}");
            Console.WriteLine($"[CompanyController] Command.Id: {command.Id}");
            Console.WriteLine($"[CompanyController] Command.LegalName: {command.LegalName}");
            Console.WriteLine($"[CompanyController] Command.Status: {command.Status}");
            Console.WriteLine($"[CompanyController] Command.BooksStartDate: {command.BooksStartDate:yyyy-MM-dd}");
            Console.WriteLine($"[CompanyController] Command.CityId: {command.CityId}");
            Console.WriteLine($"[CompanyController] Command.StateId: {command.StateId}");
            Console.WriteLine($"[CompanyController] Command.CountryId: {command.CountryId}");

            if (id != command.Id)
            {
                Console.WriteLine($"[CompanyController] ID mismatch: URL={id}, Body={command.Id}");
                return BadRequest(ApiResponse<CompanyDto>.FailResponse("ID mismatch"));
            }

            var result = await _mediator.Send(command);
            Console.WriteLine($"[CompanyController] Update successful for Company {id}");
            return Ok(ApiResponse<CompanyDto>.SuccessResponse(result, "Company updated successfully"));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[CompanyController] Update failed for Company {id}: {ex.Message}");
            return BadRequest(ApiResponse<CompanyDto>.FailResponse(ex.Message));
        }
    }

    /// <summary>
    /// Delete a company (soft delete)
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(Guid id)
    {
        try
        {
            var result = await _mediator.Send(new DeleteCompanyCommand(id));
            return Ok(ApiResponse<bool>.SuccessResponse(result, "Company deleted successfully"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<bool>.FailResponse(ex.Message));
        }
    }

    // ========================
    // Lookup Endpoints
    // ========================

    /// <summary>
    /// Get all countries for dropdown
    /// </summary>
    [HttpGet("lookups/countries")]
    public async Task<ActionResult<ApiResponse<List<CountryLookupDto>>>> GetCountries()
    {
        try
        {
            var result = await _mediator.Send(new GetCountriesQuery());
            return Ok(ApiResponse<List<CountryLookupDto>>.SuccessResponse(result));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<List<CountryLookupDto>>.FailResponse(ex.Message));
        }
    }

    /// <summary>
    /// Get states by country for cascade dropdown
    /// </summary>
    [HttpGet("lookups/states/{countryId}")]
    public async Task<ActionResult<ApiResponse<List<StateLookupDto>>>> GetStatesByCountry(Guid countryId)
    {
        try
        {
            var result = await _mediator.Send(new GetStatesByCountryQuery(countryId));
            return Ok(ApiResponse<List<StateLookupDto>>.SuccessResponse(result));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<List<StateLookupDto>>.FailResponse(ex.Message));
        }
    }

    /// <summary>
    /// Get cities by state for cascade dropdown
    /// </summary>
    [HttpGet("lookups/cities/{stateId}")]
    public async Task<ActionResult<ApiResponse<List<CityLookupDto>>>> GetCitiesByState(Guid stateId)
    {
        try
        {
            var result = await _mediator.Send(new GetCitiesByStateQuery(stateId));
            return Ok(ApiResponse<List<CityLookupDto>>.SuccessResponse(result));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<List<CityLookupDto>>.FailResponse(ex.Message));
        }
    }

    /// <summary>
    /// Get all currencies for dropdown
    /// </summary>
    [HttpGet("lookups/currencies")]
    public async Task<ActionResult<ApiResponse<List<CurrencyLookupDto>>>> GetCurrencies()
    {
        try
        {
            var result = await _mediator.Send(new GetCurrenciesQuery());
            return Ok(ApiResponse<List<CurrencyLookupDto>>.SuccessResponse(result));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<List<CurrencyLookupDto>>.FailResponse(ex.Message));
        }
    }

    /// <summary>
    /// Get all timezones for dropdown
    /// </summary>
    [HttpGet("lookups/timezones")]
    public async Task<ActionResult<ApiResponse<List<TimeZoneLookupDto>>>> GetTimeZones()
    {
        try
        {
            var result = await _mediator.Send(new GetTimeZonesQuery());
            return Ok(ApiResponse<List<TimeZoneLookupDto>>.SuccessResponse(result));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<List<TimeZoneLookupDto>>.FailResponse(ex.Message));
        }
    }

    /// <summary>
    /// Get timezones filtered by country
    /// </summary>
    [HttpGet("timezones-by-country/{countryId}")]
    public async Task<ActionResult<ApiResponse<List<TimeZoneLookupDto>>>> GetTimezonesByCountry(Guid countryId)
    {
        try
        {
            var result = await _mediator.Send(new GetTimezonesByCountryQuery(countryId));
            return Ok(ApiResponse<List<TimeZoneLookupDto>>.SuccessResponse(result));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<List<TimeZoneLookupDto>>.FailResponse(ex.Message));
        }
    }

    /// <summary>
    /// Get all companies for parent company dropdown
    /// </summary>
    [HttpGet("lookups/companies")]
    public async Task<ActionResult<ApiResponse<List<CompanyLookupDto>>>> GetCompaniesLookup()
    {
        try
        {
            var result = await _mediator.Send(new GetCompaniesLookupQuery());
            return Ok(ApiResponse<List<CompanyLookupDto>>.SuccessResponse(result));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<List<CompanyLookupDto>>.FailResponse(ex.Message));
        }
    }
}
