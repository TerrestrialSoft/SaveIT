using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace SaveIt.App.UI.Models;
public class NewGameModel
{
    [Required]
    public string Name { get; set; } = default!;

    [Required]
    public string Username { get; set; } = default!;

    [Required]
    [DisplayName("Game Save Name")]
    public string GameSaveName { get; set; } = default!;

    [Required]
    [DisplayName("Local Game Save File")]
    public LocalFileItemModel? LocalGameSaveFile { get; set; } = default!;

    [Required]
    [DisplayName("Remote Game Save File")]
    public RemoteFileItemModel? RemoteGameSaveFile { get; set; } = default!;

    public LocalFileItemModel? GameExecutableFile { get; set; }

    [Required]
    [DisplayName("Storage Account")]
    public Guid? StorageAccountId { get; set; }

    public ImageModel? Image { get; set; }
}
