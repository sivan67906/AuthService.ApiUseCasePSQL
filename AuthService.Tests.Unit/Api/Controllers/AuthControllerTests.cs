using AuthService.Application.Features.Auth.Authenticator;
using AuthService.Application.Features.Auth.ChangePassword;
using AuthService.Application.Features.Auth.EmailConfirmation;
using AuthService.Application.Features.Auth.ForgotPassword;
using AuthService.Application.Features.Auth.Login;
using AuthService.Application.Features.Auth.RefreshToken;
using AuthService.Application.Features.Auth.Register;
using AuthService.Application.Features.Auth.ResetPassword;
using AuthService.Application.Features.Auth.RevokeToken;
using AuthService.Application.Features.Auth.TwoFactor;
using AuthService.Application.Features.Profile.GetProfile;

namespace AuthService.Tests.Unit.Api.Controllers;

public class AuthControllerTests : ControllerTestBase
{
    private readonly AuthController _controller;

    public AuthControllerTests()
    {
        _controller = new AuthController(MediatorMock.Object);
        SetupControllerContext(_controller, Guid.NewGuid().ToString());
    }

    #region Register Tests

    [Fact]
    public async Task Register_WithValidCommand_ReturnsOkWithResult()
    {
        // Arrange
        var command = new RegisterCommand("test@example.com", "Password123!", "John", "Doe", "1234567890");
        var expectedResult = new RegisterResultDto { UserId = Guid.NewGuid().ToString(), Email = "test@example.com" };

        MediatorMock.Setup(m => m.Send(It.IsAny<RegisterCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _controller.Register(command);

        // Assert
        var response = AssertOkResult<RegisterResultDto>(result);
        response!.Data!.Email.Should().Be("test@example.com");
    }

    [Fact]
    public async Task Register_WithDuplicateEmail_ReturnsBadRequest()
    {
        // Arrange
        var command = new RegisterCommand("existing@example.com", "Password123!", "John", "Doe", "1234567890");

        MediatorMock.Setup(m => m.Send(It.IsAny<RegisterCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Email already registered"));

        // Act
        var result = await _controller.Register(command);

        // Assert
        AssertBadRequestResult<RegisterResultDto>(result);
    }

    [Fact]
    public async Task Register_WithWeakPassword_ReturnsBadRequest()
    {
        // Arrange
        var command = new RegisterCommand("test@example.com", "weak", "John", "Doe", "1234567890");

        MediatorMock.Setup(m => m.Send(It.IsAny<RegisterCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Password does not meet requirements"));

        // Act
        var result = await _controller.Register(command);

        // Assert
        AssertBadRequestResult<RegisterResultDto>(result);
    }

    #endregion

    #region Login Tests

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsOkWithTokens()
    {
        // Arrange
        var command = new LoginCommand("test@example.com", "Password123!");
        var expectedResult = new LoginResultDto
        {
            AccessToken = "access-token",
            ExpiresInSeconds = 900,
            RefreshToken = "refresh-token",
            RequiresTwoFactor = false
        };

        MediatorMock.Setup(m => m.Send(It.IsAny<LoginCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _controller.Login(command);

        // Assert
        var response = AssertOkResult<LoginResultDto>(result);
        response!.Data!.AccessToken.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ReturnsUnauthorized()
    {
        // Arrange
        var command = new LoginCommand("test@example.com", "WrongPassword");

        MediatorMock.Setup(m => m.Send(It.IsAny<LoginCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Invalid credentials"));

        // Act
        var result = await _controller.Login(command);

        // Assert
        AssertUnauthorizedResult<LoginResultDto>(result);
    }

    [Fact]
    public async Task Login_WithTwoFactorRequired_ReturnsOkWithTwoFactorFlag()
    {
        // Arrange
        var command = new LoginCommand("test@example.com", "Password123!");
        var expectedResult = new LoginResultDto
        {
            RequiresTwoFactor = true,
            TwoFactorType = "Email",
            TwoFactorToken = "2fa-token"
        };

        MediatorMock.Setup(m => m.Send(It.IsAny<LoginCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _controller.Login(command);

        // Assert
        var response = AssertOkResult<LoginResultDto>(result);
        response!.Data!.RequiresTwoFactor.Should().BeTrue();
    }

    #endregion

    #region ForgotPassword Tests

    [Fact]
    public async Task ForgotPassword_WithValidEmail_ReturnsOk()
    {
        // Arrange
        var command = new ForgotPasswordCommand("test@example.com", "https://app.example.com/reset", null);

        MediatorMock.Setup(m => m.Send(It.IsAny<ForgotPasswordCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.ForgotPassword(command);

        // Assert
        AssertOkResult<string>(result);
    }

    [Fact]
    public async Task ForgotPassword_WithNonExistentEmail_ReturnsOk()
    {
        // Arrange - Should still return OK for security (don't reveal if email exists)
        var command = new ForgotPasswordCommand("nonexistent@example.com", "https://app.example.com/reset", null);

        MediatorMock.Setup(m => m.Send(It.IsAny<ForgotPasswordCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.ForgotPassword(command);

        // Assert
        AssertOkResult<string>(result);
    }

    #endregion

    #region ResetPassword Tests

    [Fact]
    public async Task ResetPassword_WithValidToken_ReturnsOk()
    {
        // Arrange
        var command = new ResetPasswordCommand("test@example.com", "valid-token", "NewPassword123!", "NewPassword123!");

        MediatorMock.Setup(m => m.Send(It.IsAny<ResetPasswordCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.ResetPassword(command);

        // Assert
        AssertOkResult<string>(result);
    }

    [Fact]
    public async Task ResetPassword_WithInvalidToken_ReturnsBadRequest()
    {
        // Arrange
        var command = new ResetPasswordCommand("test@example.com", "invalid-token", "NewPassword123!", "NewPassword123!");

        MediatorMock.Setup(m => m.Send(It.IsAny<ResetPasswordCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Invalid or expired token"));

        // Act
        var result = await _controller.ResetPassword(command);

        // Assert
        AssertBadRequestResult<string>(result);
    }

    #endregion

    #region ChangePassword Tests

    [Fact]
    public async Task ChangePassword_WithValidRequest_ReturnsOk()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        SetupControllerContext(_controller, userId);
        var request = new AuthController.ChangePasswordRequest
        {
            CurrentPassword = "OldPassword123!",
            NewPassword = "NewPassword123!"
        };

        MediatorMock.Setup(m => m.Send(It.IsAny<ChangePasswordCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.ChangePassword(request);

        // Assert
        AssertOkResult<string>(result);
    }

    [Fact]
    public async Task ChangePassword_WithIncorrectCurrentPassword_ReturnsBadRequest()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        SetupControllerContext(_controller, userId);
        var request = new AuthController.ChangePasswordRequest
        {
            CurrentPassword = "WrongPassword",
            NewPassword = "NewPassword123!"
        };

        MediatorMock.Setup(m => m.Send(It.IsAny<ChangePasswordCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Current password is incorrect"));

        // Act
        var result = await _controller.ChangePassword(request);

        // Assert
        AssertBadRequestResult<string>(result);
    }

    #endregion

    #region RefreshToken Tests

    [Fact]
    public async Task RefreshToken_WithValidCookie_ReturnsOkWithNewTokens()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var cookies = new Dictionary<string, string> { { "refreshToken", "valid-refresh-token" } };
        SetupControllerContextWithCookies(_controller, CreateAuthenticatedUser(userId), cookies);

        var expectedResult = new RefreshTokenResultDto
        {
            AccessToken = "new-access-token",
            ExpiresInSeconds = 900,
            NewRefreshToken = "new-refresh-token"
        };

        MediatorMock.Setup(m => m.Send(It.IsAny<RefreshTokenCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _controller.RefreshToken();

        // Assert
        var response = AssertOkResult<RefreshTokenResultDto>(result);
        response!.Data!.AccessToken.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task RefreshToken_WithMissingCookie_ReturnsUnauthorized()
    {
        // Arrange - Controller with no refresh token cookie
        var userId = Guid.NewGuid().ToString();
        SetupControllerContext(_controller, userId);

        // Act
        var result = await _controller.RefreshToken();

        // Assert
        AssertUnauthorizedResult<RefreshTokenResultDto>(result);
    }

    #endregion

    #region RevokeToken Tests

    [Fact]
    public async Task RevokeToken_WithValidCookie_ReturnsOk()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var cookies = new Dictionary<string, string> { { "refreshToken", "valid-refresh-token" } };
        SetupControllerContextWithCookies(_controller, CreateAuthenticatedUser(userId), cookies);

        MediatorMock.Setup(m => m.Send(It.IsAny<RevokeTokenCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.RevokeToken();

        // Assert
        AssertOkResult<string>(result);
    }

    [Fact]
    public async Task RevokeToken_WithMissingCookie_ReturnsBadRequest()
    {
        // Arrange - Controller with no refresh token cookie
        var userId = Guid.NewGuid().ToString();
        SetupControllerContext(_controller, userId);

        // Act
        var result = await _controller.RevokeToken();

        // Assert
        AssertBadRequestResult<string>(result);
    }

    #endregion

    #region TwoFactor Tests

    [Fact]
    public async Task GenerateTwoFactorCode_WithAuthenticatedUser_ReturnsOk()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        SetupControllerContext(_controller, userId);

        MediatorMock.Setup(m => m.Send(It.IsAny<GenerateTwoFactorCodeCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.GenerateTwoFactorCode();

        // Assert
        AssertOkResult<string>(result);
    }

    [Fact]
    public async Task VerifyTwoFactorCode_WithValidCode_ReturnsOk()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        SetupControllerContext(_controller, userId);
        var request = new AuthController.VerifyTwoFactorRequest { Code = "123456" };

        MediatorMock.Setup(m => m.Send(It.IsAny<VerifyTwoFactorCodeCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.VerifyTwoFactorCode(request);

        // Assert
        AssertOkResult<string>(result);
    }

    [Fact]
    public async Task EnableTwoFactor_WithAuthenticatedUser_ReturnsOk()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        SetupControllerContext(_controller, userId);

        MediatorMock.Setup(m => m.Send(It.IsAny<EnableTwoFactorCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.EnableTwoFactor();

        // Assert
        AssertOkResult<string>(result);
    }

    [Fact]
    public async Task DisableTwoFactor_WithAuthenticatedUser_ReturnsOk()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        SetupControllerContext(_controller, userId);

        MediatorMock.Setup(m => m.Send(It.IsAny<DisableTwoFactorCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.DisableTwoFactor();

        // Assert
        AssertOkResult<string>(result);
    }

    #endregion

    #region Authenticator Tests

    [Fact]
    public async Task SetupAuthenticator_WithAuthenticatedUser_ReturnsOkWithSetupInfo()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        SetupControllerContext(_controller, userId);

        var expectedResult = new AuthenticatorSetupDto
        {
            SecretKey = "ABCDEFGHIJKLMNOP",
            QrCodeUri = "otpauth://totp/App:test@example.com?secret=ABCDEFGHIJKLMNOP",
            ManualEntryKey = "ABCD EFGH IJKL MNOP"
        };

        MediatorMock.Setup(m => m.Send(It.IsAny<SetupAuthenticatorCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _controller.SetupAuthenticator();

        // Assert
        var response = AssertOkResult<AuthenticatorSetupDto>(result);
        response!.Data!.SecretKey.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task EnableAuthenticator_WithValidCode_ReturnsOk()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        SetupControllerContext(_controller, userId);
        var request = new AuthController.EnableAuthenticatorRequest { Code = "123456" };

        MediatorMock.Setup(m => m.Send(It.IsAny<EnableAuthenticatorCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.EnableAuthenticator(request);

        // Assert
        AssertOkResult<string>(result);
    }

    [Fact]
    public async Task EnableAuthenticator_WithInvalidCode_ReturnsBadRequest()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        SetupControllerContext(_controller, userId);
        var request = new AuthController.EnableAuthenticatorRequest { Code = "000000" };

        MediatorMock.Setup(m => m.Send(It.IsAny<EnableAuthenticatorCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Invalid code"));

        // Act
        var result = await _controller.EnableAuthenticator(request);

        // Assert
        AssertBadRequestResult<string>(result);
    }

    [Fact]
    public async Task DisableAuthenticator_WithAuthenticatedUser_ReturnsOk()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        SetupControllerContext(_controller, userId);

        MediatorMock.Setup(m => m.Send(It.IsAny<DisableAuthenticatorCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.DisableAuthenticator();

        // Assert
        AssertOkResult<string>(result);
    }

    [Fact]
    public async Task GetAuthenticatorStatus_WithAuthenticatedUser_ReturnsOkWithStatus()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        SetupControllerContext(_controller, userId);

        var expectedResult = new AuthenticatorStatusDto
        {
            IsEnabled = true,
            TwoFactorEnabled = true,
            TwoFactorType = "Authenticator"
        };

        MediatorMock.Setup(m => m.Send(It.IsAny<GetAuthenticatorStatusQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _controller.GetAuthenticatorStatus();

        // Assert
        var response = AssertOkResult<AuthenticatorStatusDto>(result);
        response!.Data!.IsEnabled.Should().BeTrue();
    }

    [Fact]
    public async Task VerifyAuthenticatorCode_WithValidCode_ReturnsOk()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        SetupControllerContext(_controller, userId);
        var request = new AuthController.VerifyAuthenticatorRequest { Code = "123456" };

        MediatorMock.Setup(m => m.Send(It.IsAny<VerifyAuthenticatorCodeCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.VerifyAuthenticatorCode(request);

        // Assert
        AssertOkResult<string>(result);
    }

    #endregion

    #region EmailConfirmation Tests

    [Fact]
    public async Task SendConfirmationEmail_WithValidEmail_ReturnsOk()
    {
        // Arrange
        var command = new SendEmailConfirmationCommand("test@example.com", "https://app.example.com/confirm");

        MediatorMock.Setup(m => m.Send(It.IsAny<SendEmailConfirmationCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.SendConfirmationEmail(command);

        // Assert
        AssertOkResult<string>(result);
    }

    [Fact]
    public async Task ConfirmEmail_WithValidToken_ReturnsOk()
    {
        // Arrange
        MediatorMock.Setup(m => m.Send(It.IsAny<ConfirmEmailCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.ConfirmEmail("test@example.com", "valid-token");

        // Assert
        AssertOkResult<string>(result);
    }

    [Fact]
    public async Task ConfirmEmail_WithInvalidToken_ReturnsBadRequest()
    {
        // Arrange
        MediatorMock.Setup(m => m.Send(It.IsAny<ConfirmEmailCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Invalid or expired token"));

        // Act
        var result = await _controller.ConfirmEmail("test@example.com", "invalid-token");

        // Assert
        AssertBadRequestResult<string>(result);
    }

    #endregion

    #region VerifyTwoFactorLogin Tests

    [Fact]
    public async Task VerifyTwoFactorLogin_WithValidCode_ReturnsOkWithTokens()
    {
        // Arrange
        var request = new AuthController.VerifyTwoFactorLoginRequest
        {
            Email = "test@example.com",
            TwoFactorToken = "2fa-token",
            Code = "123456",
            TwoFactorType = "Email"
        };

        var expectedResult = new LoginResultDto
        {
            AccessToken = "access-token",
            RefreshToken = "refresh-token",
            RequiresTwoFactor = false
        };

        MediatorMock.Setup(m => m.Send(It.IsAny<VerifyTwoFactorLoginCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _controller.VerifyTwoFactorLogin(request);

        // Assert
        var response = AssertOkResult<LoginResultDto>(result);
        response!.Data!.AccessToken.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task VerifyTwoFactorLogin_WithInvalidCode_ReturnsUnauthorized()
    {
        // Arrange
        var request = new AuthController.VerifyTwoFactorLoginRequest
        {
            Email = "test@example.com",
            TwoFactorToken = "2fa-token",
            Code = "000000",
            TwoFactorType = "Email"
        };

        MediatorMock.Setup(m => m.Send(It.IsAny<VerifyTwoFactorLoginCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Invalid code"));

        // Act
        var result = await _controller.VerifyTwoFactorLogin(request);

        // Assert
        AssertUnauthorizedResult<LoginResultDto>(result);
    }

    #endregion

    #region ResendTwoFactorLoginCode Tests

    [Fact]
    public async Task ResendTwoFactorLoginCode_WithValidRequest_ReturnsOk()
    {
        // Arrange
        var request = new AuthController.ResendTwoFactorLoginRequest
        {
            Email = "test@example.com",
            TwoFactorToken = "2fa-token"
        };

        var expectedResult = new ResendTwoFactorCodeResultDto(
            NewTwoFactorToken: "new-2fa-token",
            Message: "Code sent"
        );

        MediatorMock.Setup(m => m.Send(It.IsAny<ResendTwoFactorLoginCodeCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _controller.ResendTwoFactorLoginCode(request);

        // Assert
        AssertOkResult<ResendTwoFactorCodeResultDto>(result);
    }

    [Fact]
    public async Task ResendTwoFactorLoginCode_WithInvalidToken_ReturnsBadRequest()
    {
        // Arrange
        var request = new AuthController.ResendTwoFactorLoginRequest
        {
            Email = "test@example.com",
            TwoFactorToken = "invalid-token"
        };

        MediatorMock.Setup(m => m.Send(It.IsAny<ResendTwoFactorLoginCodeCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Invalid token"));

        // Act
        var result = await _controller.ResendTwoFactorLoginCode(request);

        // Assert
        AssertBadRequestResult<ResendTwoFactorCodeResultDto>(result);
    }

    #endregion

    #region Profile Tests

    [Fact]
    public async Task Profile_WithAuthenticatedUser_ReturnsOkWithProfile()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        SetupControllerContext(_controller, userId);

        var expectedResult = new ProfileDto
        {
            Id = userId,
            Email = "test@example.com",
            FirstName = "John",
            LastName = "Doe"
        };

        MediatorMock.Setup(m => m.Send(It.IsAny<GetProfileQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _controller.Profile();

        // Assert
        var response = AssertOkResult<ProfileDto>(result);
        response!.Data!.Email.Should().Be("test@example.com");
    }

    [Fact]
    public async Task Profile_WithUnauthenticatedUser_ReturnsUnauthorized()
    {
        // Arrange
        var unauthenticatedUser = CreateUnauthenticatedUser();
        SetupControllerContext(_controller, unauthenticatedUser);

        // Act
        var result = await _controller.Profile();

        // Assert
        AssertUnauthorizedResult<ProfileDto>(result);
    }

    #endregion

    #region Logout Tests

    [Fact]
    public void Logout_ReturnsOk()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        SetupControllerContext(_controller, userId);

        // Act
        var result = _controller.Logout();

        // Assert
        AssertOkResult<string>(result);
    }

    #endregion
}
