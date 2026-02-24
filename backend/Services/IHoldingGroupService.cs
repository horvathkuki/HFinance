using backend.Models;

namespace backend.Services;

public interface IHoldingGroupService
{
    Task<HoldingGroup> EnsureUncategorizedGroupAsync(string userId, CancellationToken cancellationToken = default);
}
