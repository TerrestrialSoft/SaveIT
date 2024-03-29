using SQLite;
using SQLiteNetExtensions.Attributes;

namespace SaveIt.App.Domain.Entities;
public class Game
{
    [PrimaryKey]
    public Guid Id { get; init; }

    public string Name { get; set; } = default!;
    public string Username { get; set; } = default!;
    public Guid? ImageId { get; set; }
    public string? ApplicationExecutablePath { get; set; }
    public bool IsFavourite { get; set; }

    [OneToMany]
    public ICollection<GameSave> GameSaves { get; set; } = [];
}
