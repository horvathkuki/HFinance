namespace backend.Contracts.Snapshots;

public class PortfolioSnapshotDto
{
    public int Id { get; set; }
    public int PortfolioId { get; set; }
    public DateTime CapturedAtUtc { get; set; }
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

public class CreateSnapshotResponse
{
    public PortfolioSnapshotDto Snapshot { get; set; } = new();
}
