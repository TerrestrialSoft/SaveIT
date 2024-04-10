using SQLite;

namespace SaveIt.App.Domain.Entities;
public abstract class BaseEntity
{
    [PrimaryKey]
    public Guid Id { get; init; }
}
