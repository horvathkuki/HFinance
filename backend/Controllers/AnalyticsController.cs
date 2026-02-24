using backend.Contracts.Analytics;
using backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/analytics/portfolio")]
public class AnalyticsController : ControllerBase
{
    private readonly IPortfolioAnalyticsService _portfolioAnalyticsService;

    public AnalyticsController(IPortfolioAnalyticsService portfolioAnalyticsService)
    {
        _portfolioAnalyticsService = portfolioAnalyticsService;
    }

    [HttpGet("{portfolioId:int}")]
    public async Task<ActionResult<PortfolioAnalyticsDto>> GetPortfolioAnalytics(int portfolioId, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }

        var analytics = await _portfolioAnalyticsService.GetPortfolioAnalyticsAsync(userId, portfolioId, cancellationToken);
        if (analytics is null)
        {
            return NotFound();
        }

        return Ok(analytics);
    }

    [HttpGet("{portfolioId:int}/timeseries")]
    public async Task<ActionResult<IReadOnlyList<PortfolioTimeSeriesPointDto>>> GetTimeSeries(
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

        var result = await _portfolioAnalyticsService.GetTimeSeriesAsync(userId, portfolioId, from, to, cancellationToken);
        return Ok(result);
    }
}
