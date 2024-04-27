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
    Task<Result<T?>> DownloadJsonFileAsync<T>(Guid storageAccountId, string fileId);
    Task<Result> UpdateFileSimpleAsync(Guid storageAccountId, string id, object fileContent);
    Task<Result> UploadFileAsync(Guid storageAccountId, string parentId, string fileName, MemoryStream value);
    Task<Result<FileItemModel?>> GetNewestFileWithSubstringInNameAsync(Guid storageAccountId, string remoteLocationId,
        string substring);
    Task<Result<Stream>> DownloadFileAsync(Guid storageAccountId, string fileId);
    Task<Result> ShareFileWithUserAsync(Guid storageAccountId, string fileId, string email);
    Task<Result<IEnumerable<ShareWithModel>>> GetSharedWithUsersForFile(Guid storageAccountId, string fileId);
    Task<Result> StopSharingFileWithUserAsync(Guid storageAccountId, string remoteFileId, string permissionId);
}
