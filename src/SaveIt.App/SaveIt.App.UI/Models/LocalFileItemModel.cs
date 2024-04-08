namespace SaveIt.App.UI.Models;
public class LocalFileItemModel : NamedModel
{
    public string Path { get; set; } = "";
    public bool IsDirectory { get; set; }
    public override string FullPath => System.IO.Path.Combine(Path, Name);
}
