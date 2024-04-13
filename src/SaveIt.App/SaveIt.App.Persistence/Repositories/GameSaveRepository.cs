using SaveIt.App.Domain;
using SaveIt.App.Domain.Entities;
using SaveIt.App.Domain.Repositories;
using SQLite;

namespace SaveIt.App.Persistence.Repositories;
internal class GameSaveRepository(IDatabaseHandler _dbHandler) : IGameSaveRepository
{
    private readonly SQLiteAsyncConnection _db = _dbHandler.CreateAsyncConnection();

    public async Task CreateGameSaveAsync(GameSave gameSave)
        => await _db.InsertAsync(gameSave);

    public async Task<GameSave?> GetGameSaveAsync(Guid gameSaveId)
        => await _db.Table<GameSave>()
            .FirstOrDefaultAsync(x => x.Id == gameSaveId);
}
