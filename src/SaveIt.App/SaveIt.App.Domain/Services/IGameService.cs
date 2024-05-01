using FluentResults;
using SaveIt.App.Domain.Models;

namespace SaveIt.App.Domain.Services;
public interface IGameService
{
    Task<Result> DownloadGameSaveToSpecificLocationAsync(Guid gameSaveId, string remoteLocationId, string destinationPath);
    Task<Result<IEnumerable<FileItemModel>>> GetGameSaveVersionsAsync(Guid storageAccountId, string remoteLocationId);
    Task<Result<LockFileModel?>> LockRepositoryAsync(Guid gameSaveId);
    Task<Result> PrepareGameSaveAsync(Guid gameSaveId);
    Task<Result> PrepareSpecificGameSaveAsync(Guid gameSaveId, string remoteLocationId);
    Task<Result> StartGameAsync(Guid gameId);
    Task<Result> UnlockRepositoryAsync(Guid gameSaveId);
    Task<Result> UploadFolderAsGameSaveAsync(Guid gameSaveId, string folderPath);
    Task<Result> UploadGameSaveAsync(Guid gameSaveId);
    Task<Result<bool>> IsRepositoryLockedAsync(Guid gameSaveId);
    Task<Result<ConfigFileModel>> GetConfigFileOrDefaultAsync(Guid gameSaveId);
    Task<Result> UpdateConfigFileAsync(Guid gameSaveId, int keepGameSavesCount);
}
