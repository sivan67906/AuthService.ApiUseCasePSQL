using Microsoft.AspNetCore.Identity;
using AuthService.Application.Features.Role.CreateRole;

namespace AuthService.Application.Features.Role.GetRoleById;
public sealed class GetRoleByIdQueryHandler : IRequestHandler<GetRoleByIdQuery, RoleDto>
{
    private readonly RoleManager<ApplicationRole> _roleManager;
    public GetRoleByIdQueryHandler(RoleManager<ApplicationRole> roleManager)
    {
        _roleManager = roleManager;
    }
    public async Task<RoleDto> Handle(GetRoleByIdQuery request, CancellationToken cancellationToken)
    {
        var role = await _roleManager.FindByIdAsync(request.RoleId.ToString());
        if (role == null)
        {
            throw new InvalidOperationException($"Role with ID '{request.RoleId}' not found");
        }
        return new RoleDto(
            role.Id,
            role.Name!,
            role.Description,
            role.DepartmentId,
            null
        );
}
}
