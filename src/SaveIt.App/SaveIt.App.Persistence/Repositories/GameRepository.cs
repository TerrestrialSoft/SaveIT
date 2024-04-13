using SaveIt.App.Domain;
using SaveIt.App.Domain.Entities;
using SaveIt.App.Domain.Repositories;
using SQLite;
using SQLiteNetExtensionsAsync.Extensions;

namespace SaveIt.App.Persistence.Repositories;
internal class GameRepository(IDatabaseHandler _dbHandler) : IGameRepository
{
    private readonly SQLiteAsyncConnection _db = _dbHandler.CreateAsyncConnection();

    public async Task<Game?> GetGameAsync(Guid id)
        => await _db.Table<Game>()
            .FirstOrDefaultAsync(g => g.Id == id);

    public async Task<IEnumerable<Game>> GetAllGamesWithChildrenAsync()
    {
        var results = await _db.Table<Game>()
            .ToListAsync();
        List<Game> games = [];

        foreach (var game in results)
        {
            await _db.GetChildrenAsync(game);
            games.Add(game);
        }

        return games;
    }

    public async Task CreateGameAsync(Game game)
        => await _db.InsertAsync(game);

    public async Task<IEnumerable<Game>> GetAllGamesAsync()
        => await _db.Table<Game>()
            .ToListAsync();

    public async Task DeleteGameAsync(Guid id)
    {
        await _db.DeleteAsync<Game>(id);
    }
}
