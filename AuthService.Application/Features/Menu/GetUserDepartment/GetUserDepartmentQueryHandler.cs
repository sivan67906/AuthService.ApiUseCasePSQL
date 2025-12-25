using AuthService.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace AuthService.Application.Features.Menu.GetUserDepartment;

public class GetUserDepartmentQueryHandler : IRequestHandler<GetUserDepartmentQuery, Guid?>
{
    private readonly IUserAuthorizationService _authorizationService;
    private readonly ILogger<GetUserDepartmentQueryHandler> _logger;

    public GetUserDepartmentQueryHandler(
        IUserAuthorizationService authorizationService,
        ILogger<GetUserDepartmentQueryHandler> logger)
    {
        _authorizationService = authorizationService;
        _logger = logger;
    }

    public async Task<Guid?> Handle(GetUserDepartmentQuery request, CancellationToken cancellationToken)
    {
        try
        {
            return await _authorizationService.GetUserDepartmentAsync(request.UserId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user department for user: {UserId}", request.UserId);
            throw new InvalidOperationException("An error occurred while retrieving user department");
        }
    }
}
