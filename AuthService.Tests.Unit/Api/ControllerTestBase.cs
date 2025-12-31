namespace AuthService.Tests.Unit;

/// <summary>
/// Base class for controller unit tests providing common functionality
/// </summary>
public abstract class ControllerTestBase
{
    protected Mock<IMediator> MediatorMock { get; }

    protected ControllerTestBase()
    {
        MediatorMock = new Mock<IMediator>();
    }

    /// <summary>
    /// Creates a ClaimsPrincipal with the specified user ID
    /// </summary>
    protected ClaimsPrincipal CreateAuthenticatedUser(string userId, params string[] roles)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId),
            new(ClaimTypes.Name, $"testuser-{userId}"),
            new(ClaimTypes.Email, $"test-{userId}@example.com")
        };

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var identity = new ClaimsIdentity(claims, "TestAuthentication");
        return new ClaimsPrincipal(identity);
    }

    /// <summary>
    /// Creates an unauthenticated ClaimsPrincipal
    /// </summary>
    protected ClaimsPrincipal CreateUnauthenticatedUser()
    {
        return new ClaimsPrincipal(new ClaimsIdentity());
    }

    /// <summary>
    /// Sets up the HttpContext for a controller with the specified user
    /// </summary>
    protected void SetupControllerContext(ControllerBase controller, ClaimsPrincipal user)
    {
        var httpContext = new DefaultHttpContext
        {
            User = user
        };
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };
    }

    /// <summary>
    /// Sets up the HttpContext for a controller with an authenticated user
    /// </summary>
    protected void SetupControllerContext(ControllerBase controller, string userId, params string[] roles)
    {
        var user = CreateAuthenticatedUser(userId, roles);
        SetupControllerContext(controller, user);
    }

    /// <summary>
    /// Sets up the HttpContext for a controller with cookies
    /// </summary>
    protected void SetupControllerContextWithCookies(ControllerBase controller, ClaimsPrincipal user, Dictionary<string, string> cookies)
    {
        var httpContext = new DefaultHttpContext
        {
            User = user
        };

        // Setup request cookies
        var cookieCollection = new Mock<IRequestCookieCollection>();
        foreach (var cookie in cookies)
        {
            string? cookieValue = cookie.Value;
            cookieCollection.Setup(c => c.TryGetValue(cookie.Key, out cookieValue))
                .Returns(true);
            cookieCollection.Setup(c => c[cookie.Key]).Returns(cookie.Value);
        }

        // Alternative approach: setup the request cookies directly
        var requestCookiesMock = new Mock<IRequestCookieCollection>();
        foreach (var cookie in cookies)
        {
            string? outVal = cookie.Value;
            requestCookiesMock.Setup(c => c.TryGetValue(cookie.Key, out outVal)).Returns(true);
        }

        httpContext.Request.Headers["Cookie"] = string.Join("; ", cookies.Select(c => $"{c.Key}={c.Value}"));

        // Setup response cookies
        httpContext.Response.Headers.Append("Set-Cookie", "");

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };
    }

    private delegate bool TryGetValueDelegate(string key, out string? value);

    /// <summary>
    /// Verifies that the result is an OkObjectResult with the expected ApiResponse and returns it
    /// </summary>
    protected ApiResponse<T>? AssertOkResult<T>(ActionResult<ApiResponse<T>> result)
    {
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        okResult!.Value.Should().BeOfType<ApiResponse<T>>();
        var apiResponse = okResult.Value as ApiResponse<T>;
        apiResponse!.Success.Should().BeTrue();
        return apiResponse;
    }

    /// <summary>
    /// Verifies that the result is a BadRequestObjectResult and returns the ApiResponse
    /// </summary>
    protected ApiResponse<T>? AssertBadRequestResult<T>(ActionResult<ApiResponse<T>> result)
    {
        result.Result.Should().BeOfType<BadRequestObjectResult>();
        var badResult = result.Result as BadRequestObjectResult;
        badResult!.Value.Should().BeOfType<ApiResponse<T>>();
        var apiResponse = badResult.Value as ApiResponse<T>;
        apiResponse!.Success.Should().BeFalse();
        return apiResponse;
    }

    /// <summary>
    /// Verifies that the result is an UnauthorizedObjectResult and returns the ApiResponse
    /// </summary>
    protected ApiResponse<T>? AssertUnauthorizedResult<T>(ActionResult<ApiResponse<T>> result)
    {
        result.Result.Should().BeOfType<UnauthorizedObjectResult>();
        var unauthorizedResult = result.Result as UnauthorizedObjectResult;
        unauthorizedResult!.Value.Should().BeOfType<ApiResponse<T>>();
        var apiResponse = unauthorizedResult.Value as ApiResponse<T>;
        apiResponse!.Success.Should().BeFalse();
        return apiResponse;
    }

    /// <summary>
    /// Verifies that the result is a NotFoundObjectResult and returns the ApiResponse
    /// </summary>
    protected ApiResponse<T>? AssertNotFoundResult<T>(ActionResult<ApiResponse<T>> result)
    {
        result.Result.Should().BeOfType<NotFoundObjectResult>();
        var notFoundResult = result.Result as NotFoundObjectResult;
        notFoundResult!.Value.Should().BeOfType<ApiResponse<T>>();
        var apiResponse = notFoundResult.Value as ApiResponse<T>;
        apiResponse!.Success.Should().BeFalse();
        return apiResponse;
    }

    /// <summary>
    /// Verifies that the result is an ObjectResult with status code 500 and returns the ApiResponse
    /// </summary>
    protected ApiResponse<T>? AssertInternalServerErrorResult<T>(ActionResult<ApiResponse<T>> result)
    {
        result.Result.Should().BeOfType<ObjectResult>();
        var objectResult = result.Result as ObjectResult;
        objectResult!.StatusCode.Should().Be(500);
        objectResult.Value.Should().BeOfType<ApiResponse<T>>();
        var apiResponse = objectResult.Value as ApiResponse<T>;
        apiResponse!.Success.Should().BeFalse();
        return apiResponse;
    }

    /// <summary>
    /// Verifies that the result is a CreatedAtActionResult and returns the ApiResponse
    /// </summary>
    protected ApiResponse<T>? AssertCreatedResult<T>(ActionResult<ApiResponse<T>> result)
    {
        result.Result.Should().BeOfType<CreatedAtActionResult>();
        var createdResult = result.Result as CreatedAtActionResult;
        createdResult!.Value.Should().BeOfType<ApiResponse<T>>();
        var apiResponse = createdResult.Value as ApiResponse<T>;
        apiResponse!.Success.Should().BeTrue();
        return apiResponse;
    }

    /// <summary>
    /// Gets the ApiResponse data from an ActionResult
    /// </summary>
    protected ApiResponse<T>? GetApiResponse<T>(ActionResult<ApiResponse<T>> result)
    {
        if (result.Result is ObjectResult objectResult)
        {
            return objectResult.Value as ApiResponse<T>;
        }
        return null;
    }
}
