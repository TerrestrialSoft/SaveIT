namespace SaveIt.App.UI.Models;
public class DirectoryModel
{
    public required string Name { get; set; } = default!;
    public required string Path { get; set; } = default!;
    public bool IsSelected { get; set; }
}
