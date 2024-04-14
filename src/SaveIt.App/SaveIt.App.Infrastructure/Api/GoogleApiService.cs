using FluentResults;
using SaveIt.App.Domain.Auth;
using SaveIt.App.Domain.Models;
using SaveIt.App.Domain.Repositories;
using SaveIt.App.Infrastructure.Extensions;
using SaveIt.App.Infrastructure.Models;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace SaveIt.App.Infrastructure.Api;
public class GoogleApiService(HttpClient httpClient, IAccountSecretsService accountSecretsRepo, ISaveItApiService saveItService,
    GoogleApiUploadService _googleApiUploadService)
    : BaseApiService(httpClient, accountSecretsRepo, saveItService), IExternalStorageService
{
    private const string _profileUrl = "about?fields=user";
    private const string _baseFilesUrl = "files";
    private const string _baseFileDetailUrl = _baseFilesUrl + "/{0}";
    private const string _fileQueryfields = "id, name, parents, kind, mimeType";
    private const string _baseFileQueryUrl = $"{_baseFilesUrl}?fields=files({_fileQueryfields})" +
        $"&q=trashed=false";
    private const string _filesFolderWithSpecificParentUrl = $"{_baseFileQueryUrl} and mimeType='{_mimeTypeFolder}'" +
        "and '{0}' in parents";
    private const string _fileDetailUrl = $"{_baseFileDetailUrl}?fields=" + _fileQueryfields;
    private const string _findFileWithNameAndParentUrl = _baseFileQueryUrl + " and name='{1}' and '{0}' in parents";
    private const string _mimeTypeFolder = "application/vnd.google-apps.folder";
    private const string _fileDownloadUrl = $"{_baseFileDetailUrl}?alt=media";

    public async Task<Result<string>> GetProfileEmailAsync(string accessToken)
    {
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var response = await _httpClient.GetAsync(_profileUrl);
        
        if (!response.IsSuccessStatusCode)
        {
            return Result.Fail("Error ocurred during communication with external server");
        }

        var content = await response.Content.ReadFromJsonAsync<GoogleProfile>();

        return content is not null
            ? content.User.Email
            : Result.Fail("Error ocurred during communication with external server");
    }

    public Task<Result<IEnumerable<FileItemModel>>> GetFoldersAsync(Guid storageAccountId, string parentId)
    {
        var filter = string.Format(_filesFolderWithSpecificParentUrl, parentId);

        return GetFilesWithFilter(storageAccountId, filter);
    }

    private async Task<Result<IEnumerable<FileItemModel>>> GetFilesWithFilter(Guid storageAccountId, string filter)
    {
        var contentResult = await GetAsync<GoogleFileListModel>(storageAccountId, filter);

        if(contentResult.IsFailed)
        {
            return contentResult.ToResult();
        }

        var files = contentResult.Value.Files.Select(x => x.ToFileItem());

        return Result.Ok(files);
    }

    public async Task<Result<FileItemModel>> GetFolderAsync(Guid storageAccountId, string fileId)
    {
        var filter = string.Format(_fileDetailUrl, fileId);
        var contentResult = await GetAsync<GoogleFileModel>(storageAccountId, filter);

        return contentResult.IsSuccess
            ? Result.Ok(contentResult.Value.ToFileItem())
            : Result.Fail(contentResult.Errors);
    }

    public async Task<Result> CreateRepositoryAsync(Guid storageAccountId, string? parentId = null)
    {
        var folderMetadata = new GoogleFileCreateModel
        {
            Name = "SaveIt",
            MimeType = _mimeTypeFolder,
            Parents = parentId is not null ? new[] { parentId } : null
        };

        var messageFactory = new Func<HttpRequestMessage>(() => new HttpRequestMessage(HttpMethod.Post, _baseFilesUrl)
        {
            Content = new StringContent(JsonSerializer.Serialize(folderMetadata))
        });

        var result = await ExecuteRequestAsync<GoogleFileModel>(storageAccountId, messageFactory);

        return result.ToResult();
    }

    public async Task<Result> DeleteFileAsync(Guid storageAccountId, string id)
    {
        var filter = string.Format(_baseFileDetailUrl, id);
        var messageFactory = new Func<HttpRequestMessage>(() => new HttpRequestMessage(HttpMethod.Delete, filter));

        var result = await ExecuteRequestAsync(storageAccountId, messageFactory);

        return result;
    }

    public async Task<Result<IEnumerable<FileItemModel>>> GetFilesWithNameAsync(Guid storageAccountId, string remoteLocationId,
        string name)
    {
        var filter = string.Format(_findFileWithNameAndParentUrl, remoteLocationId, name);
        var contentResult = await GetAsync<GoogleFileListModel>(storageAccountId, filter);

        return contentResult.IsSuccess
            ? Result.Ok(contentResult.Value.Files.Select(x => x.ToFileItem()))
            : contentResult.ToResult();
    }

    public async Task<Result<T?>> DownloadFileAsync<T>(Guid storageAccountId, string fileId)
    {
        var filter = string.Format(_fileDownloadUrl, fileId);
        var messageFactory = new Func<HttpRequestMessage>(() => new HttpRequestMessage(HttpMethod.Get, filter));

        var result = await DownloadContentAsStringAsync(storageAccountId, messageFactory);

        if (result.IsFailed)
        {
            return result.ToResult();
        }

        var content = JsonSerializer.Deserialize<T>(result.Value);

        return Result.Ok(content);
    }

    public Task<Result> CreateFileAsync(Guid storageAccountId, string fileName, object fileContent, string? parentId = null)
        => _googleApiUploadService.CreateFileAsync(storageAccountId, fileName, fileContent, parentId);
}
