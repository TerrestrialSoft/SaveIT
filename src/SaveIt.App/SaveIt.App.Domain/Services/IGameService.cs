using FluentResults;
using SaveIt.App.Domain.Models;

namespace SaveIt.App.Domain.Services;
public interface IGameService
{
    Task<Result<LockFileModel?>> LockRepositoryAsync(Guid gameSaveId);
    Task<Result> StartGameAsync(Guid gameId);
    Task<Result> UnlockRepositoryAsync(Guid gameSaveId);
    Task<Result> UploadSaveAsync(Guid saveId);
}
