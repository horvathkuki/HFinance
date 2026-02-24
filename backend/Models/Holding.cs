using System.ComponentModel.DataAnnotations;

namespace backend.Models;

public class Holding
{
    public int Id { get; set; }

    [Required]
    public string Symbol { get; set; } = string.Empty;

    [Required]
    public decimal Quantity { get; set; }

    [Required]
    public decimal AveragePurchasePrice { get; set; }

    [Required]
    public string Currency { get; set; } = "USD";

    public DateTime PurchaseDate { get; set; } = DateTime.UtcNow;

    public int PortfolioId { get; set; }
    public Portfolio? Portfolio { get; set; }

    public int GroupId { get; set; }
    public HoldingGroup? Group { get; set; }
}
