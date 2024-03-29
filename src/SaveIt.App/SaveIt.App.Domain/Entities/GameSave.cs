using SQLite;
using SQLiteNetExtensions.Attributes;

namespace SaveIt.App.Domain.Entities;

public class GameSave
{
    [PrimaryKey]
    public Guid Id { get; init; }

    [ForeignKey(typeof(Game))]
    public Guid GameId { get; set; }

    public string Name { get; set; } = default!;
    public string LocalGameSavePath { get; set; } = default!;
    public string RemoteGameSavePath { get; set; } = default!;
    public Guid StorageAccountId { get; set; }

    [ManyToOne]
    public Game Game { get; set; } = default!;

}
