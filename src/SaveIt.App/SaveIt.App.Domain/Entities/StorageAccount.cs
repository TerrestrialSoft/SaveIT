using SaveIt.App.Domain.Enums;

namespace SaveIt.App.Domain.Entities;
public class StorageAccount
{
    public Guid Id { get; init; }
    public string Email { get; init; } = default!;
    public StorageAccountType Type { get; init; }
    public bool IsActive { get; set; }
}
