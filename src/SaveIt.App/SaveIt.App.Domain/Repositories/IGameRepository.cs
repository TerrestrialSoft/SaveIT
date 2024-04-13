using SaveIt.App.Domain.Entities;

namespace SaveIt.App.Domain.Repositories;
public interface IGameRepository
{
    Task CreateGameAsync(Game game);
    Task<Game?> GetGameAsync(Guid id);
    Task<IEnumerable<Game>> GetAllGamesAsync();
    Task<IEnumerable<Game>> GetAllGamesWithChildrenAsync();
    Task DeleteGameAsync(Guid id);
}
