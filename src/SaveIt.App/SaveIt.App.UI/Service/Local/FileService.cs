using FluentResults;
using SaveIt.App.Domain.Errors;
using SaveIt.App.Domain.Services;
using System.IO.Compression;

namespace SaveIt.App.UI.Service.Local;
public class FileService : IFileService
{
    public Result DecompressFile(string localGameSavePath, Stream value)
    {
        Guid guid = Guid.NewGuid();
        try
        {
            ZipFile.ExtractToDirectory(value, localGameSavePath + guid);
            Directory.Delete(localGameSavePath, true);
            Directory.Move(localGameSavePath + guid, localGameSavePath);
        }
        catch (DirectoryNotFoundException)
        {
            return Result.Fail(FileErrors.LocationNotFound());
        }
        catch (Exception)
        {
            return Result.Fail(FileErrors.General());
        }

        return Result.Ok();
    }

    public Result<MemoryStream> GetCompressedFile(string localGameSavePath)
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
