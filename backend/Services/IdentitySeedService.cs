using backend.Configuration;
using backend.Models;
using backend.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace backend.Services;

public class IdentitySeedService
{
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SeedAdminOptions _seedAdminOptions;
    private readonly ILogger<IdentitySeedService> _logger;

    public IdentitySeedService(
        RoleManager<IdentityRole> roleManager,
        UserManager<ApplicationUser> userManager,
        IOptions<SeedAdminOptions> seedAdminOptions,
        ILogger<IdentitySeedService> logger)
    {
        _roleManager = roleManager;
        _userManager = userManager;
        _seedAdminOptions = seedAdminOptions.Value;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        foreach (var roleName in AppRoles.All)
        {
            if (!await _roleManager.RoleExistsAsync(roleName))
            {
                await _roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }

        if (!_seedAdminOptions.Enabled)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(_seedAdminOptions.Email) || string.IsNullOrWhiteSpace(_seedAdminOptions.Password))
        {
            _logger.LogWarning("SeedAdmin is enabled but email/password are missing. Admin seed skipped.");
            return;
        }

        var existingUser = await _userManager.FindByEmailAsync(_seedAdminOptions.Email);
        if (existingUser is null)
        {
            var adminUser = new ApplicationUser
            {
                Email = _seedAdminOptions.Email,
                UserName = "Admin",
                IsActive = true,
                BaseCurrency = "EUR"
            };

            var createResult = await _userManager.CreateAsync(adminUser, _seedAdminOptions.Password);
            if (!createResult.Succeeded)
            {
                _logger.LogWarning("Could not create seed admin user: {Errors}", string.Join("; ", createResult.Errors.Select(e => e.Description)));
                return;
            }

            existingUser = adminUser;
        }

        if (!await _userManager.IsInRoleAsync(existingUser, AppRoles.Admin))
        {
            await _userManager.AddToRoleAsync(existingUser, AppRoles.Admin);
        }

        if (!await _userManager.IsInRoleAsync(existingUser, AppRoles.User))
        {
            await _userManager.AddToRoleAsync(existingUser, AppRoles.User);
        }
    }
}
