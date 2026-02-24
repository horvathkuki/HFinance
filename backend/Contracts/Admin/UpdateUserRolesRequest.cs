using System.ComponentModel.DataAnnotations;

namespace backend.Contracts.Admin;

public class UpdateUserRolesRequest
{
    [Required]
    public List<string> Roles { get; set; } = new();
}
