using SaveIt.App.Domain.Entities;

namespace SaveIt.App.Domain.Repositories;
public interface IGameRepository
{
    Task SaveGameAsync(Game game);
    Task<Game?> GetGame(Guid id);
    Task<IEnumerable<Game>> GetAllGamesAsync();
}
