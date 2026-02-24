using backend.Contracts.Market;
using Microsoft.Extensions.Caching.Memory;
using YahooFinanceApi;

namespace backend.Services;

public class MarketDataService : IMarketDataService
{
    private readonly IMemoryCache _memoryCache;
    private readonly ILogger<MarketDataService> _logger;

    public MarketDataService(IMemoryCache memoryCache, ILogger<MarketDataService> logger)
    {
        _memoryCache = memoryCache;
        _logger = logger;
    }

    public async Task<QuoteDto?> GetQuoteAsync(string symbol, CancellationToken cancellationToken = default)
    {
        var normalizedSymbol = symbol.Trim().ToUpperInvariant();
        var cacheKey = $"quote:{normalizedSymbol}";
        if (_memoryCache.TryGetValue(cacheKey, out QuoteDto? cachedQuote) && cachedQuote is not null)
        {
            return cachedQuote;
        }

        try
        {
            var securities = await Yahoo.Symbols(normalizedSymbol).Fields(
                Field.Symbol,
                Field.RegularMarketPrice,
                Field.FiftyTwoWeekHigh,
                Field.FiftyTwoWeekLow,
                Field.LongName,
                Field.ShortName,
                Field.MarketCap,
                Field.Currency).QueryAsync();

            if (!securities.TryGetValue(normalizedSymbol, out var quote))
            {
                return null;
            }

            object? GetField(Field field) => quote[field];

            if (!TryGetDecimal(GetField(Field.RegularMarketPrice), out var price))
            {
                return null;
            }

            var dto = new QuoteDto
            {
                Symbol = GetField(Field.Symbol)?.ToString() ?? normalizedSymbol,
                Price = price,
                High52Week = TryGetNullableDecimal(GetField(Field.FiftyTwoWeekHigh)),
                Low52Week = TryGetNullableDecimal(GetField(Field.FiftyTwoWeekLow)),
                CompanyName = GetField(Field.LongName)?.ToString() ?? GetField(Field.ShortName)?.ToString() ?? normalizedSymbol,
                MarketCap = TryGetNullableDecimal(GetField(Field.MarketCap)),
                Currency = GetField(Field.Currency)?.ToString() ?? "USD",
                RetrievedAtUtc = DateTime.UtcNow
            };

            _memoryCache.Set(cacheKey, dto, TimeSpan.FromSeconds(45));
            return dto;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not fetch quote for {Symbol}.", normalizedSymbol);
            return null;
        }
    }

    public async Task<IReadOnlyList<HistoricalBarDto>> GetHistoricalAsync(string symbol, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        var normalizedSymbol = symbol.Trim().ToUpperInvariant();
        var history = await Yahoo.GetHistoricalAsync(normalizedSymbol, startDate, endDate, Period.Daily);
        return history
            .Select(candle => new HistoricalBarDto
            {
                Date = candle.DateTime,
                Open = candle.Open,
                High = candle.High,
                Low = candle.Low,
                Close = candle.Close,
                AdjustedClose = candle.AdjustedClose,
                Volume = candle.Volume
            })
            .OrderBy(point => point.Date)
            .ToList();
    }

    private static bool TryGetDecimal(object? rawValue, out decimal value)
    {
        value = 0;
        return rawValue is not null && decimal.TryParse(rawValue.ToString(), out value);
    }

    private static decimal? TryGetNullableDecimal(object? rawValue)
    {
        return rawValue is not null && decimal.TryParse(rawValue.ToString(), out var parsed)
            ? parsed
            : null;
    }
}
