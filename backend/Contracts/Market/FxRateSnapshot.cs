namespace backend.Contracts.Market;

public class FxRateSnapshot
{
    public DateTime RetrievedAtUtc { get; set; }
    public decimal EurUsd { get; set; }
    public decimal EurRon { get; set; }
}
