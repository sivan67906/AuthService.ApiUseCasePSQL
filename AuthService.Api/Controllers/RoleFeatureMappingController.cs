using AuthService.Application.DTOs;
using AuthService.Application.Features.RoleFeatureMapping.CreateRoleFeatureMapping;
using AuthService.Application.Features.RoleFeatureMapping.DeleteRoleFeatureMapping;
using AuthService.Application.Features.RoleFeatureMapping.GetAllRoleFeatureMappings;
using AuthService.Application.Features.RoleFeatureMapping.GetRoleFeatureMappingById;
using AuthService.Application.Features.RoleFeatureMapping.GetRoleFeatureMappingsByDepartment;
using AuthService.Application.Features.RoleFeatureMapping.GetRoleFeatureMappingsByRole;
using AuthService.Application.Features.RoleFeatureMapping.UpdateRoleFeatureMapping;

namespace AuthService.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class RoleFeatureMappingController : ControllerBase
{
    private readonly IMediator _mediator;

    public RoleFeatureMappingController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<RoleFeatureMappingDto>>>> GetAll()
    {
        try
        {
            var result = await _mediator.Send(new GetAllRoleFeatureMappingsQuery());
            return Ok(ApiResponse<List<RoleFeatureMappingDto>>.SuccessResponse(result, "Role feature mappings retrieved successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<List<RoleFeatureMappingDto>>.FailResponse("Internal server error", new() { ex.Message }));
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<RoleFeatureMappingDto>>> GetById(Guid id)
    {
        try
        {
            var result = await _mediator.Send(new GetRoleFeatureMappingByIdQuery(id));
            return Ok(ApiResponse<RoleFeatureMappingDto>.SuccessResponse(result, "Role feature mapping retrieved successfully"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<RoleFeatureMappingDto>.FailResponse(ex.Message));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<RoleFeatureMappingDto>.FailResponse("Internal server error", new() { ex.Message }));
        }
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<RoleFeatureMappingDto>>> Create([FromBody] CreateRoleFeatureMappingDto dto)
    {
        try
        {
            var command = new CreateRoleFeatureMappingCommand(
                dto.RoleId,
                dto.FeatureId,
                dto.DepartmentId,
                dto.IsActive
            );
            var result = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, ApiResponse<RoleFeatureMappingDto>.SuccessResponse(result, "Role feature mapping created successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<RoleFeatureMappingDto>.FailResponse("Internal server error", new() { ex.Message }));
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<RoleFeatureMappingDto>>> Update(Guid id, [FromBody] UpdateRoleFeatureMappingDto dto)
    {
        try
        {
            if (id != dto.Id)
            {
                return BadRequest(ApiResponse<RoleFeatureMappingDto>.FailResponse("ID mismatch"));
            }

            var command = new UpdateRoleFeatureMappingCommand(
                dto.Id,
                dto.RoleId,
                dto.FeatureId,
                dto.DepartmentId,
                dto.IsActive
            );
            var result = await _mediator.Send(command);
            return Ok(ApiResponse<RoleFeatureMappingDto>.SuccessResponse(result, "Role feature mapping updated successfully"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<RoleFeatureMappingDto>.FailResponse(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<RoleFeatureMappingDto>.FailResponse(ex.Message));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<RoleFeatureMappingDto>.FailResponse("Internal server error", new() { ex.Message }));
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(Guid id)
    {
        try
        {
            var result = await _mediator.Send(new DeleteRoleFeatureMappingCommand(id));
            return Ok(ApiResponse<bool>.SuccessResponse(result, "Role feature mapping deleted successfully"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<bool>.FailResponse(ex.Message, new() { ex.Message }));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<bool>.FailResponse("Internal server error", new() { ex.Message }));
        }
    }

    [HttpGet("by-department/{departmentId}")]
    public async Task<ActionResult<ApiResponse<List<RoleFeatureMappingDto>>>> GetByDepartment(Guid departmentId)
    {
        try
        {
            var result = await _mediator.Send(new GetRoleFeatureMappingsByDepartmentQuery(departmentId));
            return Ok(ApiResponse<List<RoleFeatureMappingDto>>.SuccessResponse(result, "Role feature mappings retrieved successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<List<RoleFeatureMappingDto>>.FailResponse("Internal server error", new() { ex.Message }));
        }
    }

    [HttpGet("by-role/{roleId}")]
    public async Task<ActionResult<ApiResponse<List<RoleFeatureMappingDto>>>> GetByRole(Guid roleId)
    {
        try
        {
            var result = await _mediator.Send(new GetRoleFeatureMappingsByRoleQuery(roleId));
            return Ok(ApiResponse<List<RoleFeatureMappingDto>>.SuccessResponse(result, "Role feature mappings retrieved successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<List<RoleFeatureMappingDto>>.FailResponse("Internal server error", new() { ex.Message }));
        }
    }
}
