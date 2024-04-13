using FluentResults;

namespace SaveIt.App.Domain;
public interface IGameService
{
    Task<Result> LockRepositoryAsync(Guid gameSaveId);
}
