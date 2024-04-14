using SaveIt.App.Domain.Models;
using SaveIt.App.UI.Models;

namespace SaveIt.App.UI.Extensions;
public static class ModelExtensions
{
    public static RemoteFileItemModel ToRemoteFileItemModel(this FileItemModel item)
    {
        ArgumentNullException.ThrowIfNull(item.Id);

        return new RemoteFileItemModel
        {
            Id = item.Id,
            Name = item.Name,
            ParentId = item.ParentId,
            IsDirectory = item.FileType == FileItemType.Folder,
        };
    }
}
