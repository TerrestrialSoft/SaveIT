using SaveIt.App.Domain.Entities;
using SaveIt.App.Domain.Models;
using SaveIt.App.UI.Models;
using SaveIt.App.UI.Models.Game;

namespace SaveIt.App.UI.Extensions;
public static class ModelExtensions
{
    public static RemoteFileItemModel ToRemoteFileItemModel(this FileItemModel item)
    {
        ArgumentNullException.ThrowIfNull(item.Id);

        return new RemoteFileItemModel
        {
            Id = item.Id,
            Name = item.Name,
            ParentId = item.ParentId,
            IsDirectory = item.FileType == FileItemType.Folder,
        };
    }

    public static GameModel? ToEditGameModel(this Game? game)
    {
        if (game is null)
        {
            return null;
        }

        var fileName = Path.GetFileName(game.GameExecutablePath)!;
        var directory = Path.GetDirectoryName(game.GameExecutablePath)!;

        return new GameModel()
        {
            Id = game.Id,
            Name = game.Name,
            Username = game.Username,
            GameExecutableFile = new LocalFileItemModel()
            {
                Name = fileName,
                Path = directory
            },
            Image = game.Image is not null
                ? new ImageModel(game.Image.Name, game.Image.Content)
                : null
        };
    }
}
