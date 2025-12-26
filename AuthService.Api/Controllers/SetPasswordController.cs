using AuthService.Application.Features.SetPassword.SetPasswords;

namespace AuthService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SetPasswordController : ControllerBase
{
    private readonly IMediator _mediator;

    public SetPasswordController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<string>>> SetPasswords([FromBody] List<string> emails)
    {
        try
        {
            var result = await _mediator.Send(new SetPasswordsCommand(emails));
            return Ok(ApiResponse<string>.SuccessResponse(result, "Passwords updated successfully"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<string>.FailFromException("Failed to update passwords", ex));
        }
    }
}
