using backend.Contracts.Snapshots;
using backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/portfolios/{portfolioId:int}/snapshots")]
public class SnapshotsController : ControllerBase
{
    private readonly IPortfolioAnalyticsService _portfolioAnalyticsService;

    public SnapshotsController(IPortfolioAnalyticsService portfolioAnalyticsService)
    {
        _portfolioAnalyticsService = portfolioAnalyticsService;
    }

    [HttpPost]
    public async Task<ActionResult<CreateSnapshotResponse>> CreateSnapshot(int portfolioId, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }

        var snapshot = await _portfolioAnalyticsService.CreateSnapshotAsync(userId, portfolioId, cancellationToken);
        if (snapshot is null)
        {
            return NotFound();
        }

        return Ok(new CreateSnapshotResponse { Snapshot = snapshot });
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<PortfolioSnapshotDto>>> GetSnapshots(
        int portfolioId,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }

        var snapshots = await _portfolioAnalyticsService.GetSnapshotsAsync(userId, portfolioId, from, to, cancellationToken);
        return Ok(snapshots);
    }
}
