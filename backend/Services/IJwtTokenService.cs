using backend.Models;

namespace backend.Services;

public interface IJwtTokenService
{
    Task<(string AccessToken, DateTime ExpiresAtUtc)> CreateAccessTokenAsync(ApplicationUser user);
}
