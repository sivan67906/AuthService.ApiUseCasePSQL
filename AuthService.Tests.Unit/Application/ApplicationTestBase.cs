using Microsoft.Extensions.Configuration;
using MockQueryable.Moq;

namespace AuthService.Tests.Unit.Application;

/// <summary>
/// Base class for Application layer unit tests providing common mock setups
/// </summary>
public abstract class ApplicationTestBase
{
    protected Mock<IAppDbContext> DbContextMock { get; }
    protected Mock<UserManager<ApplicationUser>> UserManagerMock { get; }
    protected Mock<RoleManager<ApplicationRole>> RoleManagerMock { get; }
    protected Mock<SignInManager<ApplicationUser>> SignInManagerMock { get; }
    protected Mock<IEmailService> EmailServiceMock { get; }
    protected Mock<IConfiguration> ConfigurationMock { get; }
    protected Mock<IHttpContextAccessor> HttpContextAccessorMock { get; }
    protected Mock<IMediator> MediatorMock { get; }
    protected Mock<ITwoFactorCodeThrottlingService> TwoFactorThrottlingServiceMock { get; }
    protected Mock<IEmailResendThrottlingService> EmailResendThrottlingServiceMock { get; }
    protected Mock<IEmailConfirmationTokenTracker> EmailConfirmationTokenTrackerMock { get; }
    protected Mock<IUserAuthorizationService> UserAuthorizationServiceMock { get; }

    protected ApplicationTestBase()
    {
        DbContextMock = new Mock<IAppDbContext>();

        // Setup UserManager mock
        var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
        UserManagerMock = new Mock<UserManager<ApplicationUser>>(
            userStoreMock.Object, null!, null!, null!, null!, null!, null!, null!, null!);

        // Setup RoleManager mock
        var roleStoreMock = new Mock<IRoleStore<ApplicationRole>>();
        RoleManagerMock = new Mock<RoleManager<ApplicationRole>>(
            roleStoreMock.Object, null!, null!, null!, null!);

        // Setup SignInManager mock
        var contextAccessorMock = new Mock<IHttpContextAccessor>();
        var claimsFactoryMock = new Mock<IUserClaimsPrincipalFactory<ApplicationUser>>();
        SignInManagerMock = new Mock<SignInManager<ApplicationUser>>(
            UserManagerMock.Object,
            contextAccessorMock.Object,
            claimsFactoryMock.Object,
            null!, null!, null!, null!);

        EmailServiceMock = new Mock<IEmailService>();
        ConfigurationMock = new Mock<IConfiguration>();
        HttpContextAccessorMock = new Mock<IHttpContextAccessor>();
        MediatorMock = new Mock<IMediator>();
        TwoFactorThrottlingServiceMock = new Mock<ITwoFactorCodeThrottlingService>();
        EmailResendThrottlingServiceMock = new Mock<IEmailResendThrottlingService>();
        EmailConfirmationTokenTrackerMock = new Mock<IEmailConfirmationTokenTracker>();
        UserAuthorizationServiceMock = new Mock<IUserAuthorizationService>();

        SetupDefaultConfigurations();
    }

    protected virtual void SetupDefaultConfigurations()
    {
        // Setup default JWT configuration
        ConfigurationMock.Setup(c => c["Jwt:Key"]).Returns("ThisIsASecretKeyForTestingPurposesOnly12345678");
        ConfigurationMock.Setup(c => c["Jwt:Issuer"]).Returns("TestIssuer");
        ConfigurationMock.Setup(c => c["Jwt:Audience"]).Returns("TestAudience");
        ConfigurationMock.Setup(c => c["Email:ConfirmationCallbackUrl"]).Returns("https://test.com/confirm");
    }

    /// <summary>
    /// Creates a mock DbSet from a list of entities using MockQueryable
    /// </summary>
    protected Mock<DbSet<T>> SetupMockDbSet<T>(List<T> data) where T : class
    {
        var mock = data.AsQueryable().BuildMockDbSet();
        return mock;
    }

    /// <summary>
    /// Sets up HttpContext with authenticated user
    /// </summary>
    protected void SetupAuthenticatedUser(Guid userId, string email, string[] roles)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new(ClaimTypes.Email, email),
            new("email", email)
        };

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);

        var httpContext = new DefaultHttpContext { User = principal };
        HttpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);
    }

    /// <summary>
    /// Creates a test user with specified properties
    /// </summary>
    protected ApplicationUser CreateTestUser(
        Guid? id = null,
        string email = "test@example.com",
        string firstName = "Test",
        string lastName = "User",
        bool emailConfirmed = true,
        bool twoFactorEnabled = false,
        bool isActive = true)
    {
        return new ApplicationUser
        {
            Id = id ?? Guid.NewGuid(),
            Email = email,
            UserName = email,
            FirstName = firstName,
            LastName = lastName,
            EmailConfirmed = emailConfirmed,
            TwoFactorEnabled = twoFactorEnabled,
            IsActive = isActive,
            CreatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Creates a test role with specified properties
    /// </summary>
    protected ApplicationRole CreateTestRole(
        Guid? id = null,
        string code = "TEST",
        string name = "Test Role",
        string? description = null,
        Guid? departmentId = null,
        bool isActive = true,
        bool isDeleted = false)
    {
        return new ApplicationRole
        {
            Id = id ?? Guid.NewGuid(),
            Code = code,
            Name = name,
            Description = description,
            DepartmentId = departmentId,
            IsActive = isActive,
            IsDeleted = isDeleted
        };
    }

    /// <summary>
    /// Creates a test department with specified properties
    /// </summary>
    protected Department CreateTestDepartment(
        Guid? id = null,
        string code = "TEST",
        string name = "Test Department",
        string? description = null,
        bool isActive = true,
        bool isDeleted = false)
    {
        return new Department
        {
            Id = id ?? Guid.NewGuid(),
            Code = code,
            Name = name,
            Description = description,
            IsActive = isActive,
            IsDeleted = isDeleted,
            CreatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Creates a test feature with specified properties
    /// </summary>
    protected Feature CreateTestFeature(
        Guid? id = null,
        string code = "TEST",
        string name = "Test Feature",
        string? description = null,
        bool isMainMenu = true,
        int displayOrder = 1,
        bool isActive = true,
        bool isDeleted = false,
        Guid? parentFeatureId = null)
    {
        return new Feature
        {
            Id = id ?? Guid.NewGuid(),
            Code = code,
            Name = name,
            Description = description,
            IsMainMenu = isMainMenu,
            DisplayOrder = displayOrder,
            IsActive = isActive,
            IsDeleted = isDeleted,
            ParentFeatureId = parentFeatureId,
            Level = parentFeatureId.HasValue ? 1 : 0,
            CreatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Creates a test page with specified properties
    /// </summary>
    protected Page CreateTestPage(
        Guid? id = null,
        string code = "TEST",
        string name = "Test Page",
        string url = "/test",
        string? description = null,
        int displayOrder = 1,
        bool isActive = true,
        bool isDeleted = false)
    {
        return new Page
        {
            Id = id ?? Guid.NewGuid(),
            Code = code,
            Name = name,
            Url = url,
            Description = description,
            DisplayOrder = displayOrder,
            IsActive = isActive,
            IsDeleted = isDeleted,
            CreatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Creates a test permission with specified properties
    /// </summary>
    protected Permission CreateTestPermission(
        Guid? id = null,
        string code = "TEST",
        string name = "Test Permission",
        string? description = null,
        bool isActive = true,
        bool isDeleted = false)
    {
        return new Permission
        {
            Id = id ?? Guid.NewGuid(),
            Code = code,
            Name = name,
            Description = description,
            IsActive = isActive,
            IsDeleted = isDeleted,
            CreatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Creates a test company with specified properties
    /// </summary>
    protected Company CreateTestCompany(
        Guid? id = null,
        string companyCode = "TEST001",
        string legalName = "Test Company Ltd",
        string addressLine1 = "123 Test Street",
        string postalCode = "12345",
        Guid? countryId = null,
        Guid? stateId = null,
        Guid? cityId = null,
        Guid? timeZoneId = null,
        Guid? baseCurrencyId = null,
        bool isDeleted = false)
    {
        return new Company
        {
            Id = id ?? Guid.NewGuid(),
            CompanyCode = companyCode,
            LegalName = legalName,
            AddressLine1 = addressLine1,
            PostalCode = postalCode,
            CountryId = countryId ?? Guid.NewGuid(),
            StateId = stateId ?? Guid.NewGuid(),
            CityId = cityId ?? Guid.NewGuid(),
            TimeZoneId = timeZoneId ?? Guid.NewGuid(),
            BaseCurrencyId = baseCurrencyId ?? Guid.NewGuid(),
            RegistrationCountryId = countryId ?? Guid.NewGuid(),
            BooksStartDate = DateTime.UtcNow.Date,
            Status = CompanyStatus.Active,
            IsDeleted = isDeleted,
            CreatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Creates a test user role mapping
    /// </summary>
    protected UserRoleMapping CreateTestUserRoleMapping(
        Guid? id = null,
        Guid? userId = null,
        Guid? roleId = null,
        Guid? departmentId = null,
        bool isActive = true,
        bool isDeleted = false)
    {
        return new UserRoleMapping
        {
            Id = id ?? Guid.NewGuid(),
            UserId = userId ?? Guid.NewGuid(),
            RoleId = roleId ?? Guid.NewGuid(),
            DepartmentId = departmentId ?? Guid.NewGuid(),
            IsActive = isActive,
            IsDeleted = isDeleted,
            CreatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Creates a test page feature mapping
    /// </summary>
    protected PageFeatureMapping CreateTestPageFeatureMapping(
        Guid? id = null,
        Guid? pageId = null,
        Guid? featureId = null,
        bool isActive = true,
        bool isDeleted = false)
    {
        return new PageFeatureMapping
        {
            Id = id ?? Guid.NewGuid(),
            PageId = pageId ?? Guid.NewGuid(),
            FeatureId = featureId ?? Guid.NewGuid(),
            IsActive = isActive,
            IsDeleted = isDeleted,
            CreatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Creates a test role feature mapping
    /// </summary>
    protected RoleFeatureMapping CreateTestRoleFeatureMapping(
        Guid? id = null,
        Guid? roleId = null,
        Guid? featureId = null,
        Guid? departmentId = null,
        bool isActive = true,
        bool isDeleted = false)
    {
        return new RoleFeatureMapping
        {
            Id = id ?? Guid.NewGuid(),
            RoleId = roleId ?? Guid.NewGuid(),
            FeatureId = featureId ?? Guid.NewGuid(),
            DepartmentId = departmentId,
            IsActive = isActive,
            IsDeleted = isDeleted,
            CreatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Creates a test role page permission mapping
    /// </summary>
    protected RolePagePermissionMapping CreateTestRolePagePermissionMapping(
        Guid? id = null,
        Guid? roleId = null,
        Guid? pageId = null,
        Guid? permissionId = null,
        Guid? departmentId = null,
        bool isActive = true,
        bool isDeleted = false)
    {
        return new RolePagePermissionMapping
        {
            Id = id ?? Guid.NewGuid(),
            RoleId = roleId ?? Guid.NewGuid(),
            PageId = pageId ?? Guid.NewGuid(),
            PermissionId = permissionId ?? Guid.NewGuid(),
            DepartmentId = departmentId,
            IsActive = isActive,
            IsDeleted = isDeleted,
            CreatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Creates a mock logger for the specified type
    /// </summary>
    protected Mock<ILogger<T>> CreateMockLogger<T>()
    {
        return new Mock<ILogger<T>>();
    }
}
