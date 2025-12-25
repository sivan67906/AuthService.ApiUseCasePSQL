using AuthService.Application.Features.Admin.GetAdminStats;

namespace AuthService.Api.Controllers;

[ApiController]
[Route("api/auth/admin")]
[Authorize(Policy = "RequireAdmin")]
public class AdminController : ControllerBase
{
    private readonly IMediator _mediator;

    public AdminController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("stats")]
    public async Task<ActionResult<ApiResponse<AdminStatsDto>>> GetStats()
    {
        try
        {
            var result = await _mediator.Send(new GetAdminStatsQuery());
            return Ok(ApiResponse<AdminStatsDto>.SuccessResponse(result, "Admin stats retrieved successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<AdminStatsDto>.FailFromException("Failed to retrieve admin stats", ex));
        }
    }
}
