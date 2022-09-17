using SQLite;
using System.Linq.Expressions;

namespace SaveIT.Core.Storage
{
    public class SQLiteStorage<T> : IStorage<T> where T : new()
    {
        // This doesn't work untill MAUI 8.0, Nov 10
        // https://github.com/dotnet/runtime/issues/74884
        //private static string dataDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "SaveIT");
        private static string dataDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "SaveIT");
        private const string fileName = "data.db";
        private static string filePath = Path.Combine(dataDirectory, fileName);

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

        public async Task CreateAsync(T item)
            => await _connection.InsertAsync(item);

        public async Task DeleteAsync(Expression<Func<T, bool>> exp)
            => await _connection.DeleteAsync(exp);

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
            => await _connection.Table<T>()
                .Where(exp)
                .ToListAsync();

    }
}
