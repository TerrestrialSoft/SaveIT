using SaveIt.App.Domain;
using SaveIt.App.Domain.Entities;
using SQLite;

namespace SaveIt.App.Persistence;
public class DatabaseHandler : IDatabaseHandler
{
    private readonly string _databasePath;

    public DatabaseHandler(IApplicationContext contextProvider)
    {
        _databasePath = contextProvider.DatabasePath;

        if (_databasePath is null)
            throw new InvalidOperationException("Database path is not set.");

        using var db = new SQLiteConnection(_databasePath);
        CreateTables(db);
    }

    private static void CreateTables(SQLiteConnection db)
    {
        db.CreateTable<ImageEntity>();
        db.CreateTable<Game>();
        db.CreateTable<GameSave>();
        db.CreateTable<StorageAccount>();
    }

    public SQLiteConnection CreateConnection()
        => new(_databasePath);

    public SQLiteAsyncConnection CreateAsyncConnection()
        => new(_databasePath);
}
