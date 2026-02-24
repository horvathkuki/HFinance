using System.Xml.Linq;
using backend.Contracts.Market;
using Microsoft.Extensions.Caching.Memory;

namespace backend.Services;

public class EcbFxRateService : IFxRateService
{
    private const string CacheKey = "fx:ecb:eurusd_eurron";
    private const string EcbUrl = "https://www.ecb.europa.eu/stats/eurofxref/eurofxref-daily.xml";

    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _memoryCache;
    private readonly ILogger<EcbFxRateService> _logger;

    public EcbFxRateService(HttpClient httpClient, IMemoryCache memoryCache, ILogger<EcbFxRateService> logger)
    {
        _httpClient = httpClient;
        _memoryCache = memoryCache;
        _logger = logger;
    }

    public async Task<FxRateSnapshot> GetRatesAsync(CancellationToken cancellationToken = default)
    {
        if (_memoryCache.TryGetValue(CacheKey, out FxRateSnapshot? cached) && cached is not null)
        {
            return cached;
        }

        try
        {
            var xml = await _httpClient.GetStringAsync(EcbUrl, cancellationToken);
            var rates = ParseRates(xml);
            _memoryCache.Set(CacheKey, rates, TimeSpan.FromMinutes(30));
            _memoryCache.Set($"{CacheKey}:last_known_good", rates, TimeSpan.FromDays(2));
            return rates;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not fetch ECB FX rates, trying last known good cache.");
            if (_memoryCache.TryGetValue($"{CacheKey}:last_known_good", out FxRateSnapshot? lastKnownGood) && lastKnownGood is not null)
            {
                return lastKnownGood;
            }

            throw new InvalidOperationException("Could not fetch FX rates and no cached fallback exists.");
        }
    }

    public decimal Convert(decimal amount, string fromCurrency, string toCurrency, FxRateSnapshot rates)
    {
        var from = CurrencyHelper.NormalizeCurrency(fromCurrency);
        var to = CurrencyHelper.NormalizeCurrency(toCurrency);
        if (from == to)
        {
            return amount;
        }

        var amountInEur = from switch
        {
            "EUR" => amount,
            "USD" => amount / rates.EurUsd,
            "RON" => amount / rates.EurRon,
            _ => throw new InvalidOperationException($"Unsupported currency '{from}'.")
        };

        var converted = to switch
        {
            "EUR" => amountInEur,
            "USD" => amountInEur * rates.EurUsd,
            "RON" => amountInEur * rates.EurRon,
            _ => throw new InvalidOperationException($"Unsupported currency '{to}'.")
        };

        return decimal.Round(converted, 4, MidpointRounding.AwayFromZero);
    }

    private static FxRateSnapshot ParseRates(string xml)
    {
        var document = XDocument.Parse(xml);
        var cubes = document.Descendants().Where(element => element.Name.LocalName == "Cube");

        decimal? eurUsd = null;
        decimal? eurRon = null;
        DateTime? date = null;

        foreach (var cube in cubes)
        {
            var timeAttribute = cube.Attribute("time")?.Value;
            if (!string.IsNullOrWhiteSpace(timeAttribute) && DateTime.TryParse(timeAttribute, out var parsedDate))
            {
                date = parsedDate;
            }

            var currency = cube.Attribute("currency")?.Value;
            var rateRaw = cube.Attribute("rate")?.Value;
            if (string.IsNullOrWhiteSpace(currency) || string.IsNullOrWhiteSpace(rateRaw))
            {
                continue;
            }

            if (!decimal.TryParse(rateRaw, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var rate))
            {
                continue;
            }

            if (currency.Equals("USD", StringComparison.OrdinalIgnoreCase))
            {
                eurUsd = rate;
            }
            else if (currency.Equals("RON", StringComparison.OrdinalIgnoreCase))
            {
                eurRon = rate;
            }
        }

        if (eurUsd is null || eurRon is null)
        {
            throw new InvalidOperationException("ECB response does not contain USD and RON rates.");
        }

        return new FxRateSnapshot
        {
            EurUsd = eurUsd.Value,
            EurRon = eurRon.Value,
            RetrievedAtUtc = date?.Date ?? DateTime.UtcNow
        };
    }
}
