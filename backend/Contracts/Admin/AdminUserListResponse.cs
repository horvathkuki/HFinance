namespace backend.Contracts.Admin;

public class AdminUserListResponse
{
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public IReadOnlyList<AdminUserDto> Items { get; set; } = Array.Empty<AdminUserDto>();
}
