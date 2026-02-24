using Microsoft.AspNetCore.Identity;

namespace backend.Models;

public class ApplicationUser : IdentityUser
{
    public bool IsActive { get; set; } = true;
    public string BaseCurrency { get; set; } = "EUR";
}
