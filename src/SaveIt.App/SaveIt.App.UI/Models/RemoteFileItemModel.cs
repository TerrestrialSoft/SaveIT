namespace SaveIt.App.UI.Models;
public class RemoteFileItemModel : NamedModel
{
    public required string Id { get; set; }
    public string? ParentId { get; set; }
    public bool IsDirectory { get; set; }
    public string? DriveId { get; set; }
    public bool IsShared { get; set; }

    public const string DefaultId = "root";
}
