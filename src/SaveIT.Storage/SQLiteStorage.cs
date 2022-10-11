using SQLite;
using System.Linq.Expressions;

namespace SaveIT.Storage;

public class SQLiteStorage<T> : IStorage<T> where T : new()
{
	private static readonly string dataDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
	private const string fileName = "data.db";
	private static readonly string filePath = Path.Combine(dataDirectory, fileName);

	private readonly SQLiteAsyncConnection _connection;

	public SQLiteStorage()
	{
		EnsureDirectoryExists();

		_connection = new SQLiteAsyncConnection(filePath, SQLiteOpenFlags.Create | SQLiteOpenFlags.ReadWrite);
		_ = _connection.CreateTableAsync<T>().Result;
	}

	private static void EnsureDirectoryExists()
	{
		if (Directory.Exists(dataDirectory))
			return;

		Directory.CreateDirectory(dataDirectory);
	}

	public async Task<T> CreateAsync(T item)
	{
		await _connection.InsertAsync(item);
		return item;
	}

	public async Task DeleteAsync(Expression<Func<T, bool>> exp)
		=> await _connection.Table<T>().DeleteAsync(exp);

	public async Task<T?> GetAsync(Expression<Func<T, bool>> exp)
		=> await _connection.Table<T>()
			.FirstOrDefaultAsync(exp);

	public async Task<IEnumerable<T>> GetAllAsync()
		=> await _connection.Table<T>()
			.ToListAsync();

	public async Task<IEnumerable<T>> GetManyAsync(Expression<Func<T, bool>> exp)
		=> await _connection.Table<T>()
			.Where(exp)
			.ToListAsync();

	public async Task UpdateAsync(Expression<Func<T, bool>> exp, T item)
		=> await _connection.UpdateAsync(item);

}
