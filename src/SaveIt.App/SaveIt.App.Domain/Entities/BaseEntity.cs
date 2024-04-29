using SQLite;

namespace SaveIt.App.Domain.Entities;
public class BaseEntity
{
    [PrimaryKey]
    public Guid Id { get; init; }
}
