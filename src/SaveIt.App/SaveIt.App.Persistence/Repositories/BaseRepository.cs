using SaveIt.App.Domain.Entities;
using SaveIt.App.Domain;
using SQLite;
using SQLiteNetExtensionsAsync.Extensions;
using SaveIt.App.Domain.Repositories;

namespace SaveIt.App.Persistence.Repositories;
public class BaseRepository<T>(IDatabaseHandler _dbHandler) : IRepository<T> where T : BaseEntity, new()
{
    protected readonly SQLiteAsyncConnection _db = _dbHandler.CreateAsyncConnection();

    public async Task<T?> GetAsync(Guid id)
        => await _db.Table<T>()
            .FirstOrDefaultAsync(g => g.Id == id);

    public async Task<IEnumerable<T>> GetAllWithChildrenAsync()
    {
        var results = await _db.Table<T>()
            .ToListAsync();
        List<T> games = [];

        foreach (var game in results)
        {
            await _db.GetChildrenAsync(game);
            games.Add(game);
        }

        return games;
    }

    public async Task CreateAsync(T game)
        => await _db.InsertAsync(game);

    public async Task<IEnumerable<T>> GetAllAsync()
        => await _db.Table<T>()
            .ToListAsync();

    public async Task DeleteAsync(Guid id, bool recursive = false)
    {
        if (!recursive)
        {
            await _db.DeleteAsync<T>(id);
            return;
        }

        var game = await _db.Table<T>()
            .FirstOrDefaultAsync(g => g.Id == id);

        if (game is null)
        {
            return;
        }

        await _db.GetChildrenAsync(game);
        await _db.DeleteAsync(game, true);
    }

    public async Task<T?> GetWithChildrenAsync(Guid id)
    {
        var game = await _db.Table<T>()
            .FirstOrDefaultAsync(g => g.Id == id);

        if (game is null)
        {
            return null;
        }

        await _db.GetChildrenAsync(game);
        return game;
    }

    public async Task UpdateAsync(T game)
    {
        await _db.UpdateAsync(game);
    }
}
