using SaveIt.App.Domain.Entities;

namespace SaveIt.App.Domain.Repositories;
public interface IStorageAccountRepository
{
    Task AddAccountAsync(StorageAccount account);
    Task<IEnumerable<StorageAccount>> GetAccountsWithEmailAsync(string email);
    Task<IEnumerable<StorageAccount>> GetAllStorageAccounts();
}
