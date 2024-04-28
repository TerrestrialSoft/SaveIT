﻿using FluentResults;
using SaveIt.App.Domain.Models;

namespace SaveIt.App.Domain.Services;
public interface IGameService
{
    Task<Result<IEnumerable<FileItemModel>>> GetGameSaveVersionsAsync(Guid storageAccountId, string remoteLocationId);
    Task<Result<LockFileModel?>> LockRepositoryAsync(Guid gameSaveId);
    Task<Result> PrepareGameSaveAsync(Guid gameSaveId);
    Task<Result> StartGameAsync(Guid gameId);
    Task<Result> UnlockRepositoryAsync(Guid gameSaveId);
    Task<Result> UploadSaveAsync(Guid gameSaveId);
}
