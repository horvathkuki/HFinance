using backend.Contracts.Market;

namespace backend.Services;

public interface IMarketDataService
{
    Task<QuoteDto?> GetQuoteAsync(string symbol, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<HistoricalBarDto>> GetHistoricalAsync(string symbol, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
}
