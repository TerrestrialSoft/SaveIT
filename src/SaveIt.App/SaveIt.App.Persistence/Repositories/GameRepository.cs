using SaveIt.App.Domain;
using SaveIt.App.Domain.Entities;
using SaveIt.App.Domain.Repositories;
using SQLite;

namespace SaveIt.App.Persistence.Repositories;
internal class GameRepository(IDatabaseHandler _dbHandler) : IGameRepository
{
    private readonly SQLiteAsyncConnection _db = _dbHandler.CreateAsyncConnection();

    public async Task<Game?> GetGame(Guid id)
        => await _db.Table<Game>()
            .FirstOrDefaultAsync(g => g.Id == id)!;

    public async Task CreateGameAsync(Game game)
        => await _db.InsertAsync(game);

    public async Task<IEnumerable<Game>> GetAllGamesAsync()
        => await _db.Table<Game>()
            .ToListAsync();
}
