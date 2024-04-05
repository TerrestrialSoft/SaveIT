using SaveIt.App.Domain;
using SaveIt.App.Domain.Entities;
using SaveIt.App.Domain.Repositories;
using SQLite;

namespace SaveIt.App.Persistence.Repositories;
public class StorageAccountRepository(IDatabaseHandler _dbHandler) : IStorageAccountRepository
{
    private readonly SQLiteAsyncConnection _db = _dbHandler.CreateAsyncConnection();

    public async Task AddAccountAsync(StorageAccount account)
    {
        await _db.InsertAsync(account);
    }

    public async Task<IEnumerable<StorageAccount>> GetAccountsWithEmailAsync(string email)
    {
        return await _db.Table<StorageAccount>()
            .Where(x => x.Email == email)
            .ToListAsync();
    }

    public async Task<IEnumerable<StorageAccount>> GetAllStorageAccounts()
    {
        return await _db.Table<StorageAccount>()
            .ToListAsync();
    }
}
