namespace SaveIt.App.UI.Models;
public class LocalFileItemModel : NamedModel
{
    public string Path { get; set; } = "";
    public bool IsDirectory { get; set; }
    public bool IsDrive { get; set; }

    public override string FullPath => !IsDrive
        ? System.IO.Path.Combine(Path, Name)
        : Path;

    public string DirectoryPath
        => IsDirectory
            ? FullPath
            : Path;
}
