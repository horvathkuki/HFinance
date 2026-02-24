using System.ComponentModel.DataAnnotations;

namespace backend.Contracts.Holdings;

public class HoldingDto
{
    public int Id { get; set; }
    public int PortfolioId { get; set; }
    public string Symbol { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal AveragePurchasePrice { get; set; }
    public string Currency { get; set; } = "USD";
    public DateTime PurchaseDate { get; set; }
    public int GroupId { get; set; }
    public string GroupName { get; set; } = string.Empty;
}

public class CreateHoldingRequest
{
    [Required]
    [MinLength(1)]
    [MaxLength(20)]
    public string Symbol { get; set; } = string.Empty;

    [Range(typeof(decimal), "0.0001", "1000000000")]
    public decimal Quantity { get; set; }

    [Range(typeof(decimal), "0.01", "1000000000")]
    public decimal AveragePurchasePrice { get; set; }

    [Required]
    [RegularExpression("^(EUR|USD|RON)$", ErrorMessage = "Currency must be EUR, USD or RON.")]
    public string Currency { get; set; } = "USD";

    public DateTime? PurchaseDate { get; set; }
    public int GroupId { get; set; }
}

public class UpdateHoldingRequest : CreateHoldingRequest
{
}
