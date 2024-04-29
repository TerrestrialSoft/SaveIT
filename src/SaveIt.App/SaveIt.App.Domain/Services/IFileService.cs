using FluentResults;

namespace SaveIt.App.Domain.Services;
public interface IFileService
{
    Result DecompressFile(string localGameSavePath, Stream value, string? fileName = null);
    Result<MemoryStream> GetCompressedFile(string localGameSavePath);
}
