namespace SaveIt.App.Domain.Models;
public record FileItemModel(string Name, FileItemType FileType, string ParentId, string? Id = null);

public enum FileItemType
{
    File = 1,
    Folder = 2
}