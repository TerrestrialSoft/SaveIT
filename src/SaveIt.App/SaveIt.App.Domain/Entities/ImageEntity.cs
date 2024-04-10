using SQLite;

namespace SaveIt.App.Domain.Entities;

[Table("Images")]
public class ImageEntity : BaseEntity
{
    public string Name { get; init; } = default!;
    public string Content { get; init; } = default!;
}
