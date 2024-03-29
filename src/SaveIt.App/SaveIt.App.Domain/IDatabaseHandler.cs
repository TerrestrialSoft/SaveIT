using SQLite;

namespace SaveIt.App.Domain;
public interface IDatabaseHandler
{
    SQLiteAsyncConnection CreateAsyncConnection();
    SQLiteConnection CreateConnection();
}
