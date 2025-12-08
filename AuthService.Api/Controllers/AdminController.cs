namespace AuthService.Api.Controllers;

[ApiController]
[Route("api/auth/admin")]
[Authorize(Policy = "RequireAdmin")]
public class AdminController : ControllerBase
{
    [HttpGet("stats")]
    public ActionResult<ApiResponse<object>> GetStats()
    {
        var payload = new
        {
            UsersOnline = 0,
            GeneratedAtUtc = DateTime.UtcNow
        };
        return Ok(ApiResponse<object>.SuccessResponse(payload, "Admin stats"));
    }
}
