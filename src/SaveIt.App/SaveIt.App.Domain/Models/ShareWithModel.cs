namespace SaveIt.App.Domain.Models;
public class ShareWithModel
{
    public required string PermissionId { get; set; }
    public required string Username { get; set; }
    public required string Email { get; set; }
    public required bool IsOwner { get; set; }
}
