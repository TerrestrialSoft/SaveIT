using SQLite;
using SQLiteNetExtensions.Attributes;

namespace SaveIt.App.Domain.Entities;

[Table("GameSaves")]
public class GameSave : BaseEntity
{
    public string Name { get; set; } = default!;
    public string LocalGameSavePath { get; set; } = default!;
    public string RemoteLocationId { get; set; } = default!;
    public string RemoteLocationName { get; set; } = default!;

    [ForeignKey(typeof(ImageEntity))]
    public Guid StorageAccountId { get; set; }

    [ForeignKey(typeof(Game))]
    public Guid GameId { get; set; }

    [ManyToOne(CascadeOperations = CascadeOperation.CascadeRead)]
    public Game Game { get; set; } = default!;

    [ManyToOne(CascadeOperations = CascadeOperation.CascadeRead)]
    public StorageAccount StorageAccount { get; set; } = default!;

}
