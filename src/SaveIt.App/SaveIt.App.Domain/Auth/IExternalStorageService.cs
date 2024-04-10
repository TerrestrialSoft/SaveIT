using FluentResults;
using SaveIt.App.Domain.Models;

namespace SaveIt.App.Domain.Auth;
public interface IExternalStorageService
{
    Task<Result<FileItem>> GetFolderAsync(Guid storageAccountId, string fileId);
    Task<Result<IEnumerable<FileItem>>> GetFoldersAsync(Guid storageAccountId, string parentId);
    Task<string> GetProfileEmailAsync(string accessToken);
    Task<Result> CreateRepositoryAsync(Guid storageAccountId, string? parentId = null);
    Task<Result> DeleteFileAsync(Guid storageAccountId, string id);
}
