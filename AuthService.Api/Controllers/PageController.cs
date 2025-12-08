using AuthService.Application.Features.Page.CreatePage;
using AuthService.Application.Features.Page.DeletePage;
using AuthService.Application.Features.Page.GetAllPages;
using AuthService.Application.Features.Page.GetPage;
using AuthService.Application.Features.Page.UpdatePage;

namespace AuthService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PageController : ControllerBase
{
    private readonly IMediator _mediator;

    public PageController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    [Authorize(Roles = "SuperAdmin,FinanceAdmin")]
    public async Task<ActionResult<ApiResponse<PageDto>>> Create([FromBody] CreatePageCommand command)
    {
        try
        {
            var result = await _mediator.Send(command);
            return Ok(ApiResponse<PageDto>.SuccessResponse(result, "Page created successfully"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<PageDto>.FailResponse(ex.Message));
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<PageDto>>> Update(Guid id, [FromBody] UpdatePageCommand command)
    {
        try
        {
            if (id != command.Id)
            {
                return BadRequest(ApiResponse<PageDto>.FailResponse("ID mismatch"));
            }

            var result = await _mediator.Send(command);
            return Ok(ApiResponse<PageDto>.SuccessResponse(result, "Page updated successfully"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<PageDto>.FailResponse(ex.Message));
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(Guid id)
    {
        try
        {
            var command = new DeletePageCommand(id);
            var result = await _mediator.Send(command);
            return Ok(ApiResponse<bool>.SuccessResponse(result, "Page deleted successfully"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<bool>.FailResponse(ex.Message));
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<PageDto>>> Get(Guid id)
    {
        try
        {
            var query = new GetPageQuery(id);
            var result = await _mediator.Send(query);

            if (result == null)
            {
                return NotFound(ApiResponse<PageDto>.FailResponse("Page not found"));
            }

            return Ok(ApiResponse<PageDto>.SuccessResponse(result));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<PageDto>.FailResponse(ex.Message));
        }
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<PageDto>>>> GetAll()
    {
        try
        {
            var query = new GetAllPagesQuery();
            var result = await _mediator.Send(query);
            return Ok(ApiResponse<List<PageDto>>.SuccessResponse(result));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<List<PageDto>>.FailResponse(ex.Message));
        }
    }
}