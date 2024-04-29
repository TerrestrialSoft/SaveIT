using SaveIt.App.Domain;
using SaveIt.App.Domain.Entities;
using SaveIt.App.Domain.Repositories;

namespace SaveIt.App.Persistence.Repositories;
public class StorageAccountRepository(IDatabaseHandler _dbHandler)
    : BaseRepository<StorageAccount>(_dbHandler), IStorageAccountRepository
{
    public async Task<IEnumerable<StorageAccount>> GetAccountsWithEmailAsync(string email)
        => await _db.Table<StorageAccount>()
            .Where(x => x.Email == email)
            .ToListAsync();
    public async Task DeactiveAccountAsync(Guid id)
    {
        var account = await _db.Table<StorageAccount>()
            .FirstOrDefaultAsync(x => x.Id == id);

        if (account is null)
            return;

        account.IsActive = false;

        await _db.UpdateAsync(account);
    }
}
