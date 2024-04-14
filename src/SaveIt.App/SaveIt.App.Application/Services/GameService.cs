using FluentResults;
using SaveIt.App.Domain;
using SaveIt.App.Domain.Auth;
using SaveIt.App.Domain.Errors;
using SaveIt.App.Domain.Models;
using SaveIt.App.Domain.Repositories;

namespace SaveIt.App.Application.Services;
public class GameService(IProcessService _processService, IGameSaveRepository _gameSaveRepository,
    IExternalStorageService _externalStorageService) : IGameService
{
    private const string _lockFileName = ".lockfile";

    public async Task<Result<LockFileModel?>> LockRepositoryAsync(Guid gameSaveId)
    {
        var gameSave = await _gameSaveRepository.GetGameSaveWithChildrenAsync(gameSaveId);
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

        var newLockFile = new LockFileModel()
        {
            Status = LockFileStatus.Locked,
            LockDetails = new LockDetailsModel()
            {
                LockedAt = DateTime.UtcNow,
                LockedBy = gameSave.Game.Username
            }
        };

        if (lockFilesResult.Value.Any())
        {
            var lockFileModel = lockFilesResult.Value.First();
            var lockfileResult = await _externalStorageService
                .DownloadFileAsync<LockFileModel>(gameSave.StorageAccountId, lockFileModel.Id!);

            if (lockfileResult.IsFailed)
            {
                return lockfileResult.ToResult();
            }

            var lockFile = lockfileResult.Value;

            if (lockFile is null)
            {
                return Result.Fail("Invalid lockfile content structure");
            }

            if (lockFile.Status == LockFileStatus.Locked)
            {
                return Result.Fail(GameError.GameSaveInUse(lockFile));
            }

            // TODO: Lockfile is unlocked, lock it – Update lockfile with new lock details.

            return Result.Ok(newLockFile);
        }

        var result = await _externalStorageService.CreateFileAsync(gameSave.StorageAccountId, _lockFileName, newLockFile,
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
