using System.Security.Claims;
using AuthService.Application.Common.Interfaces;
using AuthService.Application.Features.Menu.CheckPageAccess;
using AuthService.Application.Features.Menu.CheckPermission;
using AuthService.Application.Features.Menu.GetPagePermissions;
using AuthService.Application.Features.Menu.GetUserDepartment;
using AuthService.Application.Features.Menu.GetUserMenus;
using AuthService.Application.Features.Menu.GetUserRoles;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuthService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MenuController : ControllerBase
{
    private readonly IMediator _mediator;

    public MenuController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get menu structure for the current logged-in user.
    /// Returns hierarchical menu structure based on user's role and permissions.
    /// </summary>
    [HttpGet("user-menus")]
    public async Task<ActionResult<ApiResponse<List<MenuItemDto>>>> GetUserMenus()
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(ApiResponse<List<MenuItemDto>>.ErrorResponse("User not authenticated"));
            }

            var menus = await _mediator.Send(new GetUserMenusQuery(userId));
            return Ok(ApiResponse<List<MenuItemDto>>.SuccessResponse(menus, "Menus retrieved successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<List<MenuItemDto>>.FailFromException("An error occurred while retrieving menus", ex));
        }
    }

    /// <summary>
    /// Check if user has access to a specific page.
    /// </summary>
    [HttpGet("check-page-access/{pageName}")]
    public async Task<ActionResult<ApiResponse<bool>>> CheckPageAccess(string pageName)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(ApiResponse<bool>.ErrorResponse("User not authenticated"));
            }

            var hasAccess = await _mediator.Send(new CheckPageAccessQuery(userId, pageName));
            return Ok(ApiResponse<bool>.SuccessResponse(hasAccess, $"Access check completed for page: {pageName}"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<bool>.FailFromException($"An error occurred while checking page access for: {pageName}", ex));
        }
    }

    /// <summary>
    /// Check if user has a specific permission.
    /// </summary>
    [HttpGet("check-permission/{permissionName}")]
    public async Task<ActionResult<ApiResponse<bool>>> CheckPermission(string permissionName)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(ApiResponse<bool>.ErrorResponse("User not authenticated"));
            }

            var hasPermission = await _mediator.Send(new CheckPermissionQuery(userId, permissionName));
            return Ok(ApiResponse<bool>.SuccessResponse(hasPermission, $"Permission check completed for: {permissionName}"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<bool>.FailFromException($"An error occurred while checking permission: {permissionName}", ex));
        }
    }

    /// <summary>
    /// Get user roles.
    /// </summary>
    [HttpGet("user-roles")]
    public async Task<ActionResult<ApiResponse<List<string>>>> GetUserRoles()
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(ApiResponse<List<string>>.ErrorResponse("User not authenticated"));
            }

            var roles = await _mediator.Send(new GetUserRolesQuery(userId));
            return Ok(ApiResponse<List<string>>.SuccessResponse(roles, "User roles retrieved successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<List<string>>.FailFromException("An error occurred while retrieving user roles", ex));
        }
    }

    /// <summary>
    /// Get user department.
    /// </summary>
    [HttpGet("user-department")]
    public async Task<ActionResult<ApiResponse<Guid?>>> GetUserDepartment()
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(ApiResponse<Guid?>.ErrorResponse("User not authenticated"));
            }

            var departmentId = await _mediator.Send(new GetUserDepartmentQuery(userId));
            return Ok(ApiResponse<Guid?>.SuccessResponse(departmentId, "User department retrieved successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<Guid?>.FailFromException("An error occurred while retrieving user department", ex));
        }
    }

    /// <summary>
    /// Get all permissions user has for a specific page.
    /// Returns list of permission names (e.g., ["View"], ["View", "Create", "Update", "Delete"]).
    /// Frontend can use this to show/hide action buttons.
    /// </summary>
    [HttpGet("page-permissions/{pageName}")]
    public async Task<ActionResult<ApiResponse<PagePermissionsDto>>> GetPagePermissions(string pageName)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(ApiResponse<PagePermissionsDto>.ErrorResponse("User not authenticated"));
            }

            var permissions = await _mediator.Send(new GetPagePermissionsQuery(userId, pageName));
            return Ok(ApiResponse<PagePermissionsDto>.SuccessResponse(permissions, $"Page permissions retrieved for: {pageName}"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<PagePermissionsDto>.FailFromException($"An error occurred while retrieving page permissions for: {pageName}", ex));
        }
    }
}