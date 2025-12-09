using System.Security.Claims;
using AuthService.Application.Features.Auth.Authenticator;
using AuthService.Application.Features.Auth.ChangePassword;
using AuthService.Application.Features.Auth.EmailConfirmation;
using AuthService.Application.Features.Auth.ExternalLogin;
using AuthService.Application.Features.Auth.ForgotPassword;
using AuthService.Application.Features.Auth.Login;
using AuthService.Application.Features.Auth.RefreshToken;
using AuthService.Application.Features.Auth.Register;
using AuthService.Application.Features.Auth.ResetPassword;
using AuthService.Application.Features.Auth.RevokeToken;
using AuthService.Application.Features.Auth.TwoFactor;
using AuthService.Application.Features.Profile.GetProfile;
using Microsoft.AspNetCore.Http;
using DisableTwoFactorCommand = AuthService.Application.Features.Auth.TwoFactor.DisableTwoFactorCommand;
using EnableTwoFactorCommand = AuthService.Application.Features.Auth.TwoFactor.EnableTwoFactorCommand;

namespace AuthService.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<RegisterResultDto>>> Register([FromBody] RegisterCommand command)
    {
        try
        {
            var result = await _mediator.Send(command);
            return Ok(ApiResponse<RegisterResultDto>.SuccessResponse(result, "User registered."));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<RegisterResultDto>.FailResponse("Registration failed.", new() { ex.Message }));
        }
    }

    [HttpPost("login")]
    public async Task<ActionResult<ApiResponse<LoginResultDto>>> Login([FromBody] LoginCommand command)
    {
        try
        {
            var result = await _mediator.Send(command);
            
            // Only set cookies if not requiring 2FA and tokens are present
            if (!result.RequiresTwoFactor && !string.IsNullOrEmpty(result.RefreshToken))
            {
                // Set RefreshToken in HttpOnly secure cookie (7 days)
                // Using SameSite=None for cross-origin gateway support
                Response.Cookies.Append("refreshToken", result.RefreshToken, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.None,  // Required for cross-origin (gateway) cookie support
                    Path = "/",
                    Expires = DateTimeOffset.UtcNow.AddDays(7)
                });
                
                // Set AccessToken in regular cookie (accessible by JavaScript for API calls)
                if (!string.IsNullOrEmpty(result.AccessToken))
                {
                    Response.Cookies.Append("accessToken", result.AccessToken, new CookieOptions
                    {
                        HttpOnly = false,
                        Secure = true,
                        SameSite = SameSiteMode.None,  // Required for cross-origin (gateway) cookie support
                        Path = "/",
                        Expires = DateTimeOffset.UtcNow.AddMinutes(15)
                    });
                }
            }
            
            return Ok(ApiResponse<LoginResultDto>.SuccessResponse(result, 
                result.RequiresTwoFactor ? "Two-factor authentication required." : "Login successful."));
        }
        catch (Exception ex)
        {
            return Unauthorized(ApiResponse<LoginResultDto>.FailResponse("Login failed.", new() { ex.Message }));
        }
    }

    [HttpPost("verify-2fa-login")]
    public async Task<ActionResult<ApiResponse<LoginResultDto>>> VerifyTwoFactorLogin([FromBody] VerifyTwoFactorLoginRequest request)
    {
        try
        {
            var command = new VerifyTwoFactorLoginCommand(
                request.Email,
                request.TwoFactorToken,
                request.Code,
                request.TwoFactorType);
            
            var result = await _mediator.Send(command);
            
            if (!string.IsNullOrEmpty(result.RefreshToken))
            {
                // Set RefreshToken in HttpOnly secure cookie (7 days)
                Response.Cookies.Append("refreshToken", result.RefreshToken, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.None,  // Required for cross-origin (gateway) cookie support
                    Path = "/",
                    Expires = DateTimeOffset.UtcNow.AddDays(7)
                });
                
                // Set AccessToken in regular cookie (accessible by JavaScript for API calls)
                if (!string.IsNullOrEmpty(result.AccessToken))
                {
                    Response.Cookies.Append("accessToken", result.AccessToken, new CookieOptions
                    {
                        HttpOnly = false,
                        Secure = true,
                        SameSite = SameSiteMode.None,  // Required for cross-origin (gateway) cookie support
                        Path = "/",
                        Expires = DateTimeOffset.UtcNow.AddMinutes(15)
                    });
                }
            }
            
            return Ok(ApiResponse<LoginResultDto>.SuccessResponse(result, "Login successful."));
        }
        catch (Exception ex)
        {
            return Unauthorized(ApiResponse<LoginResultDto>.FailResponse("Verification failed.", new() { ex.Message }));
        }
    }

    public class VerifyTwoFactorLoginRequest
    {
        public string Email { get; set; } = string.Empty;
        public string TwoFactorToken { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string TwoFactorType { get; set; } = string.Empty;
    }

    [HttpPost("resend-2fa-login")]
    public async Task<ActionResult<ApiResponse<string>>> ResendTwoFactorLoginCode([FromBody] ResendTwoFactorLoginRequest request)
    {
        try
        {
            var command = new ResendTwoFactorLoginCodeCommand(request.Email, request.TwoFactorToken);
            await _mediator.Send(command);
            return Ok(ApiResponse<string>.SuccessResponse("OK", "Two-factor code resent successfully."));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<string>.FailResponse("Failed to resend code.", new() { ex.Message }));
        }
    }

    public class ResendTwoFactorLoginRequest
    {
        public string Email { get; set; } = string.Empty;
        public string TwoFactorToken { get; set; } = string.Empty;
    }

    [HttpPost("logout")]
    public ActionResult<ApiResponse<string>> Logout()
    {
        // Delete RefreshToken cookie
        Response.Cookies.Delete("refreshToken", new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None,  // Must match how cookie was set
            Path = "/"
        });
        
        // Delete AccessToken cookie
        Response.Cookies.Delete("accessToken", new CookieOptions
        {
            HttpOnly = false,
            Secure = true,
            SameSite = SameSiteMode.None,  // Must match how cookie was set
            Path = "/"
        });
        
        return Ok(ApiResponse<string>.SuccessResponse("OK", "Logged out."));
    }

    [Authorize]
    [HttpGet("profile")]
    public async Task<ActionResult<ApiResponse<ProfileDto>>> Profile()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized(ApiResponse<ProfileDto>.FailResponse("User not found."));
        }
        
        var dto = await _mediator.Send(new GetProfileQuery(userId));
        return Ok(ApiResponse<ProfileDto>.SuccessResponse(dto));
    }

    [HttpPost("forgot-password")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<string>>> ForgotPassword([FromBody] ForgotPasswordCommand command)
    {
        // enrich command with client IP address
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        var enriched = command with { IpAddress = ip };
        await _mediator.Send(enriched);
        return Ok(ApiResponse<string>.SuccessResponse("OK", "If the email exists, a reset link was sent."));
    }

    [HttpPost("reset-password")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<string>>> ResetPassword([FromBody] ResetPasswordCommand command)
    {
        try
        {
            await _mediator.Send(command);
            return Ok(ApiResponse<string>.SuccessResponse("OK", "Password reset successful."));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<string>.FailResponse("Reset password failed.", new() { ex.Message }));
        }
    }

    [Authorize]
    [HttpPost("change-password")]
    public async Task<ActionResult<ApiResponse<string>>> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized(ApiResponse<string>.FailResponse("User not found."));
            }
            
            var cmd = new ChangePasswordCommand(userId, request.CurrentPassword, request.NewPassword);
            await _mediator.Send(cmd);
            return Ok(ApiResponse<string>.SuccessResponse("OK", "Password changed."));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<string>.FailResponse("Change password failed.", new() { ex.Message }));
        }
    }

    public class ChangePasswordRequest
    {
        public string CurrentPassword { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }

    [HttpPost("refresh-token")]
    public async Task<ActionResult<ApiResponse<RefreshTokenResultDto>>> RefreshToken()
    {
        if (!Request.Cookies.TryGetValue("refreshToken", out var token) || string.IsNullOrWhiteSpace(token))
        {
            return Unauthorized(ApiResponse<RefreshTokenResultDto>.FailResponse("No refresh token."));
        }
        
        try
        {
            var result = await _mediator.Send(new RefreshTokenCommand(token));
            
            // Set new RefreshToken in HttpOnly secure cookie (7 days)
            Response.Cookies.Append("refreshToken", result.NewRefreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,  // Required for cross-origin (gateway) cookie support
                Path = "/",
                Expires = DateTimeOffset.UtcNow.AddDays(7)
            });
            
            // Set AccessToken in regular cookie (accessible by JavaScript for API calls)
            Response.Cookies.Append("accessToken", result.AccessToken, new CookieOptions
            {
                HttpOnly = false,
                Secure = true,
                SameSite = SameSiteMode.None,  // Required for cross-origin (gateway) cookie support
                Path = "/",
                Expires = DateTimeOffset.UtcNow.AddMinutes(15)
            });
            
            return Ok(ApiResponse<RefreshTokenResultDto>.SuccessResponse(result, "Token refreshed."));
        }
        catch (Exception ex)
        {
            return Unauthorized(ApiResponse<RefreshTokenResultDto>.FailResponse("Refresh failed.", new() { ex.Message }));
        }
    }

    [HttpPost("revoke-token")]
    public async Task<ActionResult<ApiResponse<string>>> RevokeToken()
    {
        if (!Request.Cookies.TryGetValue("refreshToken", out var token) || string.IsNullOrWhiteSpace(token))
        {
            return BadRequest(ApiResponse<string>.FailResponse("No refresh token."));
        }
        
        var revoked = await _mediator.Send(new RevokeTokenCommand(token));
        if (revoked)
        {
            Response.Cookies.Delete("refreshToken", new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,  // Must match how cookie was set
                Path = "/"
            });
            return Ok(ApiResponse<string>.SuccessResponse("OK", "Refresh token revoked."));
        }
        
        return BadRequest(ApiResponse<string>.FailResponse("Token already revoked or not found."));
    }

    [HttpPost("send-confirmation-email")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<string>>> SendConfirmationEmail([FromBody] SendEmailConfirmationCommand command)
    {
        var result = await _mediator.Send(command);
        if (!result)
        {
            return Ok(ApiResponse<string>.SuccessResponse("OK", "Your email is already confirmed."));
        }
        return Ok(ApiResponse<string>.SuccessResponse("OK", "If the email exists, a confirmation email was sent."));
    }

    [HttpGet("confirm-email")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<string>>> ConfirmEmail([FromQuery] string email, [FromQuery] string token)
    {
        try
        {
            await _mediator.Send(new ConfirmEmailCommand(email, token));
            return Ok(ApiResponse<string>.SuccessResponse("OK", "Email confirmed."));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<string>.FailResponse("Email confirmation failed.", new() { ex.Message }));
        }
    }

    [Authorize]
    [HttpPost("2fa/generate")]
    public async Task<ActionResult<ApiResponse<string>>> GenerateTwoFactorCode()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized(ApiResponse<string>.FailResponse("User not found."));
        }
        
        await _mediator.Send(new GenerateTwoFactorCodeCommand(userId));
        return Ok(ApiResponse<string>.SuccessResponse("OK", "2FA code sent."));
    }

    [Authorize]
    [HttpPost("2fa/verify")]
    public async Task<ActionResult<ApiResponse<string>>> VerifyTwoFactorCode([FromBody] VerifyTwoFactorRequest request)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized(ApiResponse<string>.FailResponse("User not found."));
            }
            
            await _mediator.Send(new VerifyTwoFactorCodeCommand(userId, request.Code));
            return Ok(ApiResponse<string>.SuccessResponse("OK", "2FA code verified."));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<string>.FailResponse("2FA verification failed.", new() { ex.Message }));
        }
    }

    [Authorize]
    [HttpPost("2fa/enable")]
    public async Task<ActionResult<ApiResponse<string>>> EnableTwoFactor()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized(ApiResponse<string>.FailResponse("User not found."));
        }
        
        await _mediator.Send(new EnableTwoFactorCommand(userId));
        return Ok(ApiResponse<string>.SuccessResponse("OK", "Two-factor enabled."));
    }

    [Authorize]
    [HttpPost("2fa/disable")]
    public async Task<ActionResult<ApiResponse<string>>> DisableTwoFactor()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized(ApiResponse<string>.FailResponse("User not found."));
        }
        
        await _mediator.Send(new DisableTwoFactorCommand(userId));
        return Ok(ApiResponse<string>.SuccessResponse("OK", "Two-factor disabled."));
    }

    public class VerifyTwoFactorRequest
    {
        public string Code { get; set; } = string.Empty;
    }

    // ==================== AUTHENTICATOR APP ENDPOINTS ====================

    /// <summary>
    /// Setup authenticator app - generates secret key and QR code URI
    /// </summary>
    [Authorize]
    [HttpPost("authenticator/setup")]
    public async Task<ActionResult<ApiResponse<AuthenticatorSetupDto>>> SetupAuthenticator()
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized(ApiResponse<AuthenticatorSetupDto>.FailResponse("User not found."));
            }
            
            var result = await _mediator.Send(new SetupAuthenticatorCommand(userId));
            return Ok(ApiResponse<AuthenticatorSetupDto>.SuccessResponse(result, "Authenticator setup initiated."));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<AuthenticatorSetupDto>.FailResponse("Setup failed.", new() { ex.Message }));
        }
    }

    /// <summary>
    /// Enable authenticator app after verifying the code
    /// </summary>
    [Authorize]
    [HttpPost("authenticator/enable")]
    public async Task<ActionResult<ApiResponse<string>>> EnableAuthenticator([FromBody] EnableAuthenticatorRequest request)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized(ApiResponse<string>.FailResponse("User not found."));
            }
            
            await _mediator.Send(new EnableAuthenticatorCommand(userId, request.Code));
            return Ok(ApiResponse<string>.SuccessResponse("OK", "Authenticator app enabled successfully."));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<string>.FailResponse("Enable failed.", new() { ex.Message }));
        }
    }

    public class EnableAuthenticatorRequest
    {
        public string Code { get; set; } = string.Empty;
    }

    /// <summary>
    /// Disable authenticator app
    /// </summary>
    [Authorize]
    [HttpPost("authenticator/disable")]
    public async Task<ActionResult<ApiResponse<string>>> DisableAuthenticator()
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized(ApiResponse<string>.FailResponse("User not found."));
            }
            
            await _mediator.Send(new DisableAuthenticatorCommand(userId));
            return Ok(ApiResponse<string>.SuccessResponse("OK", "Authenticator app disabled."));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<string>.FailResponse("Disable failed.", new() { ex.Message }));
        }
    }

    /// <summary>
    /// Get authenticator status
    /// </summary>
    [Authorize]
    [HttpGet("authenticator/status")]
    public async Task<ActionResult<ApiResponse<AuthenticatorStatusDto>>> GetAuthenticatorStatus()
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized(ApiResponse<AuthenticatorStatusDto>.FailResponse("User not found."));
            }
            
            var result = await _mediator.Send(new GetAuthenticatorStatusQuery(userId));
            return Ok(ApiResponse<AuthenticatorStatusDto>.SuccessResponse(result));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<AuthenticatorStatusDto>.FailResponse("Failed to get status.", new() { ex.Message }));
        }
    }

    /// <summary>
    /// Verify authenticator code (for testing during setup)
    /// </summary>
    [Authorize]
    [HttpPost("authenticator/verify")]
    public async Task<ActionResult<ApiResponse<string>>> VerifyAuthenticatorCode([FromBody] VerifyAuthenticatorRequest request)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized(ApiResponse<string>.FailResponse("User not found."));
            }
            
            await _mediator.Send(new VerifyAuthenticatorCodeCommand(userId, request.Code));
            return Ok(ApiResponse<string>.SuccessResponse("OK", "Code verified successfully."));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<string>.FailResponse("Verification failed.", new() { ex.Message }));
        }
    }

    public class VerifyAuthenticatorRequest
    {
        public string Code { get; set; } = string.Empty;
    }

    [HttpPost("external-login")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<LoginResultDto>>> ExternalLogin([FromBody] ExternalLoginCommand command)
    {
        try
        {
            var result = await _mediator.Send(command);
            return Ok(ApiResponse<LoginResultDto>.SuccessResponse(result, "External login successful."));
        }
        catch (Exception ex)
        {
            return Unauthorized(ApiResponse<LoginResultDto>.FailResponse("External login failed.", new() { ex.Message }));
        }
    }
}
