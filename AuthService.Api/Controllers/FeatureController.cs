using AuthService.Application.Features.Feature.CreateFeature;
using AuthService.Application.Features.Feature.DeleteFeature;
using AuthService.Application.Features.Feature.GetAllFeatures;
using AuthService.Application.Features.Feature.GetFeature;
using AuthService.Application.Features.Feature.UpdateFeature;

namespace AuthService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FeatureController : ControllerBase
{
    private readonly IMediator _mediator;

    public FeatureController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    [Authorize(Roles = "SuperAdmin,FinanceAdmin")]
    public async Task<ActionResult<ApiResponse<FeatureDto>>> Create([FromBody] CreateFeatureCommand command)
    {
        try
        {
            var result = await _mediator.Send(command);
            return Ok(ApiResponse<FeatureDto>.SuccessResponse(result, "Feature created successfully"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<FeatureDto>.FailResponse(ex.Message));
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<FeatureDto>>> Update(Guid id, [FromBody] UpdateFeatureCommand command)
    {
        try
        {
            if (id != command.Id)
            {
                return BadRequest(ApiResponse<FeatureDto>.FailResponse("ID mismatch"));
            }

            var result = await _mediator.Send(command);
            return Ok(ApiResponse<FeatureDto>.SuccessResponse(result, "Feature updated successfully"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<FeatureDto>.FailResponse(ex.Message));
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(Guid id)
    {
        try
        {
            var command = new DeleteFeatureCommand(id);
            var result = await _mediator.Send(command);
            return Ok(ApiResponse<bool>.SuccessResponse(result, "Feature deleted successfully"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<bool>.FailResponse(ex.Message));
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<FeatureDto>>> Get(Guid id)
    {
        try
        {
            var query = new GetFeatureQuery(id);
            var result = await _mediator.Send(query);

            if (result == null)
            {
                return NotFound(ApiResponse<FeatureDto>.FailResponse("Feature not found"));
            }

            return Ok(ApiResponse<FeatureDto>.SuccessResponse(result));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<FeatureDto>.FailResponse(ex.Message));
        }
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<FeatureDto>>>> GetAll()
    {
        try
        {
            var query = new GetAllFeaturesQuery();
            var result = await _mediator.Send(query);
            return Ok(ApiResponse<List<FeatureDto>>.SuccessResponse(result));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<List<FeatureDto>>.FailResponse(ex.Message));
        }
    }
}