using System.Linq.Expressions;

namespace SaveIT.Storage;

public interface IStorage<T> where T : new()
{
	Task<IEnumerable<T>> GetManyAsync(Expression<Func<T, bool>> exp);
	Task<T?> GetAsync(Expression<Func<T, bool>> exp);
	Task<T> CreateAsync(T item);
	Task UpdateAsync(Expression<Func<T, bool>> exp, T item);
	Task DeleteAsync(Expression<Func<T, bool>> exp);
	Task<IEnumerable<T>> GetAllAsync();
}
