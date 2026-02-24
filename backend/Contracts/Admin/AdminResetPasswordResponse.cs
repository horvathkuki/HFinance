namespace backend.Contracts.Admin;

public class AdminResetPasswordResponse
{
    public string UserId { get; set; } = string.Empty;
    public string ResetToken { get; set; } = string.Empty;
}
