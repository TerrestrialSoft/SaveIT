using System.ComponentModel.DataAnnotations;

namespace SaveIt.App.UI.Models;
public class NewGameModel
{
    [Required(ErrorMessage = "Game Name is required.")]
    public string Name { get; set; } = default!;

    [Required(ErrorMessage = "Username is required.")]
    public string Username { get; set; } = default!;

    [Required(ErrorMessage = "Game Save Name is required.")]
    public string GameSaveName { get; set; } = default!;

    [Required(ErrorMessage = "Local Game Save File is required.")]
    public LocalFileItemModel? LocalGameSaveFile { get; set; } = default!;

    [Required(ErrorMessage = "Remote Game Save File is required.")]
    public RemoteFileItemModel? RemoteGameSaveFile { get; set; } = default!;

    public LocalFileItemModel? GameExecutableFile { get; set; }

    [Required(ErrorMessage = "Storage Account is required.")]
    public Guid? StorageAccountId { get; set; }

    public ImageModel? Image { get; set; }
}
