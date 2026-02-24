namespace backend.Contracts.Market;

public class QuoteDto
{
    public string Symbol { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public decimal? High52Week { get; set; }
    public decimal? Low52Week { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public decimal? MarketCap { get; set; }
    public string Currency { get; set; } = "USD";
    public DateTime RetrievedAtUtc { get; set; }
}

public class HistoricalBarDto
{
    public DateTime Date { get; set; }
    public decimal Open { get; set; }
    public decimal High { get; set; }
    public decimal Low { get; set; }
    public decimal Close { get; set; }
    public decimal AdjustedClose { get; set; }
    public long Volume { get; set; }
}
