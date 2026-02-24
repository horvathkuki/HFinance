using System.ComponentModel.DataAnnotations;

namespace backend.Contracts.Admin;

public class UpdateUserRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(2)]
    [MaxLength(100)]
    public string DisplayName { get; set; } = string.Empty;

    [Required]
    [RegularExpression("^(EUR|USD|RON)$", ErrorMessage = "BaseCurrency must be EUR, USD, or RON.")]
    public string BaseCurrency { get; set; } = "EUR";
}
