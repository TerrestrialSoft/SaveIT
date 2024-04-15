using FluentResults;
using SaveIt.App.Domain.Models;

namespace SaveIt.App.Domain.Auth;
public interface IExternalStorageService
{
    Task<Result<FileItemModel>> GetFolderAsync(Guid storageAccountId, string fileId);
    Task<Result<IEnumerable<FileItemModel>>> GetFoldersAsync(Guid storageAccountId, string parentId);
    Task<Result<string>> GetProfileEmailAsync(string accessToken);
    Task<Result> CreateFolderAsync(Guid storageAccountId, string name, string? parentId = null);
    Task<Result> DeleteFileAsync(Guid storageAccountId, string id);
    Task<Result<IEnumerable<FileItemModel>>> GetFilesWithNameAsync(Guid storageAccountId, string remoteLocationId, string name);
    Task<Result> CreateFileAsync(Guid storageAccountId, string fileName, object fileContent, string? parentId = null);
    Task<Result<T?>> DownloadFileAsync<T>(Guid storageAccountId, string fileId);
    Task<Result> UpdateFileSimpleAsync(Guid storageAccountId, string id, object fileContent);
}
