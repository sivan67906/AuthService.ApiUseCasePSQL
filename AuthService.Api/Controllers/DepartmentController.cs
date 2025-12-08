using AuthService.Application.Features.Department.CreateDepartment;
using AuthService.Application.Features.Department.DeleteDepartment;
using AuthService.Application.Features.Department.GetAllDepartments;
using AuthService.Application.Features.Department.GetDepartment;
using AuthService.Application.Features.Department.UpdateDepartment;
using AuthService.Domain.Constants;

namespace AuthService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DepartmentController : ControllerBase
{
    private readonly IMediator _mediator;

    public DepartmentController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    [Authorize(Roles = SystemRoles.SuperAdmin)]
    public async Task<ActionResult<ApiResponse<DepartmentDto>>> Create([FromBody] CreateDepartmentCommand command)
    {
        try
        {
            var result = await _mediator.Send(command);
            return Ok(ApiResponse<DepartmentDto>.SuccessResponse(result, "Department created successfully"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<DepartmentDto>.FailResponse(ex.Message));
        }
    }

    [HttpPut("{id}")]
    [Authorize(Roles = SystemRoles.SuperAdmin)]
    public async Task<ActionResult<ApiResponse<DepartmentDto>>> Update(Guid id, [FromBody] UpdateDepartmentCommand command)
    {
        try
        {
            if (id != command.Id)
            {
                return BadRequest(ApiResponse<DepartmentDto>.FailResponse("ID mismatch"));
            }

            var result = await _mediator.Send(command);
            return Ok(ApiResponse<DepartmentDto>.SuccessResponse(result, "Department updated successfully"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<DepartmentDto>.FailResponse(ex.Message));
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = SystemRoles.SuperAdmin)]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(Guid id)
    {
        try
        {
            var command = new DeleteDepartmentCommand(id);
            var result = await _mediator.Send(command);
            return Ok(ApiResponse<bool>.SuccessResponse(result, "Department deleted successfully"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<bool>.FailResponse(ex.Message));
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<DepartmentDto>>> Get(Guid id)
    {
        try
        {
            var query = new GetDepartmentQuery(id);
            var result = await _mediator.Send(query);

            if (result == null)
            {
                return NotFound(ApiResponse<DepartmentDto>.FailResponse("Department not found"));
            }

            return Ok(ApiResponse<DepartmentDto>.SuccessResponse(result));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<DepartmentDto>.FailResponse(ex.Message));
        }
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<DepartmentDto>>>> GetAll()
    {
        try
        {
            var query = new GetAllDepartmentsQuery();
            var result = await _mediator.Send(query);
            return Ok(ApiResponse<List<DepartmentDto>>.SuccessResponse(result));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<List<DepartmentDto>>.FailResponse(ex.Message));
        }
    }
}