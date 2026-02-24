using System.ComponentModel.DataAnnotations;

namespace backend.Contracts.Admin;

public class AdminResetPasswordConfirmRequest
{
    [Required]
    public string ResetToken { get; set; } = string.Empty;

    [Required]
    [MinLength(8)]
    public string NewPassword { get; set; } = string.Empty;
}
