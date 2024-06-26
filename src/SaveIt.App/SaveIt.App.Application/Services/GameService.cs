﻿using FluentResults;
using SaveIt.App.Domain.Auth;
using SaveIt.App.Domain.Entities;
using SaveIt.App.Domain.Errors;
using SaveIt.App.Domain.Models;
using SaveIt.App.Domain.Repositories;
using SaveIt.App.Domain.Services;

namespace SaveIt.App.Application.Services;
public class GameService(IProcessService _processService, IGameSaveRepository _gameSaveRepository, IFileService _fileService,
    IExternalStorageService _externalStorageService, IGameRepository _gameRepository) : IGameService
{
    private const string _lockFileName = ".lockfile";
    private const string _configFileName = ".config";
    private const string _savePrefix = "save_";
    private const string _saveFileNameTemplate = _savePrefix + "{0}.zip";
    private const string _dateTimeFormat = "yyyy-MM-dd_HH-mm-ss";
    private const int _keepGameSaveCount = 10;
    private static readonly ConfigFileModel DefaultConfigFile = new()
    {
        KeepGameSavesCount = _keepGameSaveCount
    };

    public async Task<Result<LockFileModel?>> LockRepositoryAsync(Guid gameSaveId, CancellationToken cancellationToken = default)
    {
        var gameSave = await _gameSaveRepository.GetWithChildrenAsync(gameSaveId);
        if (gameSave is null)
        {
            return Result.Fail("Game save not found");
        }

        var lockFileResult = await GetLockFileMetadataAsync(gameSave.StorageAccountId, gameSave.RemoteLocationId,
            cancellationToken);

        if (lockFileResult.IsFailed)
        {
            return lockFileResult.ToResult();
        }

        var lockFileModel = lockFileResult.Value;

        LockFileModel? lockFile;

        if (lockFileModel is null)
        {
            lockFile = GetLockFileModel(gameSave.Game.Id, gameSave.Game.Username);

            var result = await _externalStorageService.CreateFileAsync(gameSave.StorageAccountId, _lockFileName, lockFile,
                gameSave.RemoteLocationId, cancellationToken);

            if(result.IsFailed)
            {
                return result;
            }

            gameSave.IsHosting = true;
            await _gameSaveRepository.UpdateAsync(gameSave);

            return Result.Ok();
        }

        var lockfileResult = await _externalStorageService
            .DownloadJsonFileAsync<LockFileModel>(gameSave.StorageAccountId, lockFileModel.Id!, cancellationToken);

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

        lockFile = GetLockFileModel(gameSave.Game.Id, gameSave.Game.Username);

        var updateResult = await _externalStorageService.UpdateFileSimpleAsync(gameSave.StorageAccountId, lockFileModel.Id!,
            lockFile, cancellationToken: cancellationToken);

        if(updateResult.IsFailed)
        {
            return updateResult;
        }

        gameSave.IsHosting = true;
        await _gameSaveRepository.UpdateAsync(gameSave);

        return Result.Ok(lockFile)!;

        static LockFileModel GetLockFileModel(Guid userId, string username) => new()
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

    public async Task<Result> PrepareGameSaveAsync(Guid gameSaveId, CancellationToken cancellationToken = default)
    {
        var gameSave = await _gameSaveRepository.GetWithChildrenAsync(gameSaveId);
        if (gameSave is null)
        {
            return Result.Fail("Game save not found");
        }

        var fileDownloadResult = await _externalStorageService.GetNewestFileWithSubstringInNameAsync(gameSave.StorageAccountId,
            gameSave.RemoteLocationId, _savePrefix, cancellationToken);

        if (fileDownloadResult.IsFailed)
        {
            return fileDownloadResult.ToResult();
        }

        if (fileDownloadResult.Value is null)
        {
            return Result.Ok();
        }

        var downloadResult = await _externalStorageService.DownloadFileAsync(gameSave.StorageAccountId,
            fileDownloadResult.Value.Id!, cancellationToken: cancellationToken);

        return downloadResult.IsSuccess
            ? _fileService.DecompressFile(gameSave.LocalGameSavePath, downloadResult.Value)
            : downloadResult.ToResult();
    }

    public async Task<Result> StartGameAsync(Guid gameId, CancellationToken cancellationToken = default)
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

    public async Task<Result> UnlockRepositoryAsync(Guid gameSaveId, bool forceUnlock = false,
        CancellationToken cancellationToken = default)
    {
        var gameSave = await _gameSaveRepository.GetWithChildrenAsync(gameSaveId);
        if (gameSave is null)
        {
            return Result.Fail("Game save not found");
        }

        var lockfileResult = await GetLockFileContentWithIdAsync(gameSave.StorageAccountId, gameSave.RemoteLocationId,
            cancellationToken);

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

        if (lockfileContent.LockDetails!.LockedByUserId != gameSave.GameId && !forceUnlock)
        {
            return Result.Fail(GameErrors.GameLockedByAnotherUser(lockfileContent));
        }

        lockfileContent = new()
        {
            Status = LockFileStatus.Unlocked
        };

        var updateResult = await _externalStorageService.UpdateFileSimpleAsync(gameSave.StorageAccountId, lockfileId!,
            lockfileContent, cancellationToken: cancellationToken);

        if (updateResult.IsFailed)
        {
            return updateResult;
        }

        gameSave.IsHosting = false;
        await _gameSaveRepository.UpdateAsync(gameSave);

        return Result.Ok();
    }

    public async Task<Result> UploadGameSaveAsync(Guid gameSaveId, CancellationToken cancellationToken = default)
    {
        var gameSave = await _gameSaveRepository.GetWithChildrenAsync(gameSaveId);
        if (gameSave is null)
        {
            return Result.Fail("Game save not found");
        }

        var lockfileResult = await GetLockFileContentAsync(gameSave.StorageAccountId, gameSave.RemoteLocationId,
            cancellationToken);

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
            fileName, stream, cancellationToken);
        
        if(uploadResult.IsFailed)
        {
            return uploadResult;
        }

        await EnsureStoredGameSaveCountAsync(gameSave, cancellationToken);
        var unlockResult = await UnlockRepositoryAsync(gameSaveId, cancellationToken: cancellationToken);

        return unlockResult;
    }

    private async Task<Result> EnsureStoredGameSaveCountAsync(GameSave gameSave, CancellationToken cancellationToken = default)
    {
        var configFileResult = await GetConfigFileOrDefaultAsync(gameSave.Id, cancellationToken);

        if (configFileResult.IsFailed)
        {
            return configFileResult.ToResult();
        }

        var configFile = configFileResult.Value;

        var gameSavesResult = await _externalStorageService.GetFilesWithSubstringInNameOrderedByDateAscAsync(
            gameSave.StorageAccountId, gameSave.RemoteLocationId, _savePrefix, cancellationToken);

        if (gameSavesResult.IsFailed)
        {
            return gameSavesResult.ToResult();
        }

        var gameSaves = gameSavesResult.Value;

        if (gameSaves.Count() <= configFile.KeepGameSavesCount)
        {
            return Result.Ok();
        }

        var gameSavesToDelete = gameSaves.Take(gameSaves.Count() - configFile.KeepGameSavesCount);

        foreach (var file in gameSavesToDelete)
        {
            var deleteResult = await _externalStorageService.DeleteFileAsync(gameSave.StorageAccountId, file.Id!,
                cancellationToken);
            if (deleteResult.IsFailed)
            {
                return deleteResult;
            }
        }

        return Result.Ok();
    }

    public Task<Result<IEnumerable<FileItemModel>>> GetGameSaveVersionsAsync(Guid storageAccountId, string remoteLocationId,
        CancellationToken cancellationToken = default)
        => _externalStorageService.GetFilesWithSubstringInNameAsync(storageAccountId, remoteLocationId, _savePrefix,
            cancellationToken);

    public async Task<Result> PrepareSpecificGameSaveAsync(Guid gameSaveId, string remoteLocationId,
        CancellationToken cancellationToken = default)
    {
        var gameSave = await _gameSaveRepository.GetAsync(gameSaveId);
        if (gameSave is null)
        {
            return Result.Fail("Game save not found");
        }

        var result = await LockRepositoryAsync(gameSaveId, cancellationToken);

        if (result.IsFailed)
        {
            return result.HasError<GameErrors.GameSaveInUseError>() || result.HasError<GameErrors.GameSaveAlreadyLocked>()
                ? Result.Fail("Unable to lock repository. Repository is already locked")
                : Result.Fail("Unable to lock repository.");
        }

        var fileDownloadResult = await _externalStorageService.DownloadFileAsync(gameSave.StorageAccountId, remoteLocationId,
            cancellationToken);

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
        string destinationPath, CancellationToken cancellationToken = default)
    {
        var gameSave = await _gameSaveRepository.GetAsync(gameSaveId);
        if (gameSave is null)
        {
            return Result.Fail("Game save not found");
        }

        var fileInfoResult = await _externalStorageService.GetFileAsync(gameSave.StorageAccountId, remoteLocationId,
            cancellationToken);

        if (fileInfoResult.IsFailed)
        {
            return fileInfoResult.ToResult();
        }

        var fileDownloadResult = await _externalStorageService.DownloadFileAsync(gameSave.StorageAccountId, remoteLocationId,
            cancellationToken);

        if (fileDownloadResult.IsFailed)
        {
            return fileDownloadResult.ToResult();
        }

        using var stream = fileDownloadResult.Value;

        return fileDownloadResult.Value is not null
           ? _fileService.DecompressFile(destinationPath, stream, fileInfoResult.Value.Name)
           : Result.Fail("File not found");
    }

    public async Task<Result> UploadFolderAsGameSaveAsync(Guid gameSaveId, string folderPath,
        CancellationToken cancellationToken = default)
    {
        var gameSave = await _gameSaveRepository.GetWithChildrenAsync(gameSaveId);
        if (gameSave is null)
        {
            return Result.Fail("Game save not found");
        }

        var lockResult = await LockRepositoryAsync(gameSaveId, cancellationToken);

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
            fileName, stream, cancellationToken);

        if (uploadResult.IsFailed)
        {
            return uploadResult;
        }   

        await EnsureStoredGameSaveCountAsync(gameSave, cancellationToken);

        return await UnlockRepositoryAsync(gameSaveId, cancellationToken: cancellationToken);
    }

    private async Task<Result<FileItemModel?>> GetLockFileMetadataAsync(Guid storageAccountId, string remoteLocationId,
        CancellationToken cancellationToken = default)
    {
        var lockFilesResult = await _externalStorageService.GetFilesWithNameAsync(storageAccountId, remoteLocationId,
            _lockFileName, cancellationToken);

        if (lockFilesResult.IsFailed)
        {
            return lockFilesResult.ToResult();
        }

        var result = lockFilesResult.Value.Any()
            ? lockFilesResult.Value.First()
            : null;

        return Result.Ok(result);
    }

    private async Task<Result<LockFileModel?>> GetLockFileContentAsync(Guid storageAccountId, string remoteLocationId,
        CancellationToken cancellationToken = default)
    {
        var result = await GetLockFileContentWithIdAsync(storageAccountId, remoteLocationId, cancellationToken);

        return result.IsSuccess
            ? result.Value.Item2
            : result.ToResult();
    }

    private async Task<Result<(string?, LockFileModel?)>> GetLockFileContentWithIdAsync(Guid storageAccountId,
        string remoteLocationId, CancellationToken cancellationToken = default)
    {
        var lockFileResult = await GetLockFileMetadataAsync(storageAccountId, remoteLocationId, cancellationToken);

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
            .DownloadJsonFileAsync<LockFileModel>(storageAccountId, fileId!, cancellationToken);

        if (lockfileResult.IsFailed)
        {
            return lockfileResult.ToResult();
        }
        
        lockFile = lockfileResult.Value;

        return (lockFile is not null
            ? Result.Ok((fileId, lockFile))
            : Result.Fail("Invalid lockfile content structure"))!;
    }

    public async Task<Result<bool>> IsRepositoryLockedAsync(Guid gameSaveId, CancellationToken cancellationToken = default)
    {
        var gameSave = await _gameSaveRepository.GetWithChildrenAsync(gameSaveId);
        if (gameSave is null)
        {
            return Result.Fail("Game save not found");
        }

        var lockFileResult = await GetLockFileMetadataAsync(gameSave.StorageAccountId, gameSave.RemoteLocationId,
            cancellationToken);

        if (lockFileResult.IsFailed)
        {
            return lockFileResult.ToResult();
        }

        if(lockFileResult.Value is null)
        {
            return Result.Ok(false);
        }

        var lockfileResult = await _externalStorageService
            .DownloadJsonFileAsync<LockFileModel>(gameSave.StorageAccountId, lockFileResult.Value.Id!,
            cancellationToken: cancellationToken);

        if (lockfileResult.IsFailed)
        {
            return lockfileResult.ToResult();
        }

        var lockFile = lockfileResult.Value;

        return lockFile is not null
            ? Result.Ok(lockFile.Status == LockFileStatus.Locked)
            : Result.Fail("Invalid lockfile content structure");
    }

    public async Task<Result> UpdateConfigFileAsync(Guid gameSaveId, int keepGameSavesCount,
        CancellationToken cancellationToken = default)
    {
        var gameSave = await _gameSaveRepository.GetWithChildrenAsync(gameSaveId);
        if (gameSave is null)
        {
            return Result.Fail("Game save not found");
        }

        var configFileResult = await GetConfigMetadataAsync(gameSave, cancellationToken);

        if (configFileResult.IsFailed)
        {
            return configFileResult.ToResult();
        }

        var configFile = configFileResult.Value;

        var configFileModel = new ConfigFileModel
        {
            KeepGameSavesCount = keepGameSavesCount
        };

        if (configFile is null)
        {
            var createFileResult = await _externalStorageService.CreateFileAsync(gameSave.StorageAccountId, _configFileName,
                configFileModel, gameSave.RemoteLocationId, cancellationToken);

            return createFileResult;
        }

        var result = await _externalStorageService.UpdateFileSimpleAsync(gameSave.StorageAccountId, configFile.Id!,
            configFileModel, cancellationToken: cancellationToken);

        return result;
    }

    private async Task<Result<FileItemModel?>> GetConfigMetadataAsync(GameSave gameSave, 
       CancellationToken cancellationToken = default)
    {
        var configFileResult = await _externalStorageService.GetFilesWithNameAsync(gameSave.StorageAccountId,
            gameSave.RemoteLocationId, _configFileName, cancellationToken);

        if (configFileResult.IsFailed)
        {
            return configFileResult.ToResult();
        }

        var configFile = configFileResult.Value.FirstOrDefault();

        return configFileResult.Value.Count() <= 1
            ? Result.Ok(configFile)
            : Result.Fail("Multiple config files found");
    }

    public async Task<Result<ConfigFileModel>> GetConfigFileOrDefaultAsync(Guid gameSaveId,
        CancellationToken cancellationToken = default)
    {
        var gameSave = await _gameSaveRepository.GetWithChildrenAsync(gameSaveId);
        if (gameSave is null)
        {
            return Result.Fail("Game save not found");
        }

        var configFileMetadataResult = await GetConfigMetadataAsync(gameSave, cancellationToken);

        if (configFileMetadataResult.IsFailed)
        {
            return configFileMetadataResult.ToResult();
        }

        var configFileMetadata = configFileMetadataResult.Value;

        if (configFileMetadata is null)
        {
            return Result.Ok(DefaultConfigFile);
        }

        var configFileResult = await _externalStorageService.DownloadJsonFileAsync<ConfigFileModel>(gameSave.StorageAccountId,
            configFileMetadata.Id!, cancellationToken: cancellationToken);

        return configFileResult.IsSuccess
            ? Result.Ok(configFileResult.Value ?? DefaultConfigFile)
            : configFileResult!;
    }
}
