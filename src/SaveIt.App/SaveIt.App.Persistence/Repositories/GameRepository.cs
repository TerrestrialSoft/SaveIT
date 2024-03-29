using SaveIt.App.Domain;
using SaveIt.App.Domain.Entities;
using SaveIt.App.Domain.Repositories;
using SQLite;

namespace SaveIt.App.Persistence.Repositories;
internal class GameRepository(IDatabaseHandler dbHandler) : IGameRepository
{
    private readonly SQLiteAsyncConnection _db = dbHandler.CreateAsyncConnection();

    public async Task<Game?> GetGame(Guid id)
    {
        return await _db.Table<Game>()
            .FirstOrDefaultAsync(g => g.Id == id);
    }

    public async Task SaveGameAsync(Game game)
    {
        await _db.InsertAsync(game);
    }

    public async Task<IEnumerable<Game>> GetAllGamesAsync()
    {
        return await _db.Table<Game>()
            .ToListAsync();
    }
}
