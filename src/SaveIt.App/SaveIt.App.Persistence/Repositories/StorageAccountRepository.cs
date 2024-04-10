using SaveIt.App.Domain;
using SaveIt.App.Domain.Entities;
using SaveIt.App.Domain.Repositories;
using SQLite;

namespace SaveIt.App.Persistence.Repositories;
public class StorageAccountRepository(IDatabaseHandler _dbHandler) : IStorageAccountRepository
{
    private readonly SQLiteAsyncConnection _db = _dbHandler.CreateAsyncConnection();

    public async Task CreateAccountAsync(StorageAccount account)
        => await _db.InsertAsync(account);

    public async Task<IEnumerable<StorageAccount>> GetAccountsWithEmailAsync(string email)
        => await _db.Table<StorageAccount>()
            .Where(x => x.Email == email)
            .ToListAsync();

    public async Task<IEnumerable<StorageAccount>> GetAllStorageAccounts(bool includeDeactivated = false)
    {
        var table = _db.Table<StorageAccount>();

        if(includeDeactivated)
            table.Where(x => x.IsActive);

        return await table.ToListAsync();
    }

    public async Task DeactiveAccountAsync(Guid id)
    {
        var account = await _db.Table<StorageAccount>()
            .FirstOrDefaultAsync(x => x.Id == id);

        if (account is null)
            return;

        account.IsActive = false;

        await _db.UpdateAsync(account);
    }

    public async Task UpdateAccountAsync(StorageAccount account)
        => await _db.UpdateAsync(account);
}
