using AuthService.Application.Features.RoleHierarchyMapping;
using AuthService.Application.Features.RoleHierarchyMapping.CreateRoleHierarchyMapping;
using AuthService.Application.Features.RoleHierarchyMapping.GetAllRoleHierarchyMappings;
using AuthService.Application.Features.RoleHierarchyMapping.GetRoleHierarchyMappingById;
using AuthService.Application.Features.RoleHierarchyMapping.UpdateRoleHierarchyMapping;
using AuthService.Application.Features.RoleHierarchyMapping.DeleteRoleHierarchyMapping;
using AuthService.Domain.Constants;

namespace AuthService.Api.Controllers;

[ApiController]
[Route("api/auth/[controller]")]
[Authorize]
public class RoleHierarchyMappingController : ControllerBase
{
    private readonly IMediator _mediator;

    public RoleHierarchyMappingController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    [Authorize(Roles = SystemRoles.SuperAdmin)]
    public async Task<ActionResult<ApiResponse<RoleHierarchyMappingDto>>> Create([FromBody] CreateRoleHierarchyMappingCommand command)
    {
        try
        {
            var result = await _mediator.Send(command);
            return Ok(ApiResponse<RoleHierarchyMappingDto>.SuccessResponse(result, "Role hierarchy mapping created successfully"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<RoleHierarchyMappingDto>.FailResponse(ex.Message));
        }
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<RoleHierarchyMappingDto>>>> GetAll()
    {
        try
        {
            var query = new GetAllRoleHierarchyMappingsQuery();
            var result = await _mediator.Send(query);
            return Ok(ApiResponse<List<RoleHierarchyMappingDto>>.SuccessResponse(result));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<List<RoleHierarchyMappingDto>>.FailResponse(ex.Message));
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<RoleHierarchyMappingDto>>> GetById(Guid id)
    {
        try
        {
            var query = new GetRoleHierarchyMappingByIdQuery(id);
            var result = await _mediator.Send(query);

            if (result == null)
            {
                return NotFound(ApiResponse<RoleHierarchyMappingDto>.FailResponse("Role hierarchy mapping not found"));
            }

            return Ok(ApiResponse<RoleHierarchyMappingDto>.SuccessResponse(result));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<RoleHierarchyMappingDto>.FailResponse(ex.Message));
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<RoleHierarchyMappingDto>>> Update(Guid id, [FromBody] UpdateRoleHierarchyMappingCommand command)
    {
        if (id != command.Id)
        {
            return BadRequest(ApiResponse<RoleHierarchyMappingDto>.FailResponse("ID mismatch"));
        }

        try
        {
            var result = await _mediator.Send(command);
            return Ok(ApiResponse<RoleHierarchyMappingDto>.SuccessResponse(result, "Role hierarchy mapping updated successfully"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<RoleHierarchyMappingDto>.FailResponse(ex.Message));
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(Guid id)
    {
        try
        {
            var command = new DeleteRoleHierarchyMappingCommand(id);
            var result = await _mediator.Send(command);
            return Ok(ApiResponse<bool>.SuccessResponse(result, "Role hierarchy mapping deleted successfully"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<bool>.FailResponse(ex.Message));
        }
    }
}