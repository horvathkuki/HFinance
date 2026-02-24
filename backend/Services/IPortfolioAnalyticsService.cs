using backend.Contracts.Analytics;
using backend.Contracts.Snapshots;

namespace backend.Services;

public interface IPortfolioAnalyticsService
{
    Task<PortfolioAnalyticsDto?> GetPortfolioAnalyticsAsync(string userId, int portfolioId, CancellationToken cancellationToken = default);
    Task<PortfolioSnapshotDto?> CreateSnapshotAsync(string userId, int portfolioId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PortfolioSnapshotDto>> GetSnapshotsAsync(string userId, int portfolioId, DateTime? from, DateTime? to, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PortfolioTimeSeriesPointDto>> GetTimeSeriesAsync(string userId, int portfolioId, DateTime? from, DateTime? to, CancellationToken cancellationToken = default);
}
