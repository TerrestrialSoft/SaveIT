using FluentResults;
using SaveIt.App.Domain.Models;

namespace SaveIt.App.Domain.Auth;
public interface IExternalStorageService
{
    Task<Result<FileItemModel>> GetFileAsync(Guid storageAccountId, string? fileId = null,
        CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<FileItemModel>>> GetFilesAsync(Guid storageAccountId, string parentId,
        CancellationToken cancellationToken = default);
    Task<Result<string>> GetProfileEmailAsync(string accessToken, CancellationToken cancellationToken = default);
    Task<Result> CreateFolderAsync(Guid storageAccountId, string name, string? parentId = null,
        CancellationToken cancellationToken = default);
    Task<Result> DeleteFileAsync(Guid storageAccountId, string id, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<FileItemModel>>> GetFilesWithNameAsync(Guid storageAccountId, string remoteLocationId, string name,
        CancellationToken cancellationToken = default);
    Task<Result> CreateFileAsync(Guid storageAccountId, string fileName, object fileContent, string? parentId = null,
        CancellationToken cancellationToken = default);
    Task<Result<T?>> DownloadJsonFileAsync<T>(Guid storageAccountId, string fileId,
        CancellationToken cancellationToken = default);
    Task<Result> UpdateFileSimpleAsync(Guid storageAccountId, string id, object fileContent,
        CancellationToken cancellationToken = default);
    Task<Result> UploadFileAsync(Guid storageAccountId, string parentId, string fileName, MemoryStream value,
        CancellationToken cancellationToken = default);
    Task<Result<FileItemModel?>> GetNewestFileWithSubstringInNameAsync(Guid storageAccountId, string remoteLocationId,
        string substring, CancellationToken cancellationToken = default);
    Task<Result<Stream>> DownloadFileAsync(Guid storageAccountId, string fileId, CancellationToken cancellationToken = default);
    Task<Result> ShareFileWithUserAsync(Guid storageAccountId, string fileId, string email,
        CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<ShareWithModel>>> GetSharedWithUsersForFile(Guid storageAccountId, string fileId,
        CancellationToken cancellationToken = default);
    Task<Result> StopSharingFileWithUserAsync(Guid storageAccountId, string remoteFileId, string permissionId,
        CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<FileItemModel>>> GetFilesWithSubstringInNameAsync(Guid storageAccountId, string remoteLocationId,
        string name, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<FileItemModel>>> GetFilesWithSubstringInNameOrderedByDateAscAsync(Guid storageAccountId,
        string remoteLocationId, string name, CancellationToken cancellationToken = default);
}
