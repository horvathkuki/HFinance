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
    public DateTime PurchaseDate { get; set; } = DateTime.UtcNow;
    // PortfolioId to link to portfolio
    public int PortfolioId { get; set; }
    public Portfolio? Portfolio { get; set; }
}