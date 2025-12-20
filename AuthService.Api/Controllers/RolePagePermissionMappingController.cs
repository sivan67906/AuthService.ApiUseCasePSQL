using AuthService.Application.DTOs;
using AuthService.Application.Features.RolePagePermissionMapping.CreateRolePagePermissionMapping;
using AuthService.Application.Features.RolePagePermissionMapping.DeleteRolePagePermissionMapping;
using AuthService.Application.Features.RolePagePermissionMapping.GetAllRolePagePermissionMappings;
using AuthService.Application.Features.RolePagePermissionMapping.GetRolePagePermissionMappingById;
using AuthService.Application.Features.RolePagePermissionMapping.GetRolePagePermissionMappingsByDepartment;
using AuthService.Application.Features.RolePagePermissionMapping.GetRolePagePermissionMappingsByRole;
using AuthService.Application.Features.RolePagePermissionMapping.GetRolePagePermissionMappingsByRoleAndPage;
using AuthService.Application.Features.RolePagePermissionMapping.UpdateRolePagePermissionMapping;
using AuthService.Application.Features.RolePagePermissionMapping.GetGroupedRolePagePermissions;
using AuthService.Application.Features.RolePagePermissionMapping.CreateOrUpdateBatch;

namespace AuthService.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class RolePagePermissionMappingController : ControllerBase
{
    private readonly IMediator _mediator;

    public RolePagePermissionMappingController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<RolePagePermissionMappingDto>>>> GetAll()
    {
        try
        {
            var result = await _mediator.Send(new GetAllRolePagePermissionMappingsQuery());
            return Ok(ApiResponse<List<RolePagePermissionMappingDto>>.SuccessResponse(result, "Role page permission mappings retrieved successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<List<RolePagePermissionMappingDto>>.FailFromException("Internal server error", ex));
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<RolePagePermissionMappingDto>>> GetById(Guid id)
    {
        try
        {
            var result = await _mediator.Send(new GetRolePagePermissionMappingByIdQuery(id));
            return Ok(ApiResponse<RolePagePermissionMappingDto>.SuccessResponse(result, "Role page permission mapping retrieved successfully"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<RolePagePermissionMappingDto>.FailResponse(ex.Message));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<RolePagePermissionMappingDto>.FailFromException("Internal server error", ex));
        }
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<RolePagePermissionMappingDto>>> Create([FromBody] CreateRolePagePermissionMappingDto dto)
    {
        try
        {
            var command = new CreateRolePagePermissionMappingCommand(
                dto.RoleId,
                dto.PageId,
                dto.PermissionId,
                dto.DepartmentId,
                dto.IsActive
            );
            var result = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, ApiResponse<RolePagePermissionMappingDto>.SuccessResponse(result, "Role page permission mapping created successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<RolePagePermissionMappingDto>.FailFromException("Internal server error", ex));
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<RolePagePermissionMappingDto>>> Update(Guid id, [FromBody] UpdateRolePagePermissionMappingDto dto)
    {
        try
        {
            if (id != dto.Id)
            {
                return BadRequest(ApiResponse<RolePagePermissionMappingDto>.FailResponse("ID mismatch"));
            }

            var command = new UpdateRolePagePermissionMappingCommand(
                dto.Id,
                dto.RoleId,
                dto.PageId,
                dto.PermissionId,
                dto.DepartmentId,
                dto.IsActive
            );
            var result = await _mediator.Send(command);
            return Ok(ApiResponse<RolePagePermissionMappingDto>.SuccessResponse(result, "Role page permission mapping updated successfully"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<RolePagePermissionMappingDto>.FailResponse(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<RolePagePermissionMappingDto>.FailResponse(ex.Message));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<RolePagePermissionMappingDto>.FailFromException("Internal server error", ex));
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(Guid id)
    {
        try
        {
            var result = await _mediator.Send(new DeleteRolePagePermissionMappingCommand(id));
            return Ok(ApiResponse<bool>.SuccessResponse(result, "Role page permission mapping deleted successfully"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<bool>.FailFromException(ex.Message, ex));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<bool>.FailFromException(ex.Message, ex));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<bool>.FailFromException("Internal server error", ex));
        }
    }

    [HttpGet("by-department/{departmentId}")]
    public async Task<ActionResult<ApiResponse<List<RolePagePermissionMappingDto>>>> GetByDepartment(Guid departmentId)
    {
        try
        {
            var result = await _mediator.Send(new GetRolePagePermissionMappingsByDepartmentQuery(departmentId));
            return Ok(ApiResponse<List<RolePagePermissionMappingDto>>.SuccessResponse(result, "Role page permission mappings retrieved successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<List<RolePagePermissionMappingDto>>.FailFromException("Internal server error", ex));
        }
    }

    [HttpGet("by-role/{roleId}")]
    public async Task<ActionResult<ApiResponse<List<RolePagePermissionMappingDto>>>> GetByRole(Guid roleId)
    {
        try
        {
            var result = await _mediator.Send(new GetRolePagePermissionMappingsByRoleQuery(roleId));
            return Ok(ApiResponse<List<RolePagePermissionMappingDto>>.SuccessResponse(result, "Role page permission mappings retrieved successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<List<RolePagePermissionMappingDto>>.FailFromException("Internal server error", ex));
        }
    }

    [HttpGet("by-role-page/{roleId}/{pageId}")]
    public async Task<ActionResult<ApiResponse<List<RolePagePermissionMappingDto>>>> GetByRoleAndPage(Guid roleId, Guid pageId)
    {
        try
        {
            var result = await _mediator.Send(new GetRolePagePermissionMappingsByRoleAndPageQuery(roleId, pageId));
            return Ok(ApiResponse<List<RolePagePermissionMappingDto>>.SuccessResponse(result, "Role page permission mappings retrieved successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<List<RolePagePermissionMappingDto>>.FailFromException("Internal server error", ex));
        }
    }

    /// <summary>
    /// Get grouped role page permission mappings (Department-Role-Page with all permissions)
    /// </summary>
    [HttpGet("grouped")]
    public async Task<ActionResult<ApiResponse<List<RolePagePermissionGroupDto>>>> GetGrouped()
    {
        try
        {
            var result = await _mediator.Send(new GetGroupedRolePagePermissionsQuery());
            return Ok(ApiResponse<List<RolePagePermissionGroupDto>>.SuccessResponse(result, "Grouped role page permission mappings retrieved successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<List<RolePagePermissionGroupDto>>.FailFromException("Internal server error", ex));
        }
    }

    /// <summary>
    /// Create or update permissions in batch for a Department-Role-Page combination
    /// </summary>
    [HttpPost("batch")]
    public async Task<ActionResult<ApiResponse<List<RolePagePermissionMappingDto>>>> CreateOrUpdateBatch([FromBody] CreateOrUpdatePermissionBatchDto dto)
    {
        try
        {
            var command = new CreateOrUpdateRolePagePermissionBatchCommand(
                dto.DepartmentId,
                dto.RoleId,
                dto.PageId,
                dto.PermissionIds
            );
            var result = await _mediator.Send(command);
            return Ok(ApiResponse<List<RolePagePermissionMappingDto>>.SuccessResponse(result, "Permissions updated successfully"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<List<RolePagePermissionMappingDto>>.FailResponse(ex.Message));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<List<RolePagePermissionMappingDto>>.FailFromException("Internal server error", ex));
        }
    }
}
