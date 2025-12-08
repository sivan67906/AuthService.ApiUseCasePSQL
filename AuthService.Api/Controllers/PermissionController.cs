using AuthService.Application.Features.Permission.CreatePermission;
using AuthService.Application.Features.Permission.DeletePermission;
using AuthService.Application.Features.Permission.GetAllPermissions;
using AuthService.Application.Features.Permission.GetPermission;
using AuthService.Application.Features.Permission.UpdatePermission;

namespace AuthService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PermissionController : ControllerBase
{
    private readonly IMediator _mediator;

    public PermissionController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    [Authorize(Roles = "SuperAdmin,FinanceAdmin")]
    public async Task<ActionResult<ApiResponse<PermissionDto>>> Create([FromBody] CreatePermissionCommand command)
    {
        try
        {
            var result = await _mediator.Send(command);
            return Ok(ApiResponse<PermissionDto>.SuccessResponse(result, "Permission created successfully"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<PermissionDto>.FailResponse(ex.Message));
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<PermissionDto>>> Update(Guid id, [FromBody] UpdatePermissionCommand command)
    {
        if (id != command.Id)
        {
            return BadRequest(ApiResponse<PermissionDto>.FailResponse("ID mismatch"));
        }

        try
        {
            var result = await _mediator.Send(command);
            return Ok(ApiResponse<PermissionDto>.SuccessResponse(result, "Permission updated successfully"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<PermissionDto>.FailResponse(ex.Message));
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(Guid id)
    {
        try
        {
            var command = new DeletePermissionCommand(id);
            var result = await _mediator.Send(command);
            return Ok(ApiResponse<bool>.SuccessResponse(result, "Permission deleted successfully"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<bool>.FailResponse(ex.Message));
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<PermissionDto>>> Get(Guid id)
    {
        try
        {
            var query = new GetPermissionQuery(id);
            var result = await _mediator.Send(query);

            if (result == null)
            {
                return NotFound(ApiResponse<PermissionDto>.FailResponse("Permission not found"));
            }

            return Ok(ApiResponse<PermissionDto>.SuccessResponse(result));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<PermissionDto>.FailResponse(ex.Message));
        }
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<PermissionDto>>>> GetAll()
    {
        try
        {
            var query = new GetAllPermissionsQuery();
            var result = await _mediator.Send(query);
            return Ok(ApiResponse<List<PermissionDto>>.SuccessResponse(result));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<List<PermissionDto>>.FailResponse(ex.Message));
        }
    }
}