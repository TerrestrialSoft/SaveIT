using FluentResults;
using SaveIt.App.Domain.Models;

namespace SaveIt.App.Domain;
public interface IGameService
{
    Task<Result<LockFileModel?>> LockRepositoryAsync(Guid gameSaveId);
}
