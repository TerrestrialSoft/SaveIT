using SaveIt.App.Domain.Entities;

namespace SaveIt.App.Domain.Repositories;
public interface IStorageAccountRepository
{
    Task CreateAccountAsync(StorageAccount account);
    Task<IEnumerable<StorageAccount>> GetAccountsWithEmailAsync(string email);
    Task<IEnumerable<StorageAccount>> GetAllStorageAccounts(bool includeDeactivated = false);
    Task DeactiveAccountAsync(Guid id);
    Task UpdateAccountAsync(StorageAccount account);
}
