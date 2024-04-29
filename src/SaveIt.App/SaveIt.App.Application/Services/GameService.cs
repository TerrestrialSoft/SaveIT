﻿using FluentResults;
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

        var lockFilesResult = await _externalStorageService.GetFilesWithNameAsync(gameSave.StorageAccountId,
            gameSave.RemoteLocationId, _lockFileName);

        if (lockFilesResult.IsFailed)
        {
            return lockFilesResult.ToResult();
        }

        LockFileModel? lockFile;

        if (!lockFilesResult.Value.Any())
        {
            lockFile = GetLockFile(gameSave.Game.Id, gameSave.Game.Username);

            var result = await _externalStorageService.CreateFileAsync(gameSave.StorageAccountId, _lockFileName, lockFile,
                gameSave.RemoteLocationId);

            return result;
        }

        var lockFileModel = lockFilesResult.Value.First();
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

        return (updateResult.IsSuccess
            ? Result.Ok(lockFile)!
            : updateResult)!;

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

        var lockFilesResult = await _externalStorageService.GetFilesWithNameAsync(gameSave.StorageAccountId,
            gameSave.RemoteLocationId, _lockFileName);

        if (lockFilesResult.IsFailed)
        {
            return lockFilesResult.ToResult();
        }

        LockFileModel? lockFile;

        if (!lockFilesResult.Value.Any())
        {
            return Result.Ok();
        }

        var lockFileModel = lockFilesResult.Value.First();
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

        if (lockFile.Status != LockFileStatus.Locked)
        {
            return Result.Ok();
        }

        if (lockFile.LockDetails!.LockedByUserId != gameSave.GameId)
        {
            return Result.Fail(GameErrors.GameLockedByAnotherUser(lockFile));
        }

        lockFile = new()
        {
            Status = LockFileStatus.Unlocked
        };

        var updateResult = await _externalStorageService.UpdateFileSimpleAsync(gameSave.StorageAccountId, lockFileModel.Id!,
            lockFile);

        return updateResult.IsSuccess
            ? Result.Ok()
            : updateResult;
    }

    public async Task<Result> UploadSaveAsync(Guid gameSaveId)
    {
        var gameSave = await _gameSaveRepository.GetWithChildrenAsync(gameSaveId);
        if (gameSave is null)
        {
            return Result.Fail("Game save not found");
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
}
