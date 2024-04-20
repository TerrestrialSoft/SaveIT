using SQLite;
using SQLiteNetExtensions.Attributes;

namespace SaveIt.App.Domain.Entities;

[Table("Games")]
public class Game : BaseEntity
{
    public string Name { get; init; } = default!;
    public string Username { get; init; } = default!;
    public string? GameExecutablePath { get; set; }

    [ForeignKey(typeof(ImageEntity))]
    public Guid? ImageId { get; set; }

    [OneToOne(CascadeOperations = CascadeOperation.CascadeDelete)]
    public ImageEntity? Image { get; set; }

    [OneToMany(CascadeOperations = CascadeOperation.CascadeDelete)]
    public List<GameSave>? GameSaves { get; set; }
}
