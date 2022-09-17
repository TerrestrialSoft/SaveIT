using System.Linq.Expressions;

namespace SaveIT.Core.Repositories
{
    public interface IRepository<T>
    {
        Task<IEnumerable<T>> GetManyAsync(Expression<Func<T, bool>> exp);
        Task<IEnumerable<T>> GetAllAsync();
        Task<T?> GetAsync(Expression<Func<T, bool>> exp);
        Task CreateAsync(T item);
        Task UpdateAsync(Expression<Func<T, bool>> exp, T item);
        Task DeleteAsync(Expression<Func<T, bool>> exp);
    }
}
