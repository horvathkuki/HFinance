using backend.Contracts;
using backend.Contracts.Auth;
using backend.Models;
using backend.Security;
using backend.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

[ApiController]
[Route("api/v1/auth")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IJwtTokenService _jwtTokenService;

    public AuthController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IJwtTokenService jwtTokenService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _jwtTokenService = jwtTokenService;
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request)
    {
        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser is not null)
        {
            return Conflict(new ApiErrorResponse("email_already_exists", "A user with this email already exists.", HttpContext.TraceIdentifier));
        }

        var user = new ApplicationUser
        {
            Email = request.Email,
            UserName = request.DisplayName,
            IsActive = true,
            BaseCurrency = "EUR"
        };

        var createResult = await _userManager.CreateAsync(user, request.Password);
        if (!createResult.Succeeded)
        {
            return BadRequest(new ApiErrorResponse("registration_failed", string.Join("; ", createResult.Errors.Select(e => e.Description)), HttpContext.TraceIdentifier));
        }

        await _userManager.AddToRoleAsync(user, AppRoles.User);
        return Ok(await BuildAuthResponseAsync(user));
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null)
        {
            return Unauthorized(new ApiErrorResponse("invalid_credentials", "Invalid email or password.", HttpContext.TraceIdentifier));
        }

        if (!user.IsActive)
        {
            return Unauthorized(new ApiErrorResponse("account_inactive", "Your account is inactive.", HttpContext.TraceIdentifier));
        }

        var loginResult = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: false);
        if (!loginResult.Succeeded)
        {
            return Unauthorized(new ApiErrorResponse("invalid_credentials", "Invalid email or password.", HttpContext.TraceIdentifier));
        }

        return Ok(await BuildAuthResponseAsync(user));
    }

    private async Task<AuthResponse> BuildAuthResponseAsync(ApplicationUser user)
    {
        var roles = (await _userManager.GetRolesAsync(user)).ToList();
        var (accessToken, expiresAtUtc) = await _jwtTokenService.CreateAccessTokenAsync(user);

        return new AuthResponse
        {
            AccessToken = accessToken,
            ExpiresAtUtc = expiresAtUtc,
            User = UserProfileMapper.ToUserProfileResponse(user, roles)
        };
    }
}
