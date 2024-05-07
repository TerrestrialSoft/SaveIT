namespace SaveIt.App.Infrastructure.Models;
public record GoogleFileListModel(IEnumerable<GoogleFileModel> Files);

public record GoogleFileModel(string Id, string Kind, string Name, string MimeType, string? DriveId, string? SharedWithMeTime,
    IEnumerable<string>? Parents = null)
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
                ? RootParentId
                : Parents.First();

            return _parent;
        }
    }

    public bool SharedWithMe => SharedWithMeTime is not null;

    public const string RootParentId = "root";
}
