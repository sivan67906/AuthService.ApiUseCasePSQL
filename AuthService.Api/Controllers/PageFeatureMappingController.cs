using AuthService.Application.Features.PageFeatureMapping.CreatePageFeatureMapping;
using AuthService.Application.Features.PageFeatureMapping.DeletePageFeatureMapping;
using AuthService.Application.Features.PageFeatureMapping.GetAllPageFeatureMappings;
using AuthService.Application.Features.PageFeatureMapping.GetPageFeatureMapping;
using AuthService.Application.Features.PageFeatureMapping.UpdatePageFeatureMapping;

namespace AuthService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PageFeatureMappingController : ControllerBase
{
    private readonly IMediator _mediator;

    public PageFeatureMappingController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    [Authorize(Roles = "SuperAdmin,FinanceAdmin")]
    public async Task<ActionResult<ApiResponse<PageFeatureMappingDto>>> Create([FromBody] CreatePageFeatureMappingCommand command)
    {
        try
        {
            var result = await _mediator.Send(command);
            return Ok(ApiResponse<PageFeatureMappingDto>.SuccessResponse(result, "PageFeatureMapping created successfully"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<PageFeatureMappingDto>.FailResponse(ex.Message));
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<PageFeatureMappingDto>>> Update(Guid id, [FromBody] UpdatePageFeatureMappingCommand command)
    {
        try
        {
            if (id != command.Id)
            {
                return BadRequest(ApiResponse<PageFeatureMappingDto>.FailResponse("ID mismatch"));
            }

            var result = await _mediator.Send(command);
            return Ok(ApiResponse<PageFeatureMappingDto>.SuccessResponse(result, "PageFeatureMapping updated successfully"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<PageFeatureMappingDto>.FailResponse(ex.Message));
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(Guid id)
    {
        try
        {
            var command = new DeletePageFeatureMappingCommand(id);
            var result = await _mediator.Send(command);
            return Ok(ApiResponse<bool>.SuccessResponse(result, "PageFeatureMapping deleted successfully"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<bool>.FailResponse(ex.Message));
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<PageFeatureMappingDto>>> Get(Guid id)
    {
        try
        {
            var query = new GetPageFeatureMappingQuery(id);
            var result = await _mediator.Send(query);

            if (result == null)
            {
                return NotFound(ApiResponse<PageFeatureMappingDto>.FailResponse("PageFeatureMapping not found"));
            }

            return Ok(ApiResponse<PageFeatureMappingDto>.SuccessResponse(result));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<PageFeatureMappingDto>.FailResponse(ex.Message));
        }
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<PageFeatureMappingDto>>>> GetAll()
    {
        try
        {
            var query = new GetAllPageFeatureMappingsQuery();
            var result = await _mediator.Send(query);
            return Ok(ApiResponse<List<PageFeatureMappingDto>>.SuccessResponse(result));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<List<PageFeatureMappingDto>>.FailResponse(ex.Message));
        }
    }
}