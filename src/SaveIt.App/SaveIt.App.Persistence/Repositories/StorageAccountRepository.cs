using SaveIt.App.Domain.Entities;
using SaveIt.App.Domain.Repositories;

namespace SaveIt.App.Persistence.Repositories;
public class StorageAccountRepository : IStorageAccountRepository
{
    public Task<IEnumerable<StorageAccount>> GetAllStorageAccounts()
    {
        return Task.FromResult(new List<StorageAccount>
        {
            new StorageAccount { Name = "Storage Account 1" },
            new StorageAccount { Name = "Storage Account 2" }
        }.AsEnumerable());
    }
}
