using SaveIt.App.Domain.Entities;
using SaveIt.App.Domain.Models;
using SaveIt.App.UI.Models;
using SaveIt.App.UI.Models.Games;
using SaveIt.App.UI.Models.GameSaves;

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
            IsShared = item.ShareWithMe,
            DriveId = item.DriveId
        };
    }

    public static GameModel? ToEditGameModel(this Game? game)
    {
        if (game is null)
        {
            return null;
        }

        LocalFileItemModel? fileModel = null;

        if(game.GameExecutablePath is not null)
        {
            var (fileName, directory) = GetNameAndPath(game.GameExecutablePath);
            fileModel = new LocalFileItemModel()
            {
                Name = fileName!,
                Path = directory!
            };
        }
        return new GameModel()
        {
            Id = game.Id,
            Name = game.Name,
            Username = game.Username,
            GameExecutableFile = fileModel,
            Image = game.Image is not null
                ? new ImageModel(game.Image.Name, game.Image.Content)
                : null
        };
    }

    private static (string?, string?) GetNameAndPath(string path)
    {
        var fileName = Path.GetFileName(path)!;
        var directory = Path.GetDirectoryName(path)!;

        return (fileName, directory);
    }

    public static GameSaveViewModel? ToViewModel(this GameSave? gameSave)
        => gameSave is not null
            ? new GameSaveViewModel()
            {
                Name = gameSave.Name,
                GameName = gameSave.Game.Name,
                GameSave = gameSave
            }
            : null;

    public static NewGameSaveModel ToNewGameSaveModel(this GameSave gameSave)
        => new()
        {
            GameId = gameSave.GameId,
            GameSave = gameSave.ToGameSaveModel()
        };

    public static GameSaveModel ToGameSaveModel(this GameSave gameSave)
    {
        var (localFileName, localDirectory) = GetNameAndPath(gameSave.LocalGameSavePath);

        return new()
        {
            Name = gameSave.Name,
            LocalGameSaveFile = new LocalFileItemModel()
            {
                Name = localFileName!,
                Path = localDirectory!,
                IsDirectory = true
            },
            RemoteGameSaveFile = new RemoteFileItemModel()
            {
                Id = gameSave.RemoteLocationId,
                Name = gameSave.RemoteLocationName,
            },
            StorageAccountId = gameSave.StorageAccountId
        };
    }
}
