﻿using SQLite;
using SQLiteNetExtensions.Attributes;

namespace SaveIt.App.Domain.Entities;

[Table("Games")]
public class Game : BaseEntity
{
    public string Name { get; set; } = default!;
    public string Username { get; set; } = default!;
    public string? GameExecutablePath { get; set; }

    [ForeignKey(typeof(ImageEntity))]
    public Guid? ImageId { get; set; }

    [OneToOne(CascadeOperations = CascadeOperation.All)]
    public ImageEntity? Image { get; set; }

    [OneToMany(CascadeOperations = CascadeOperation.All)]
    public List<GameSave>? GameSaves { get; set; }


}
