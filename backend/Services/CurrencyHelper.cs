namespace backend.Services;

public static class CurrencyHelper
{
    public static readonly HashSet<string> AllowedCurrencies = new(StringComparer.OrdinalIgnoreCase)
    {
        "EUR",
        "USD",
        "RON"
    };

    public static string NormalizeCurrency(string currency)
    {
        return currency.Trim().ToUpperInvariant();
    }
}
