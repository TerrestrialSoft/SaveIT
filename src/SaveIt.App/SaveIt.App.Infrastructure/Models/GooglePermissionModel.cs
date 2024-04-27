namespace SaveIt.App.Infrastructure.Models;
public class GooglePermissionModel
{
    public string Id { get; set; } = default!;
    public string Role { get; set; } = default!;
    public string Type { get; set; } = default!;
    public string Kind { get; set; } = default!;
    public string EmailAddress { get; set; } = default!;
    public string DisplayName { get; set; } = default!;

    public const string OwnerRole = "owner";
}
