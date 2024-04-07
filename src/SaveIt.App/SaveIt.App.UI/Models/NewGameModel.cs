namespace SaveIt.App.UI.Models;
public class NewGameModel
{
    public string Name { get; set; } = default!;
    public string Username { get; set; } = default!;
    public string GameSaveName { get; set; } = default!;
    public string LocalGameSavePath { get; set; } = default!;
    public string? LocalExecutablePath { get; set; }
    public ImageModel? Image { get; set; }
}
