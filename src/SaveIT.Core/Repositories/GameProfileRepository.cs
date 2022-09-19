using SaveIT.Core.Entities;
using SaveIT.Core.Storage;
using System.Linq.Expressions;

namespace SaveIT.Core.Repositories;

public class GameProfileRepository : IRepository<GameProfile>
{
    private readonly IStorage<GameProfile> _storage;

    public GameProfileRepository(IStorage<GameProfile> storage)
    {
        _storage = storage;
    }

    public async Task CreateAsync(GameProfile item)
    {
        await _storage.CreateAsync(item);
    }

    public async Task UpdateAsync(Expression<Func<GameProfile, bool>> exp, GameProfile item)
    {
        await _storage.UpdateAsync(exp, item);
    }

    public async Task DeleteAsync(Expression<Func<GameProfile, bool>> exp)
    {
        await _storage.DeleteAsync(exp);
    }

    public async Task<GameProfile?> GetAsync(Expression<Func<GameProfile, bool>> exp)
    {
        return await _storage.GetAsync(exp);
    }

    public async Task<IEnumerable<GameProfile>> GetManyAsync(Expression<Func<GameProfile, bool>> exp)
    {
        return await _storage.GetManyAsync(exp);
    }

    public async Task<IEnumerable<GameProfile>> GetAllAsync()
    {
        return await _storage.GetAllAsync();
    }
}
