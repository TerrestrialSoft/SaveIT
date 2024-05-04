using FluentResults;
using SaveIt.App.Domain.Models;

namespace SaveIt.App.Domain.Services;
public interface IGameService
{
    Task<Result> DownloadGameSaveToSpecificLocationAsync(Guid gameSaveId, string remoteLocationId, string destinationPath,
        CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<FileItemModel>>> GetGameSaveVersionsAsync(Guid storageAccountId, string remoteLocationId,
        CancellationToken cancellationToken = default);
    Task<Result<LockFileModel?>> LockRepositoryAsync(Guid gameSaveId, CancellationToken cancellationToken = default);
    Task<Result> PrepareGameSaveAsync(Guid gameSaveId, CancellationToken cancellationToken = default);
    Task<Result> PrepareSpecificGameSaveAsync(Guid gameSaveId, string remoteLocationId,
        CancellationToken cancellationToken = default);
    Task<Result> StartGameAsync(Guid gameId, CancellationToken cancellationToken = default);
    Task<Result> UnlockRepositoryAsync(Guid gameSaveId, CancellationToken cancellationToken = default);
    Task<Result> UploadFolderAsGameSaveAsync(Guid gameSaveId, string folderPath, CancellationToken cancellationToken = default);
    Task<Result> UploadGameSaveAsync(Guid gameSaveId, CancellationToken cancellationToken = default);
    Task<Result<bool>> IsRepositoryLockedAsync(Guid gameSaveId, CancellationToken cancellationToken = default);
    Task<Result<ConfigFileModel>> GetConfigFileOrDefaultAsync(Guid gameSaveId, CancellationToken cancellationToken = default);
    Task<Result> UpdateConfigFileAsync(Guid gameSaveId, int keepGameSavesCount, CancellationToken cancellationToken = default);
}
