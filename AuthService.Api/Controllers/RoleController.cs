using AuthService.Application.Features.Role.CreateRole;
using AuthService.Application.Features.Role.GetAllRoles;
using AuthService.Application.Features.Role.UpdateRole;
using AuthService.Application.Features.Role.DeleteRole;
using AuthService.Application.Features.Role.GetRoleById;
using AuthService.Domain.Constants;

namespace AuthService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RoleController : ControllerBase
{
    private readonly IMediator _mediator;

    public RoleController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    [Authorize(Roles = SystemRoles.SuperAdmin)]
    public async Task<ActionResult<ApiResponse<RoleDto>>> Create([FromBody] CreateRoleCommand command)
    {
        try
        {
            var result = await _mediator.Send(command);
            return Ok(ApiResponse<RoleDto>.SuccessResponse(result, "Role created successfully"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<RoleDto>.FailResponse(ex.Message));
        }
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<RoleDto>>>> GetAll()
    {
        try
        {
            var query = new GetAllRolesQuery();
            var result = await _mediator.Send(query);
            return Ok(ApiResponse<List<RoleDto>>.SuccessResponse(result));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<List<RoleDto>>.FailResponse(ex.Message));
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<RoleDto>>> GetById(Guid id)
    {
        try
        {
            var query = new GetRoleByIdQuery(id);
            var result = await _mediator.Send(query);

            if (result == null)
            {
                return NotFound(ApiResponse<RoleDto>.FailResponse("Role not found"));
            }

            return Ok(ApiResponse<RoleDto>.SuccessResponse(result));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<RoleDto>.FailResponse(ex.Message));
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<RoleDto>>> Update(Guid id, [FromBody] UpdateRoleCommand command)
    {
        try
        {
            if (id != command.RoleId)
            {
                return BadRequest(ApiResponse<RoleDto>.FailResponse("Role ID mismatch"));
            }

            var result = await _mediator.Send(command);
            return Ok(ApiResponse<RoleDto>.SuccessResponse(result, "Role updated successfully"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<RoleDto>.FailResponse(ex.Message));
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(Guid id)
    {
        try
        {
            var command = new DeleteRoleCommand(id);
            var result = await _mediator.Send(command);
            return Ok(ApiResponse<bool>.SuccessResponse(result, "Role deleted successfully"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<bool>.FailResponse(ex.Message));
        }
    }
}