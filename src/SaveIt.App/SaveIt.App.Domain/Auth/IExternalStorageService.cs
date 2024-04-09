using FluentResults;
using SaveIt.App.Domain.Models;

namespace SaveIt.App.Domain.Auth;
public interface IExternalStorageService
{
    Task<Result<FileItem>> GetFolderAsync(Guid storageAccountId, string fileId);
    Task<Result<IEnumerable<FileItem>>> GetFoldersAsync(Guid storageAccountId, string parentId);
    Task<string> GetProfileEmailAsync(string accessToken);
}
