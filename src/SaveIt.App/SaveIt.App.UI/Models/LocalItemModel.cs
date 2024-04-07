namespace SaveIt.App.UI.Models;
public class LocalItemModel
{
    public required string Name { get; set; }
    public required string Path { get; set; }
    public bool IsDirectory { get; set; }
    public bool IsSelected { get; set; }
}
