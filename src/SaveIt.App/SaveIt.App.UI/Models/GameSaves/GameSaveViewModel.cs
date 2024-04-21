using SaveIt.App.Domain.Entities;

namespace SaveIt.App.UI.Models.GameSaves;
public class GameSaveViewModel
{
    public required string Name { get; set; }
    public required string GameName { get; set; }

    public required GameSave GameSave { get; set; }
}
