using FluentResults;
using SaveIt.App.Domain.Auth;
using SaveIt.App.Domain.Errors;
using SaveIt.App.Domain.Models;
using SaveIt.App.Domain.Repositories;
using SaveIt.App.Domain.Services;

namespace SaveIt.App.Application.Services;
public class GameService(IProcessService _processService, IGameSaveRepository _gameSaveRepository, IFileService _fileService,
    IExternalStorageService _externalStorageService, IGameRepository _gameRepository) : IGameService
{
    private const string _lockFileName = ".lockfile";
    private const string _savePrefix = "save_";
    private const string _saveFileNameTemplate = _savePrefix + "{0}.zip";
    private const string _dateTimeFormat = "yyyy-MM-dd_HH-mm-ss";

    public async Task<Result<LockFileModel?>> LockRepositoryAsync(Guid gameSaveId)
    {
        var gameSave = await _gameSaveRepository.GetWithChildrenAsync(gameSaveId);
        if (gameSave is null)
        {
            return Result.Fail("Game save not found");
        }

        var lockFileResult = await GetLockFileMetadataAsync(gameSave.StorageAccountId, gameSave.RemoteLocationId);

        if (lockFileResult.IsFailed)
        {
            return lockFileResult.ToResult();
        }

        var lockFileModel = lockFileResult.Value;

        LockFileModel? lockFile;

        if (lockFileModel is null)
        {
            lockFile = GetLockFile(gameSave.Game.Id, gameSave.Game.Username);

            var result = await _externalStorageService.CreateFileAsync(gameSave.StorageAccountId, _lockFileName, lockFile,
                gameSave.RemoteLocationId);

            return result;
        }

        var lockfileResult = await _externalStorageService
            .DownloadJsonFileAsync<LockFileModel>(gameSave.StorageAccountId, lockFileModel.Id!);

        if (lockfileResult.IsFailed)
        {
            return lockfileResult.ToResult();
        }

        lockFile = lockfileResult.Value;

        if (lockFile is null)
        {
            return Result.Fail("Invalid lockfile content structure");
        }

        if (lockFile.Status == LockFileStatus.Locked)
        {
            return lockFile.LockDetails!.LockedByUserId == gameSave.Game.Id
                ? Result.Fail(GameErrors.CurrentUserLockedGameSave(lockFile))
                : Result.Fail(GameErrors.GameSaveInUse(lockFile));
        }

        lockFile = GetLockFile(gameSave.Game.Id, gameSave.Game.Username);

        var updateResult = await _externalStorageService.UpdateFileSimpleAsync(gameSave.StorageAccountId, lockFileModel.Id!,
            lockFile);

        if(updateResult.IsFailed)
        {
            return updateResult;
        }

        gameSave.IsHosting = true;
        await _gameSaveRepository.UpdateAsync(gameSave);

        return Result.Ok(lockFile)!;

        static LockFileModel GetLockFile(Guid userId, string username) => new()
        {
            Status = LockFileStatus.Locked,
            LockDetails = new LockDetailsModel()
            {
                LockedByUserId = userId,
                LockedByUsername = username,
                LockedAt = DateTime.UtcNow
            }
        };
    }

    public async Task<Result> PrepareGameSaveAsync(Guid gameSaveId)
    {
        var gameSave = await _gameSaveRepository.GetWithChildrenAsync(gameSaveId);
        if (gameSave is null)
        {
            return Result.Fail("Game save not found");
        }

        var fileDownloadResult = await _externalStorageService.GetNewestFileWithSubstringInNameAsync(gameSave.StorageAccountId,
            gameSave.RemoteLocationId, _savePrefix);

        if (fileDownloadResult.IsFailed)
        {
            return fileDownloadResult.ToResult();
        }

        if (fileDownloadResult.Value is null)
        {
            return Result.Ok();
        }

        var downloadResult = await _externalStorageService.DownloadFileAsync(gameSave.StorageAccountId,
            fileDownloadResult.Value.Id!);

        return downloadResult.IsSuccess
            ? _fileService.DecompressFile(gameSave.LocalGameSavePath, downloadResult.Value)
            : downloadResult.ToResult();
    }

    public async Task<Result> StartGameAsync(Guid gameId)
    {
        var game = await _gameRepository.GetAsync(gameId);

        if (game is null)
        {
            return Result.Fail("Game not found");
        }

        return game.GameExecutablePath is not null
            ? _processService.StartProcess(game.GameExecutablePath)
            : Result.Ok();
    }

    public async Task<Result> UnlockRepositoryAsync(Guid gameSaveId)
    {
        var gameSave = await _gameSaveRepository.GetWithChildrenAsync(gameSaveId);
        if (gameSave is null)
        {
            return Result.Fail("Game save not found");
        }

        var lockfileResult = await GetLockFileContentWithIdAsync(gameSave.StorageAccountId, gameSave.RemoteLocationId);

        if (lockfileResult.IsFailed)
        {
            return lockfileResult.ToResult();
        }   

        var lockfileId = lockfileResult.Value.Item1;
        var lockfileContent = lockfileResult.Value.Item2;

        if (lockfileId is null || lockfileContent is null)
        {
            return Result.Fail("Invalid lockfile content structure");
        }

        if (lockfileContent.Status != LockFileStatus.Locked)
        {
            return Result.Ok();
        }

        if (lockfileContent.LockDetails!.LockedByUserId != gameSave.GameId)
        {
            return Result.Fail(GameErrors.GameLockedByAnotherUser(lockfileContent));
        }

        lockfileContent = new()
        {
            Status = LockFileStatus.Unlocked
        };

        var updateResult = await _externalStorageService.UpdateFileSimpleAsync(gameSave.StorageAccountId, lockfileId!,
            lockfileContent);

        if (updateResult.IsFailed)
        {
            return updateResult;
        }

        gameSave.IsHosting = false;
        await _gameSaveRepository.UpdateAsync(gameSave);

        return Result.Ok();
    }

    public async Task<Result> UploadGameSaveAsync(Guid gameSaveId)
    {
        var gameSave = await _gameSaveRepository.GetWithChildrenAsync(gameSaveId);
        if (gameSave is null)
        {
            return Result.Fail("Game save not found");
        }

        var lockfileResult = await GetLockFileContentAsync(gameSave.StorageAccountId, gameSave.RemoteLocationId);

        if (lockfileResult.IsFailed)
        {
            return lockfileResult.ToResult();
        }

        if (lockfileResult.Value is null)
        {
            return Result.Fail("Lockfile not found");
        }

        if (lockfileResult.Value.Status != LockFileStatus.Locked)
        {
            return Result.Fail("Repository is not locked");
        }

        if (lockfileResult.Value.LockDetails!.LockedByUserId != gameSave.GameId)
        {
            return Result.Fail(GameErrors.GameLockedByAnotherUser(lockfileResult.Value));
        }

        var fileResult = _fileService.GetCompressedFile(gameSave.LocalGameSavePath);

        if (fileResult.IsFailed)
        {
            return fileResult.ToResult();
        }

        using var stream = fileResult.Value;

        string fileName = string.Format(_saveFileNameTemplate, DateTime.UtcNow.ToString(_dateTimeFormat));

        var uploadResult = await _externalStorageService.UploadFileAsync(gameSave.StorageAccountId, gameSave.RemoteLocationId,
            fileName, stream);
        
        if(uploadResult.IsFailed)
        {
            return uploadResult;
        }

        var unlockResult = await UnlockRepositoryAsync(gameSaveId);
        return unlockResult;
    }

    public Task<Result<IEnumerable<FileItemModel>>> GetGameSaveVersionsAsync(Guid storageAccountId, string remoteLocationId)
        => _externalStorageService.GetFilesWithSubstringInNameAsync(storageAccountId, remoteLocationId, _savePrefix);

    public async Task<Result> PrepareSpecificGameSaveAsync(Guid gameSaveId, string remoteLocationId)
    {
        var gameSave = await _gameSaveRepository.GetAsync(gameSaveId);
        if (gameSave is null)
        {
            return Result.Fail("Game save not found");
        }

        var result = await LockRepositoryAsync(gameSaveId);

        if (result.IsFailed)
        {
            return result.HasError<GameErrors.GameSaveInUseError>() || result.HasError<GameErrors.GameSaveAlreadyLocked>()
                ? Result.Fail("Unable to lock repository. Repository is already locked")
                : Result.Fail("Unable to lock repository.");
        }

        var fileDownloadResult = await _externalStorageService.DownloadFileAsync(gameSave.StorageAccountId, remoteLocationId);

        if (fileDownloadResult.IsFailed)
        {
            return fileDownloadResult.ToResult();
        }

        using var stream = fileDownloadResult.Value;

        return fileDownloadResult.Value is not null
            ? _fileService.DecompressFile(gameSave.LocalGameSavePath, stream)
            : Result.Fail("File not found");
    }

    public async Task<Result> DownloadGameSaveToSpecificLocationAsync(Guid gameSaveId, string remoteLocationId,
        string destinationPath)
    {
        var gameSave = await _gameSaveRepository.GetAsync(gameSaveId);
        if (gameSave is null)
        {
            return Result.Fail("Game save not found");
        }

        var fileInfoResult = await _externalStorageService.GetFileAsync(gameSave.StorageAccountId, remoteLocationId);

        if (fileInfoResult.IsFailed)
        {
            return fileInfoResult.ToResult();
        }

        var fileDownloadResult = await _externalStorageService.DownloadFileAsync(gameSave.StorageAccountId, remoteLocationId);

        if (fileDownloadResult.IsFailed)
        {
            return fileDownloadResult.ToResult();
        }

        using var stream = fileDownloadResult.Value;

        return fileDownloadResult.Value is not null
           ? _fileService.DecompressFile(destinationPath, stream, fileInfoResult.Value.Name)
           : Result.Fail("File not found");
    }

    public async Task<Result> UploadFolderAsGameSaveAsync(Guid gameSaveId, string folderPath)
    {
        var gameSave = await _gameSaveRepository.GetWithChildrenAsync(gameSaveId);
        if (gameSave is null)
        {
            return Result.Fail("Game save not found");
        }

        var lockResult = await LockRepositoryAsync(gameSaveId);

        if (lockResult.IsFailed)
        {
            return lockResult.ToResult();
        }

        var fileResult = _fileService.GetCompressedFile(folderPath);

        if (fileResult.IsFailed)
        {
            return fileResult.ToResult();
        }

        using var stream = fileResult.Value;
        string fileName = string.Format(_saveFileNameTemplate, DateTime.UtcNow.ToString(_dateTimeFormat));

        var uploadResult = await _externalStorageService.UploadFileAsync(gameSave.StorageAccountId, gameSave.RemoteLocationId,
            fileName, stream);

        return uploadResult.IsSuccess
            ? await UnlockRepositoryAsync(gameSaveId)
            : uploadResult;
    }

    private async Task<Result<FileItemModel?>> GetLockFileMetadataAsync(Guid storageAccountId, string remoteLocationId)
    {
        var lockFilesResult = await _externalStorageService.GetFilesWithNameAsync(storageAccountId, remoteLocationId,
            _lockFileName);

        if (lockFilesResult.IsFailed)
        {
            return lockFilesResult.ToResult();
        }

        var result = lockFilesResult.Value.Any()
            ? lockFilesResult.Value.First()
            : null;

        return Result.Ok(result);
    }

    private async Task<Result<LockFileModel?>> GetLockFileContentAsync(Guid storageAccountId, string remoteLocationId)
    {
        var result = await GetLockFileContentWithIdAsync(storageAccountId, remoteLocationId);

        return result.IsSuccess
            ? result.Value.Item2
            : result.ToResult();
    }

    private async Task<Result<(string?, LockFileModel?)>> GetLockFileContentWithIdAsync(Guid storageAccountId,
        string remoteLocationId)
    {
        var lockFileResult = await GetLockFileMetadataAsync(storageAccountId, remoteLocationId);

        if (lockFileResult.IsFailed)
        {
            return lockFileResult.ToResult();
        }

        var lockFileModel = lockFileResult.Value;

        string? fileId = null;
        LockFileModel? lockFile = null;

        if (lockFileModel is null)
        {
            return Result.Ok((fileId, lockFile));
        }

        fileId = lockFileModel.Id;

        var lockfileResult = await _externalStorageService
            .DownloadJsonFileAsync<LockFileModel>(storageAccountId, fileId!);

        if (lockfileResult.IsFailed)
        {
            return lockfileResult.ToResult();
        }
        
        lockFile = lockfileResult.Value;

        return (lockFile is not null
            ? Result.Ok((fileId, lockFile))
            : Result.Fail("Invalid lockfile content structure"))!;
    }

    public async Task<Result<bool>> IsRepositoryLockedAsync(Guid gameSaveId)
    {
        var gameSave = await _gameSaveRepository.GetWithChildrenAsync(gameSaveId);
        if (gameSave is null)
        {
            return Result.Fail("Game save not found");
        }

        var lockFileResult = await GetLockFileMetadataAsync(gameSave.StorageAccountId, gameSave.RemoteLocationId);

        if (lockFileResult.IsFailed)
        {
            return lockFileResult.ToResult();
        }

        if(lockFileResult.Value is null)
        {
            return Result.Ok(false);
        }

        var lockfileResult = await _externalStorageService
            .DownloadJsonFileAsync<LockFileModel>(gameSave.StorageAccountId, lockFileResult.Value.Id!);

        if (lockfileResult.IsFailed)
        {
            return lockfileResult.ToResult();
        }

        var lockFile = lockfileResult.Value;

        return lockFile is not null
            ? Result.Ok(lockFile.Status == LockFileStatus.Locked)
            : Result.Fail("Invalid lockfile content structure");
    }
}
