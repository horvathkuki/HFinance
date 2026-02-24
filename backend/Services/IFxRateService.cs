using backend.Contracts.Market;

namespace backend.Services;

public interface IFxRateService
{
    Task<FxRateSnapshot> GetRatesAsync(CancellationToken cancellationToken = default);
    decimal Convert(decimal amount, string fromCurrency, string toCurrency, FxRateSnapshot rates);
}
