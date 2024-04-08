namespace SaveIt.App.Infrastructure.Models;
public record GoogleFileListModel(IEnumerable<GoogleFileModel> Files);

public record GoogleFileModel(string Id, string Kind, string Name, string MimeType, IEnumerable<string> Parents)
{
    private string? _parent;

    public string Parent => _parent ??= Parents.First();
}
