using FluentResults;

namespace SaveIt.App.Domain.Services;
public interface IFileService
{
    Result<MemoryStream> GetCompressedFileAsync(string localGameSavePath);
}
