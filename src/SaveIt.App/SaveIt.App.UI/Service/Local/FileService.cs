using FluentResults;
using SaveIt.App.Domain.Errors;
using SaveIt.App.Domain.Services;
using System.IO.Compression;

namespace SaveIt.App.UI.Service.Local;
public class FileService : IFileService
{
    public Result<MemoryStream> GetCompressedFileAsync(string localGameSavePath)
    {
        MemoryStream ms = new MemoryStream();

        try
        {
            ZipFile.CreateFromDirectory(localGameSavePath, ms);
        }
        catch (DirectoryNotFoundException)
        {
            ms.Close();
            ms.Dispose();
            return Result.Fail(FileErrors.LocationNotFound());
        }
        catch (Exception)
        {
            ms.Close();
            ms.Dispose();
            return Result.Fail(FileErrors.General());
        }

        return ms;
    }
}
