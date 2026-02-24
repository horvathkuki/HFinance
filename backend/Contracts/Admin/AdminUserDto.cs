namespace backend.Contracts.Admin;

public class AdminUserDto
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public string BaseCurrency { get; set; } = "EUR";
    public IReadOnlyList<string> Roles { get; set; } = Array.Empty<string>();
}
