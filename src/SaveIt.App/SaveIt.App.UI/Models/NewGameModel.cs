namespace SaveIt.App.UI.Models;
public class NewGameModel
{
    public string Name { get; set; } = default!;
    public string Username { get; set; } = default!;
    public string GameSaveName { get; set; } = default!;
    public LocalFileItemModel? LocalGameSaveFile { get; set; } = default!;
    public RemoteFileItemModel? RemoteGameSaveFile { get; set; } = default!;
    public LocalFileItemModel? LocalExecutableFile { get; set; }
    public Guid? StorageAccountId { get; set; }
    public ImageModel? Image { get; set; }
}
