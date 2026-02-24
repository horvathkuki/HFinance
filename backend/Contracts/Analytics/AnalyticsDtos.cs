namespace backend.Contracts.Analytics;

public class PositionAnalyticsDto
{
    public int HoldingId { get; set; }
    public string Symbol { get; set; } = string.Empty;
    public string HoldingCurrency { get; set; } = "USD";
    public string GroupName { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal AveragePurchasePrice { get; set; }
    public decimal CurrentPrice { get; set; }
    public decimal MarketValueBase { get; set; }
    public decimal CostBasisBase { get; set; }
    public decimal UnrealizedPnLBase { get; set; }
    public decimal UnrealizedPnLPercent { get; set; }
}

public class GroupAllocationDto
{
    public int GroupId { get; set; }
    public string GroupName { get; set; } = string.Empty;
    public decimal MarketValueBase { get; set; }
    public decimal AllocationPercent { get; set; }
}

public class CurrencyExposureDto
{
    public string Currency { get; set; } = "USD";
    public decimal MarketValueBase { get; set; }
    public decimal AllocationPercent { get; set; }
}

public class PortfolioAnalyticsDto
{
    public int PortfolioId { get; set; }
    public string BaseCurrency { get; set; } = "EUR";
    public decimal TotalMarketValueBase { get; set; }
    public decimal TotalCostBasisBase { get; set; }
    public decimal TotalUnrealizedPnLBase { get; set; }
    public decimal TotalUnrealizedPnLPercent { get; set; }
    public bool IsPartial { get; set; }
    public int MissingSymbolsCount { get; set; }
    public IReadOnlyList<PositionAnalyticsDto> Positions { get; set; } = Array.Empty<PositionAnalyticsDto>();
    public IReadOnlyList<GroupAllocationDto> GroupAllocations { get; set; } = Array.Empty<GroupAllocationDto>();
    public IReadOnlyList<CurrencyExposureDto> CurrencyExposures { get; set; } = Array.Empty<CurrencyExposureDto>();
}

public class PortfolioTimeSeriesPointDto
{
    public DateTime CapturedAtUtc { get; set; }
    public decimal TotalMarketValueBase { get; set; }
    public decimal TotalCostBasisBase { get; set; }
    public decimal TotalUnrealizedPnLBase { get; set; }
    public bool IsPartial { get; set; }
}
