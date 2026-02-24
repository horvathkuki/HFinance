using backend.Contracts;
using backend.Contracts.Account;
using backend.Contracts.Auth;
using backend.Models;
using backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/account")]
public class AccountController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;

    public AccountController(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    [HttpGet("me")]
    public async Task<ActionResult<UserProfileResponse>> GetMe()
    {
        var user = await GetCurrentUserAsync();
        if (user is null)
        {
            return Unauthorized(new ApiErrorResponse("unauthorized", "User session is invalid.", HttpContext.TraceIdentifier));
        }

        var roles = (await _userManager.GetRolesAsync(user)).ToList();
        return Ok(UserProfileMapper.ToUserProfileResponse(user, roles));
    }

    [HttpPut("me")]
    public async Task<ActionResult<UserProfileResponse>> UpdateMe(UpdateMyProfileRequest request)
    {
        var user = await GetCurrentUserAsync();
        if (user is null)
        {
            return Unauthorized(new ApiErrorResponse("unauthorized", "User session is invalid.", HttpContext.TraceIdentifier));
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
        return Ok(UserProfileMapper.ToUserProfileResponse(user, roles));
    }

    [HttpPut("password")]
    public async Task<IActionResult> ChangePassword(ChangePasswordRequest request)
    {
        var user = await GetCurrentUserAsync();
        if (user is null)
        {
            return Unauthorized(new ApiErrorResponse("unauthorized", "User session is invalid.", HttpContext.TraceIdentifier));
        }

        var result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
        if (!result.Succeeded)
        {
            return BadRequest(new ApiErrorResponse("password_change_failed", string.Join("; ", result.Errors.Select(e => e.Description)), HttpContext.TraceIdentifier));
        }

        return NoContent();
    }

    private async Task<ApplicationUser?> GetCurrentUserAsync()
    {
        var userId = User.GetUserId();
        if (string.IsNullOrWhiteSpace(userId))
        {
            return null;
        }

        return await _userManager.FindByIdAsync(userId);
    }
}
