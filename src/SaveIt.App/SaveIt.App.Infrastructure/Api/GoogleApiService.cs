﻿using FluentResults;
using SaveIt.App.Domain.Auth;
using SaveIt.App.Domain.Models;
using SaveIt.App.Domain.Repositories;
using SaveIt.App.Infrastructure.Extensions;
using SaveIt.App.Infrastructure.Models;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace SaveIt.App.Infrastructure.Api;
public class GoogleApiService(HttpClient httpClient, IAccountSecretsRepository accountSecretsRepo,
    ISaveItApiService saveItService, GoogleApiUploadService _googleApiUploadService,
    IStorageAccountRepository storageAccountRepository)
    : BaseApiService(httpClient, accountSecretsRepo, saveItService, storageAccountRepository), IExternalStorageService
{
    private const string _profileUrl = "about?fields=user";
    private const string _baseFilesUrl = "files";
    private const string _baseFileDetailUrl = _baseFilesUrl + "/{0}";
    private const string _fileQueryfields = "id, name, parents, kind, mimeType, driveId, sharedWithMeTime";
    private const string _baseFileQueryUrl = $"{_baseFilesUrl}?{_baseParameters}";
    private const string _baseFileQueryUntrashedUrl = $"{_baseFileQueryUrl}&q=trashed=false";
    private const string _baseParameters = $"fields=files({_fileQueryfields})&supportsAllDrives=true";
    private const string _filesFolderWithSpecificParentUrl = $"{_baseFileQueryUntrashedUrl} and mimeType='{_mimeTypeFolder}'" +
        "and ('{0}' in parents{1})";
    private const string _sharedWithMeCondition = " or sharedWithMe = true";
    private const string _filesWithSubstringInNameSpecificParentOrderedByDateUrl
        = _baseFileQueryUrl + "&orderBy=modifiedTime&q=trashed=false and name contains '{1}' and '{0}' in parents";
    private const string _fileDetailUrl = $"{_baseFileDetailUrl}?fields=" + _fileQueryfields;
    private const string _findFileWithNameAndParentUrl = _baseFileQueryUntrashedUrl + " and name='{1}' and '{0}' in parents";
    private const string _findFileWithSubstringInNameAndParentUrl
        = _baseFileQueryUntrashedUrl + " and name contains '{1}' and '{0}' in parents";
    private const string _findLastModifiedFileWithNameAndParentUrl
        = _baseFileQueryUrl + "&orderBy=modifiedTime desc&q=trashed=false and name contains '{1}' and '{0}' in parents";
    private const string _mimeTypeFolder = "application/vnd.google-apps.folder";
    private const string _fileDownloadUrl = $"{_baseFileDetailUrl}?alt=media";
    private const string _filePermissionsUrlList = $"{_baseFileDetailUrl}/permissions?fields=permissions" +
        $"(id, displayName, type, kind, emailAddress, role)";
    private const string _filePermissionsUrl = $"{_baseFileDetailUrl}/permissions";
    private const string _fileDeletePermissionsUrl = _baseFileDetailUrl + "/permissions/{1}";

    public async Task<Result<string>> GetProfileEmailAsync(string accessToken, CancellationToken cancellationToken = default)
    {
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var response = await _httpClient.GetAsync(_profileUrl, cancellationToken);
        
        if (!response.IsSuccessStatusCode)
        {
            return Result.Fail("Error ocurred during communication with external server");
        }

        var content = await response.Content.ReadFromJsonAsync<GoogleProfile>(cancellationToken);

        return content is not null
            ? content.User.Email
            : Result.Fail("Error ocurred during communication with external server");
    }

    public Task<Result<IEnumerable<FileItemModel>>> GetFilesAsync(Guid storageAccountId, string parentId, bool sharedWithMe,
        CancellationToken cancellationToken = default)
    {
        var filter = string.Format(_filesFolderWithSpecificParentUrl, parentId, sharedWithMe ? _sharedWithMeCondition : "");

        return GetFilesWithFilter(storageAccountId, filter, cancellationToken);
    }

    private async Task<Result<IEnumerable<FileItemModel>>> GetFilesWithFilter(Guid storageAccountId, string filter,
        CancellationToken cancellationToken = default)
    {
        var contentResult = await GetAsync<GoogleFileListModel>(storageAccountId, filter, cancellationToken);

        if(contentResult.IsFailed)
        {
            return contentResult.ToResult();
        }

        var files = contentResult.Value.Files.Select(x => x.ToFileItem());

        return Result.Ok(files);
    }

    public async Task<Result<FileItemModel>> GetFileAsync(Guid storageAccountId, string? fileId = null,
        CancellationToken cancellationToken = default)
    {
        fileId ??= GoogleFileModel.RootParentId;

        var filter = string.Format(_fileDetailUrl, fileId);
        var contentResult = await GetAsync<GoogleFileModel>(storageAccountId, filter, cancellationToken);

        return contentResult.IsSuccess
            ? Result.Ok(contentResult.Value.ToFileItem())
            : Result.Fail(contentResult.Errors);
    }

    public async Task<Result> CreateFolderAsync(Guid storageAccountId, string name, string? parentId = null,
        CancellationToken cancellationToken = default)
    {
        var folderMetadata = new GoogleFileCreateModel
        {
            Name = name,
            MimeType = _mimeTypeFolder,
            Parents = parentId is not null ? new[] { parentId } : null
        };

        var messageFactory = new Func<HttpRequestMessage>(() => new HttpRequestMessage(HttpMethod.Post, _baseFilesUrl)
        {
            Content = new StringContent(JsonSerializer.Serialize(folderMetadata))
        });

        var result = await ExecuteRequestAsync<GoogleFileModel>(storageAccountId, messageFactory, cancellationToken);

        return result.ToResult();
    }

    public async Task<Result> DeleteFileAsync(Guid storageAccountId, string id, CancellationToken cancellationToken = default)
    {
        var filter = string.Format(_baseFileDetailUrl, id);
        var messageFactory = new Func<HttpRequestMessage>(() => new HttpRequestMessage(HttpMethod.Delete, filter));

        var result = await ExecuteRequestAsync(storageAccountId, messageFactory, cancellationToken);

        return result;
    }

    public async Task<Result<IEnumerable<FileItemModel>>> GetFilesWithNameAsync(Guid storageAccountId, string remoteLocationId,
        string name, CancellationToken cancellationToken = default)
    {
        var filter = string.Format(_findFileWithNameAndParentUrl, remoteLocationId, name);
        var contentResult = await GetAsync<GoogleFileListModel>(storageAccountId, filter, cancellationToken);

        return contentResult.IsSuccess
            ? Result.Ok(contentResult.Value.Files.Select(x => x.ToFileItem()))
            : contentResult.ToResult();
    }

    public async Task<Result<IEnumerable<FileItemModel>>> GetFilesWithSubstringInNameAsync(Guid storageAccountId,
        string remoteLocationId, string name, CancellationToken cancellationToken = default)
    {
        var filter = string.Format(_findFileWithSubstringInNameAndParentUrl, remoteLocationId, name);
        var contentResult = await GetAsync<GoogleFileListModel>(storageAccountId, filter, cancellationToken);

        return contentResult.IsSuccess
            ? Result.Ok(contentResult.Value.Files.Select(x => x.ToFileItem()))
            : contentResult.ToResult();
    }

    public async Task<Result<T?>> DownloadJsonFileAsync<T>(Guid storageAccountId, string fileId,
        CancellationToken cancellationToken = default)
    {
        var filter = string.Format(_fileDownloadUrl, fileId);
        var messageFactory = new Func<HttpRequestMessage>(() => new HttpRequestMessage(HttpMethod.Get, filter));

        var result = await DownloadContentAsStringAsync(storageAccountId, messageFactory, cancellationToken);

        if (result.IsFailed)
        {
            return result.ToResult();
        }

        var content = JsonSerializer.Deserialize<T>(result.Value);

        return Result.Ok(content);
    }

    public Task<Result> CreateFileAsync(Guid storageAccountId, string fileName, object fileContent, string? parentId = null,
        CancellationToken cancellationToken = default)
        => _googleApiUploadService.CreateFileSimpleAsync(storageAccountId, fileName, fileContent, parentId, cancellationToken);

    public Task<Result> UpdateFileSimpleAsync(Guid storageAccountId, string id, object fileContent,
        CancellationToken cancellationToken = default) 
        => _googleApiUploadService.UpdateFileSimpleAsync(storageAccountId, id, fileContent, cancellationToken);

    public Task<Result> UploadFileAsync(Guid storageAccountId, string parentId, string fileName, MemoryStream value,
        CancellationToken cancellationToken = default)
        => _googleApiUploadService.UploadFileAsync(storageAccountId, parentId, fileName, value, cancellationToken);

    public async Task<Result<FileItemModel?>> GetNewestFileWithSubstringInNameAsync(Guid storageAccountId,
        string remoteLocationId, string substring, CancellationToken cancellationToken = default)
    {
        var filter = string.Format(_findLastModifiedFileWithNameAndParentUrl, remoteLocationId, substring);
        var contentResult = await GetAsync<GoogleFileListModel>(storageAccountId, filter, cancellationToken);

        if (contentResult.IsFailed)
        {
            return contentResult.ToResult();
        }

        var file = contentResult.Value.Files
            .FirstOrDefault()?
            .ToFileItem();

        return Result.Ok(file);
    }

    public async Task<Result<Stream>> DownloadFileAsync(Guid storageAccountId, string fileId,
        CancellationToken cancellationToken = default)
    {
        var filter = string.Format(_fileDownloadUrl, fileId);
        var messageFactory = new Func<HttpRequestMessage>(() => new HttpRequestMessage(HttpMethod.Get, filter));

        return await DownloadContentAsStreamAsync(storageAccountId, messageFactory, cancellationToken);
    }

    public async Task<Result> ShareFileWithUserAsync(Guid storageAccountId, string fileId, string email,
        CancellationToken cancellationToken = default)
    {
        var url = string.Format(_filePermissionsUrl, fileId);
        var messageFactory = new Func<HttpRequestMessage>(() => new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = new StringContent(JsonSerializer.Serialize(new GooglePermissionCreateModel
            {
                Role = "writer",
                Type = "user",
                EmailAddress = email
            }))
        });

        var result = await ExecuteRequestAsync(storageAccountId, messageFactory, cancellationToken);

        return result;
    }

    public async Task<Result<IEnumerable<ShareWithModel>>> GetSharedWithUsersForFile(Guid storageAccountId, string fileId,
        CancellationToken cancellationToken = default)
    {
        var url = string.Format(_filePermissionsUrlList, fileId);
        var contentResult = await GetAsync<GooglePermissionListModel>(storageAccountId, url, cancellationToken);

        if (contentResult.IsFailed)
        {
            return contentResult.ToResult();
        }

        if(contentResult.Value.Permissions is null)
        {
            return Result.Ok(Enumerable.Empty<ShareWithModel>());
        }

        var result = contentResult.Value.Permissions.Select(x => new ShareWithModel
        {
            PermissionId = x.Id,
            Email = x.EmailAddress,
            Username = x.DisplayName,
            IsOwner = x.Role == GooglePermissionModel.OwnerRole
        });

        return Result.Ok(result);
    }

    public Task<Result> StopSharingFileWithUserAsync(Guid storageAccountId, string remoteFileId, string permissionId,
        CancellationToken cancellationToken = default)
    {
        var url = string.Format(_fileDeletePermissionsUrl, remoteFileId, permissionId);
        var messageFactory = new Func<HttpRequestMessage>(() => new HttpRequestMessage(HttpMethod.Delete, url));

        return ExecuteRequestAsync(storageAccountId, messageFactory, cancellationToken);
    }

    public Task<Result<IEnumerable<FileItemModel>>> GetFilesWithSubstringInNameOrderedByDateAscAsync(Guid storageAccountId,
        string remoteLocationId, string name, CancellationToken cancellationToken = default)
    {
        var filter = string.Format(_filesWithSubstringInNameSpecificParentOrderedByDateUrl, remoteLocationId, name);

        return GetFilesWithFilter(storageAccountId, filter, cancellationToken);
    }
}
