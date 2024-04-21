using SaveIt.App.Domain.Entities;

namespace SaveIt.App.Domain.Repositories;
public interface IGameSaveRepository
{
    Task CreateGameSaveAsync(GameSave gameSave);
    Task<GameSave?> GetGameSaveAsync(Guid gameSaveId);
    Task<IEnumerable<GameSave>> GetAllGameSaveAsync();
    Task<GameSave?> GetGameSaveWithChildrenAsync(Guid gameSaveId);
    Task<IEnumerable<GameSave>> GetAllGameSavesWithChildrenAsync();
    Task DeleteGameSaveAsync(GameSave gameSave);
    Task UpdateGameSaveAsync(GameSave gameSave);
}
