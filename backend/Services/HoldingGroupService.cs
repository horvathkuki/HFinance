using backend.Data;
using backend.Models;
using Microsoft.EntityFrameworkCore;

namespace backend.Services;

public class HoldingGroupService : IHoldingGroupService
{
    public const string UncategorizedName = "Uncategorized";

    private readonly ApplicationDbContext _dbContext;

    public HoldingGroupService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<HoldingGroup> EnsureUncategorizedGroupAsync(string userId, CancellationToken cancellationToken = default)
    {
        var existing = await _dbContext.HoldingGroups
            .FirstOrDefaultAsync(group => group.UserId == userId && group.Name == UncategorizedName, cancellationToken);

        if (existing is not null)
        {
            return existing;
        }

        var group = new HoldingGroup
        {
            UserId = userId,
            Name = UncategorizedName,
            Description = "Default group",
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        };

        _dbContext.HoldingGroups.Add(group);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return group;
    }
}
