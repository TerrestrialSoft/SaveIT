using SaveIt.App.Domain.Enums;
using SQLite;
using SQLiteNetExtensions.Attributes;

namespace SaveIt.App.Domain.Entities;

[Table("StorageAccounts")]
public class StorageAccount : BaseEntity
{
    public string Email { get; init; } = default!;
    public StorageAccountType Type { get; init; }
    public bool IsAuthorized { get; set; }

    [ManyToOne(CascadeOperations = CascadeOperation.CascadeRead)]
    public List<GameSave>? GameSaves { get; set; }
}
