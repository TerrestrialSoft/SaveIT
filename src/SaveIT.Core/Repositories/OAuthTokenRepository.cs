using SaveIT.Core.Entities;
using SaveIT.Storage;
using System.Linq.Expressions;

namespace SaveIT.Core.Repositories;
public class OAuthTokenRepository : IRepository<OAuthToken>
{
	private readonly IStorage<OAuthToken> _storage;

	public OAuthTokenRepository(IStorage<OAuthToken> storage)
	{
		_storage = storage;
	}

	public Task<OAuthToken> CreateAsync(OAuthToken item)
		=> _storage.CreateAsync(item);

	public Task DeleteAsync(Expression<Func<OAuthToken, bool>> exp)
		=> _storage.DeleteAsync(exp);

	public Task<IEnumerable<OAuthToken>> GetAllAsync()
		=> _storage.GetAllAsync();

	public Task<OAuthToken?> GetAsync(Expression<Func<OAuthToken, bool>> exp)
		=> _storage.GetAsync(exp);

	public Task<IEnumerable<OAuthToken>> GetManyAsync(Expression<Func<OAuthToken, bool>> exp)
		=> _storage.GetManyAsync(exp);

	public Task UpdateAsync(Expression<Func<OAuthToken, bool>> exp, OAuthToken item)
		=> _storage.UpdateAsync(exp, item);
}
