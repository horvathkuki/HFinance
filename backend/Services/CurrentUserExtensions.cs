using System.Security.Claims;

namespace backend.Services;

public static class CurrentUserExtensions
{
    public static string? GetUserId(this ClaimsPrincipal principal)
    {
        return principal.FindFirstValue(ClaimTypes.NameIdentifier);
    }
}
