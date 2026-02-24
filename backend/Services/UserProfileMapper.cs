using backend.Contracts.Admin;
using backend.Contracts.Auth;
using backend.Models;

namespace backend.Services;

public static class UserProfileMapper
{
    public static UserProfileResponse ToUserProfileResponse(ApplicationUser user, IReadOnlyList<string> roles)
    {
        return new UserProfileResponse
        {
            Id = user.Id,
            Email = user.Email ?? string.Empty,
            DisplayName = user.UserName ?? string.Empty,
            BaseCurrency = user.BaseCurrency,
            IsActive = user.IsActive,
            Roles = roles
        };
    }

    public static AdminUserDto ToAdminUserDto(ApplicationUser user, IReadOnlyList<string> roles)
    {
        return new AdminUserDto
        {
            Id = user.Id,
            Email = user.Email ?? string.Empty,
            DisplayName = user.UserName ?? string.Empty,
            BaseCurrency = user.BaseCurrency,
            IsActive = user.IsActive,
            Roles = roles
        };
    }
}
