using FluentResults;

namespace SaveIt.App.Domain.Services;
public interface IFileService
{
    Result DecompressFile(string localGameSavePath, Stream value);
    Result<MemoryStream> GetCompressedFile(string localGameSavePath);
}
