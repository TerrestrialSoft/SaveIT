namespace SaveIt.App.Domain.Models;
public record FileItem(string Name, FileItemType FileType, string ParentId, string? Id = null);

public enum FileItemType
{
    File = 1,
    Folder = 2
}