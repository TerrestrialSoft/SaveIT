using FluentResults;
using SaveIt.App.Domain;
using SaveIt.App.Domain.Auth;
using SaveIt.App.Domain.Entities;
using SaveIt.App.Domain.Errors;
using SaveIt.App.Domain.Repositories;

namespace SaveIt.App.Application.Services;
public class GameService(IProcessService _processService, IGameSaveRepository _gameSaveRepository,
    IExternalStorageService _externalStorageService) : IGameService
{
    private const string _lockFileName = ".lockfile";

    public async Task<Result> LockRepositoryAsync(Guid gameSaveId)
    {
        var gameSave = await _gameSaveRepository.GetGameSaveAsync(gameSaveId);
        if (gameSave is null)
        {
            return Result.Fail("Game save not found");
        }

        var lockFilesResult = await _externalStorageService.GetFilesWithNameAsync(gameSave.StorageAccountId,
            gameSave.RemoteLocationId, _lockFileName);

        if (lockFilesResult.IsFailed)
        {
            return lockFilesResult.ToResult();
        }

        if(lockFilesResult.Value.Any())
        {
            return Result.Fail(GameError.GameSaveInUse);
        }

        var result = await _externalStorageService.CreateFileAsync(gameSave.StorageAccountId, _lockFileName,
            gameSave.RemoteLocationId);

        return result;
    }

    //public async Task<Result> StartGameXX()
    //{
    //    if (gameSave.Game.GameExecutablePath is not null)
    //    {
    //        var result = _processService.StartProcess(gameSave.Game.GameExecutablePath);

    //        if (result.IsFailed)
    //        {
    //            return result;
    //        }
    //    }
    //}
}
