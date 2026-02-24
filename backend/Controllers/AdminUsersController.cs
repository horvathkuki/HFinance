using backend.Contracts;
using backend.Contracts.Admin;
using backend.Models;
using backend.Security;
using backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backend.Controllers;

[ApiController]
[Authorize(Policy = "AdminOnly")]
[Route("api/v1/admin/users")]
public class AdminUsersController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;

    public AdminUsersController(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<ActionResult<AdminUserListResponse>> GetUsers([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string? search = null)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var query = _userManager.Users.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(u =>
                (u.Email != null && u.Email.Contains(search)) ||
                (u.UserName != null && u.UserName.Contains(search)));
        }

        var totalCount = await query.CountAsync();
        var users = await query
            .OrderBy(u => u.Email)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var items = new List<AdminUserDto>();
        foreach (var user in users)
        {
            var roles = (await _userManager.GetRolesAsync(user)).ToList();
            items.Add(UserProfileMapper.ToAdminUserDto(user, roles));
        }

        return Ok(new AdminUserListResponse
        {
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            Items = items
        });
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<AdminUserDto>> GetUser(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user is null)
        {
            return NotFound(new ApiErrorResponse("user_not_found", "User not found.", HttpContext.TraceIdentifier));
        }

        var roles = (await _userManager.GetRolesAsync(user)).ToList();
        return Ok(UserProfileMapper.ToAdminUserDto(user, roles));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<AdminUserDto>> UpdateUser(string id, UpdateUserRequest request)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user is null)
        {
            return NotFound(new ApiErrorResponse("user_not_found", "User not found.", HttpContext.TraceIdentifier));
        }

        if (!string.Equals(user.Email, request.Email, StringComparison.OrdinalIgnoreCase))
        {
            var existingUser = await _userManager.FindByEmailAsync(request.Email);
            if (existingUser is not null && existingUser.Id != user.Id)
            {
                return Conflict(new ApiErrorResponse("email_already_exists", "A user with this email already exists.", HttpContext.TraceIdentifier));
            }

            user.Email = request.Email;
            user.NormalizedEmail = _userManager.NormalizeEmail(request.Email);
        }

        user.UserName = request.DisplayName;
        user.BaseCurrency = request.BaseCurrency.ToUpperInvariant();

        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            return BadRequest(new ApiErrorResponse("update_failed", string.Join("; ", updateResult.Errors.Select(e => e.Description)), HttpContext.TraceIdentifier));
        }

        var roles = (await _userManager.GetRolesAsync(user)).ToList();
        return Ok(UserProfileMapper.ToAdminUserDto(user, roles));
    }

    [HttpPut("{id}/status")]
    public async Task<ActionResult<AdminUserDto>> UpdateUserStatus(string id, UpdateUserStatusRequest request)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user is null)
        {
            return NotFound(new ApiErrorResponse("user_not_found", "User not found.", HttpContext.TraceIdentifier));
        }

        user.IsActive = request.IsActive;
        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            return BadRequest(new ApiErrorResponse("update_failed", string.Join("; ", updateResult.Errors.Select(e => e.Description)), HttpContext.TraceIdentifier));
        }

        var roles = (await _userManager.GetRolesAsync(user)).ToList();
        return Ok(UserProfileMapper.ToAdminUserDto(user, roles));
    }

    [HttpPut("{id}/roles")]
    public async Task<ActionResult<AdminUserDto>> UpdateUserRoles(string id, UpdateUserRolesRequest request)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user is null)
        {
            return NotFound(new ApiErrorResponse("user_not_found", "User not found.", HttpContext.TraceIdentifier));
        }

        var normalizedRequestedRoles = request.Roles
            .Where(role => !string.IsNullOrWhiteSpace(role))
            .Select(role => role.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (normalizedRequestedRoles.Count == 0)
        {
            return BadRequest(new ApiErrorResponse("roles_required", "At least one role must be specified.", HttpContext.TraceIdentifier));
        }

        var invalidRole = normalizedRequestedRoles.FirstOrDefault(role => !AppRoles.All.Contains(role, StringComparer.OrdinalIgnoreCase));
        if (invalidRole is not null)
        {
            return BadRequest(new ApiErrorResponse("invalid_role", $"Role '{invalidRole}' is not supported.", HttpContext.TraceIdentifier));
        }

        var currentRoles = await _userManager.GetRolesAsync(user);
        var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
        if (!removeResult.Succeeded)
        {
            return BadRequest(new ApiErrorResponse("roles_update_failed", string.Join("; ", removeResult.Errors.Select(e => e.Description)), HttpContext.TraceIdentifier));
        }

        var addResult = await _userManager.AddToRolesAsync(user, normalizedRequestedRoles);
        if (!addResult.Succeeded)
        {
            return BadRequest(new ApiErrorResponse("roles_update_failed", string.Join("; ", addResult.Errors.Select(e => e.Description)), HttpContext.TraceIdentifier));
        }

        var finalRoles = (await _userManager.GetRolesAsync(user)).ToList();
        return Ok(UserProfileMapper.ToAdminUserDto(user, finalRoles));
    }

    [HttpPost("{id}/reset-password")]
    public async Task<ActionResult<AdminResetPasswordResponse>> GenerateResetPasswordToken(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user is null)
        {
            return NotFound(new ApiErrorResponse("user_not_found", "User not found.", HttpContext.TraceIdentifier));
        }

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        return Ok(new AdminResetPasswordResponse
        {
            UserId = id,
            ResetToken = token
        });
    }

    [HttpPost("{id}/reset-password/confirm")]
    public async Task<IActionResult> ConfirmResetPassword(string id, AdminResetPasswordConfirmRequest request)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user is null)
        {
            return NotFound(new ApiErrorResponse("user_not_found", "User not found.", HttpContext.TraceIdentifier));
        }

        var result = await _userManager.ResetPasswordAsync(user, request.ResetToken, request.NewPassword);
        if (!result.Succeeded)
        {
            return BadRequest(new ApiErrorResponse("password_reset_failed", string.Join("; ", result.Errors.Select(e => e.Description)), HttpContext.TraceIdentifier));
        }

        return NoContent();
    }
}
