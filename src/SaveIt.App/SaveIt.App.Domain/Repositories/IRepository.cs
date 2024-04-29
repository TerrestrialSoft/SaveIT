using SaveIt.App.Domain.Entities;

namespace SaveIt.App.Domain.Repositories;
public interface IRepository<T> where T : BaseEntity, new()
{
    Task CreateAsync(T game);
    Task DeleteAsync(Guid id, bool recursive = false);
    Task<IEnumerable<T>> GetAllAsync();
    Task<IEnumerable<T>> GetAllWithChildrenAsync();
    Task<T?> GetAsync(Guid id);
    Task<T?> GetWithChildrenAsync(Guid id);
    Task UpdateAsync(T game);
}