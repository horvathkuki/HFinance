using System.ComponentModel.DataAnnotations;

namespace backend.Contracts.Admin;

public class UpdateUserStatusRequest
{
    [Required]
    public bool IsActive { get; set; }
}
