using SaveIt.App.Domain.Entities;

namespace SaveIt.App.Domain.Repositories;
public interface IStorageAccountRepository
{
    Task<IEnumerable<StorageAccount>> GetAllStorageAccounts();
}
