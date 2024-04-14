using SaveIt.App.Domain.Models;
using SaveIt.App.Infrastructure.Models;

namespace SaveIt.App.Infrastructure.Extensions;
public static class ModelExtensions
{
    public static FileItemModel ToFileItem(this GoogleFileModel file)
    {
        var fileType = file.MimeType == "application/vnd.google-apps.folder"
            ? FileItemType.Folder
            : FileItemType.File;

        return new FileItemModel(file.Name, fileType, file.Parent, file.Id);
    }
}
