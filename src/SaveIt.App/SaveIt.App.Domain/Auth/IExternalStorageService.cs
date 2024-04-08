using FluentResults;
using SaveIt.App.Domain.Models;

namespace SaveIt.App.Domain.Auth;
public interface IExternalStorageService
{
    Task<Result<IEnumerable<FileItem>>> GetFilesAsync(Guid storageAccountId, string? parentId = null);
    Task<string> GetProfileEmailAsync(string accessToken);
}
