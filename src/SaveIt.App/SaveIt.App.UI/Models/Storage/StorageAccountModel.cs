using SaveIt.App.Domain.Enums;

namespace SaveIt.App.UI.Models.StorageAccounts;
public class StorageAccountModel
{
    public required Guid Id { get; set; }
    public required string Email { get; set; }
    public required bool IsAuthorized { get; set; }
    public required StorageAccountType Type { get; set; }
    public required int GameSavesCount { get; set; }
}
