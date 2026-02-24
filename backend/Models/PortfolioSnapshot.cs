namespace backend.Models;

public class PortfolioSnapshot
{
    public int Id { get; set; }
    public int PortfolioId { get; set; }
    public Portfolio? Portfolio { get; set; }

    public string UserId { get; set; } = string.Empty;
    public DateTime CapturedAtUtc { get; set; } = DateTime.UtcNow;

    public string BaseCurrency { get; set; } = "EUR";
    public decimal TotalMarketValueBase { get; set; }
    public decimal TotalCostBasisBase { get; set; }
    public decimal TotalUnrealizedPnLBase { get; set; }

    public bool IsPartial { get; set; }
    public int MissingSymbolsCount { get; set; }

    public DateTime FxTimestampUtc { get; set; }
    public decimal EurUsdRate { get; set; }
    public decimal EurRonRate { get; set; }
}
