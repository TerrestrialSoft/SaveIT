using SaveIt.App.Domain.Entities;

namespace SaveIt.App.Domain.Repositories;
public interface IStorageAccountRepository : IRepository<StorageAccount>
{
    Task<IEnumerable<StorageAccount>> GetAccountsWithEmailAsync(string email);
    Task<IEnumerable<StorageAccount>> GetAllAuthorizedAccountsAsync();
    Task DeactiveAccountAsync(Guid id);
}
