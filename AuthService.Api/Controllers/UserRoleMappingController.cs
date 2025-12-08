using AuthService.Application.Features.UserRoleMapping;
using AuthService.Application.Features.UserRoleMapping.CreateUserRoleMapping;
using AuthService.Application.Features.UserRoleMapping.GetAllUserRoleMappings;
using AuthService.Application.Features.UserRoleMapping.GetUserRoleMappingById;
using AuthService.Application.Features.UserRoleMapping.UpdateUserRoleMapping;
using AuthService.Application.Features.UserRoleMapping.DeleteUserRoleMapping;
using AuthService.Application.Features.UserRoleMapping.GetUsersWithoutRoles;
using AuthService.Domain.Constants;

namespace AuthService.Api.Controllers;

[ApiController]
[Route("api/auth/[controller]")]
[Authorize]
public class UserRoleMappingController : ControllerBase
{
    private readonly IMediator _mediator;

    public UserRoleMappingController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    [Authorize(Roles = SystemRoles.SuperAdmin)]
    public async Task<ActionResult<ApiResponse<UserRoleMappingDto>>> Create([FromBody] CreateUserRoleMappingCommand command)
    {
        try
        {
            var result = await _mediator.Send(command);
            return Ok(ApiResponse<UserRoleMappingDto>.SuccessResponse(result, "User role mapping created successfully"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<UserRoleMappingDto>.FailResponse(ex.Message));
        }
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<UserRoleMappingDto>>>> GetAll()
    {
        try
        {
            var query = new GetAllUserRoleMappingsQuery();
            var result = await _mediator.Send(query);
            return Ok(ApiResponse<List<UserRoleMappingDto>>.SuccessResponse(result));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<List<UserRoleMappingDto>>.FailResponse(ex.Message));
        }
    }

    [HttpGet("users-without-roles")]
    public async Task<ActionResult<ApiResponse<List<UserWithoutRoleDto>>>> GetUsersWithoutRoles()
    {
        try
        {
            var query = new GetUsersWithoutRolesQuery();
            var result = await _mediator.Send(query);
            return Ok(ApiResponse<List<UserWithoutRoleDto>>.SuccessResponse(result));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<List<UserWithoutRoleDto>>.FailResponse(ex.Message));
        }
    }

    [HttpGet("users-without-roles-immediate")]
    public async Task<ActionResult<ApiResponse<List<UserWithoutRoleDto>>>> GetUsersWithoutRolesImmediate()
    {
        try
        {
            var query = new GetUsersWithoutRolesFromCommandDbQuery();
            var result = await _mediator.Send(query);
            return Ok(ApiResponse<List<UserWithoutRoleDto>>.SuccessResponse(result));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<List<UserWithoutRoleDto>>.FailResponse(ex.Message));
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<UserRoleMappingDto>>> GetById(Guid id)
    {
        try
        {
            var query = new GetUserRoleMappingByIdQuery(id);
            var result = await _mediator.Send(query);

            if (result == null)
            {
                return NotFound(ApiResponse<UserRoleMappingDto>.FailResponse("User role mapping not found"));
            }

            return Ok(ApiResponse<UserRoleMappingDto>.SuccessResponse(result));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<UserRoleMappingDto>.FailResponse(ex.Message));
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<UserRoleMappingDto>>> Update(Guid id, [FromBody] UpdateUserRoleMappingCommand command)
    {
        try
        {
            if (id != command.Id)
            {
                return BadRequest(ApiResponse<UserRoleMappingDto>.FailResponse("ID mismatch"));
            }

            var result = await _mediator.Send(command);
            return Ok(ApiResponse<UserRoleMappingDto>.SuccessResponse(result, "User role mapping updated successfully"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<UserRoleMappingDto>.FailResponse(ex.Message));
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(Guid id)
    {
        try
        {
            var command = new DeleteUserRoleMappingCommand(id);
            var result = await _mediator.Send(command);
            return Ok(ApiResponse<bool>.SuccessResponse(result, "User role mapping deleted successfully"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<bool>.FailResponse(ex.Message));
        }
    }
}