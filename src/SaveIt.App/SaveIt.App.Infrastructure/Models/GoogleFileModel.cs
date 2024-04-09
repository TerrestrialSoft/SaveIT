namespace SaveIt.App.Infrastructure.Models;
public record GoogleFileListModel(IEnumerable<GoogleFileModel> Files);

public record GoogleFileModel(string Id, string Kind, string Name, string MimeType, IEnumerable<string>? Parents = null)
{
    private string? _parent;

    public string Parent
    {
        get
        {
            if (_parent is not null)
            {
                return _parent;
            }

            _parent = Parents is null || !Parents.Any()
                ? "root"
                : Parents.First();

            return _parent;
        }
    }
}
