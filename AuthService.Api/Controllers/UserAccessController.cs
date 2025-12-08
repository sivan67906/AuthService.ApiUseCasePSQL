using AuthService.Application.Features.UserAccess.AssignRole;
using AuthService.Application.Features.UserAccess.GetAllUsers;
using AuthService.Application.Features.UserAccess.GetUserAccess;
using AuthService.Application.Features.UserAccess.GetUserAccessByEmail;
using AuthService.Application.Features.UserAccess.GetUserPages;
using AuthService.Application.Features.UserAccess.GetUserNavigation;
using AuthService.Domain.Constants;
using System.Security.Claims;

namespace AuthService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UserAccessController : ControllerBase
{
    private readonly IMediator _mediator;

    public UserAccessController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("users")]
    public async Task<ActionResult<ApiResponse<List<UserDto>>>> GetAllUsers()
    {
        try
        {
            var query = new GetAllUsersQuery();
            var result = await _mediator.Send(query);
            return Ok(ApiResponse<List<UserDto>>.SuccessResponse(result));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<List<UserDto>>.FailResponse(ex.Message));
        }
    }

    [HttpPost("assign-role")]
    [Authorize(Roles = SystemRoles.SuperAdmin + "," + Roles.FinanceAdmin)]
    public async Task<ActionResult<ApiResponse<bool>>> AssignRole([FromBody] AssignRoleCommand command)
    {
        try
        {
            var result = await _mediator.Send(command);
            return Ok(ApiResponse<bool>.SuccessResponse(result, "Role assigned successfully"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<bool>.FailResponse(ex.Message));
        }
    }

    [HttpGet("{userId}")]
    public async Task<ActionResult<ApiResponse<UserAccessDto>>> GetUserAccess(Guid userId)
    {
        try
        {
            var query = new GetUserAccessQuery(userId);
            var result = await _mediator.Send(query);
            return Ok(ApiResponse<UserAccessDto>.SuccessResponse(result));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<UserAccessDto>.FailResponse(ex.Message));
        }
    }

    [HttpGet("pages")]
    public async Task<ActionResult<ApiResponse<List<UserPageDto>>>> GetUserPages()
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(ApiResponse<List<UserPageDto>>.FailResponse("User not authenticated"));
            }

            var query = new GetUserPagesQuery(userId);
            var result = await _mediator.Send(query);
            return Ok(ApiResponse<List<UserPageDto>>.SuccessResponse(result));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<List<UserPageDto>>.FailResponse(ex.Message));
        }
    }

    [HttpGet("pages/{userId}")]
    public async Task<ActionResult<ApiResponse<List<UserPageDto>>>> GetUserPagesById(string userId)
    {
        try
        {
            var query = new GetUserPagesQuery(userId);
            var result = await _mediator.Send(query);
            return Ok(ApiResponse<List<UserPageDto>>.SuccessResponse(result));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<List<UserPageDto>>.FailResponse(ex.Message));
        }
    }

    [HttpGet("by-email/{email}")]
    public async Task<ActionResult<ApiResponse<UserAccessDto>>> GetUserAccessByEmail(string email)
    {
        try
        {
            var query = new GetUserAccessByEmailQuery(email);
            var result = await _mediator.Send(query);

            if (result == null)
            {
                return NotFound(ApiResponse<UserAccessDto>.FailResponse($"User with email '{email}' not found"));
            }

            return Ok(ApiResponse<UserAccessDto>.SuccessResponse(result));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<UserAccessDto>.FailResponse(ex.Message));
        }
    }

    [HttpGet("navigation")]
    public async Task<ActionResult<ApiResponse<UserNavigationDto>>> GetUserNavigation()
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(ApiResponse<UserNavigationDto>.FailResponse("User not authenticated"));
            }

            var query = new GetUserNavigationQuery(userId);
            var result = await _mediator.Send(query);
            return Ok(ApiResponse<UserNavigationDto>.SuccessResponse(result));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<UserNavigationDto>.FailResponse(ex.Message));
        }
    }
}