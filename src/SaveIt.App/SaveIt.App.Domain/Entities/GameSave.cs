using SQLite;
using SQLiteNetExtensions.Attributes;

namespace SaveIt.App.Domain.Entities;

[Table("GameSaves")]
public class GameSave : BaseEntity
{
    public string Name { get; init; } = default!;
    public string LocalGameSavePath { get; init; } = default!;
    public string RemoteLocationId { get; init; } = default!;
    
    [ForeignKey(typeof(ImageEntity))]
    public Guid StorageAccountId { get; init; }

    [ForeignKey(typeof(Game))]
    public Guid GameId { get; init; }

    [ManyToOne]
    public Game Game { get; set; } = default!;

    [ManyToOne]
    public StorageAccount StorageAccount { get; set; } = default!;

}
