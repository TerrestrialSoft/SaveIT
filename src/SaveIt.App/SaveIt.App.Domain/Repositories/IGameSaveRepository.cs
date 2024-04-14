using SaveIt.App.Domain.Entities;

namespace SaveIt.App.Domain.Repositories;
public interface IGameSaveRepository
{
    Task CreateGameSaveAsync(GameSave gameSave);
    Task<GameSave?> GetGameSaveAsync(Guid gameSaveId);
    Task<GameSave?> GetGameSaveWithChildrenAsync(Guid gameSaveId);
}
