using System.Security.Claims;
using AuthService.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace AuthService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MenuController(
    IUserAuthorizationService authorizationService,
    ILogger<MenuController> logger) : ControllerBase
{
    private readonly IUserAuthorizationService _authorizationService = authorizationService;
    private readonly ILogger<MenuController> _logger = logger;

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

            var menus = await _authorizationService.GetUserMenusAsync(userId);
            return Ok(ApiResponse<List<MenuItemDto>>.SuccessResponse(menus, "Menus retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user menus");
            return StatusCode(500, ApiResponse<List<MenuItemDto>>.ErrorResponse("An error occurred while retrieving menus"));
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

            var hasAccess = await _authorizationService.UserHasAccessToPageAsync(userId, pageName);
            return Ok(ApiResponse<bool>.SuccessResponse(hasAccess, $"Access check completed for page: {pageName}"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking page access for page: {PageName}", pageName);
            return StatusCode(500, ApiResponse<bool>.ErrorResponse("An error occurred while checking page access"));
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

            var hasPermission = await _authorizationService.UserHasPermissionAsync(userId, permissionName);
            return Ok(ApiResponse<bool>.SuccessResponse(hasPermission, $"Permission check completed for: {permissionName}"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking permission: {PermissionName}", permissionName);
            return StatusCode(500, ApiResponse<bool>.ErrorResponse("An error occurred while checking permission"));
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

            var roles = await _authorizationService.GetUserRolesAsync(userId);
            return Ok(ApiResponse<List<string>>.SuccessResponse(roles, "User roles retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user roles");
            return StatusCode(500, ApiResponse<List<string>>.ErrorResponse("An error occurred while retrieving user roles"));
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

            var departmentId = await _authorizationService.GetUserDepartmentAsync(userId);
            return Ok(ApiResponse<Guid?>.SuccessResponse(departmentId, "User department retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user department");
            return StatusCode(500, ApiResponse<Guid?>.ErrorResponse("An error occurred while retrieving user department"));
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

            var permissions = await _authorizationService.GetUserPagePermissionsAsync(userId, pageName);
            
            var result = new PagePermissionsDto
            {
                PageName = pageName,
                Permissions = permissions,
                CanCreate = permissions.Contains("Create"),
                CanView = permissions.Contains("View"),
                CanUpdate = permissions.Contains("Update"),
                CanDelete = permissions.Contains("Delete")
            };

            return Ok(ApiResponse<PagePermissionsDto>.SuccessResponse(result, $"Page permissions retrieved for: {pageName}"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving page permissions for: {PageName}", pageName);
            return StatusCode(500, ApiResponse<PagePermissionsDto>.ErrorResponse("An error occurred while retrieving page permissions"));
        }
    }
}

public class PagePermissionsDto
{
    public string PageName { get; set; } = string.Empty;
    public List<string> Permissions { get; set; } = [];
    public bool CanCreate { get; set; }
    public bool CanView { get; set; }
    public bool CanUpdate { get; set; }
    public bool CanDelete { get; set; }
}