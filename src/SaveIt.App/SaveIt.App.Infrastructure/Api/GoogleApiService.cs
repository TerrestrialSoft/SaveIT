using FluentResults;
using SaveIt.App.Domain.Auth;
using SaveIt.App.Domain.Errors;
using SaveIt.App.Domain.Models;
using SaveIt.App.Domain.Repositories;
using SaveIt.App.Infrastructure.Extensions;
using SaveIt.App.Infrastructure.Models;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace SaveIt.App.Infrastructure.Api;
public class GoogleApiService(HttpClient _httpClient, ISaveItApiService _saveItService,
    IAccountSecretsService _accountSecretsRepo) : IExternalStorageService
{
    private const string _profileUrl = "about?fields=user";
    private const string _baseFilesUrl = "files";
    private const string _baseFileDetailUrl = _baseFilesUrl + "/{0}";
    private const string _fileQueryfields = "id, name, parents, kind, mimeType";
    private const string _baseFileQueryUrl = _baseFilesUrl + "?fields=files("+ _fileQueryfields + ")" +
        "&q=trashed=false and mimeType='application/vnd.google-apps.folder'";
    private const string _filesWithSpecificParentUrl = _baseFileQueryUrl + "and '{0}' in parents";
    private const string _fileDetailUrl = _baseFileDetailUrl + "?fields=" + _fileQueryfields;
    private const string _fileUploadMultipartUrl = _baseFilesUrl + "?uploadType=multipart";

    public async Task<Result<string>> GetProfileEmailAsync(string accessToken)
    {
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var response = await _httpClient.GetAsync(_profileUrl);
        
        if (response.IsSuccessStatusCode)
        {
            return Result.Fail("Error ocurred during communication with external server");
        }

        var content = await response.Content.ReadFromJsonAsync<GoogleProfile>();
        
        if (content is null)
        {
            return Result.Fail("Error ocurred during communication with external server");
        }
        
        return content.User.Email;
    }

    public Task<Result<IEnumerable<FileItem>>> GetFoldersAsync(Guid storageAccountId, string parentId)
    {
        var filter = string.Format(_filesWithSpecificParentUrl, parentId);

        return GetFilesWithFilter(storageAccountId, filter);
    }

    private async Task<Result<IEnumerable<FileItem>>> GetFilesWithFilter(Guid storageAccountId, string filter)
    {
        var contentResult = await GetAsync<GoogleFileListModel>(storageAccountId, filter);

        if(contentResult.IsFailed)
        {
            return contentResult.ToResult();
        }

        var files = contentResult.Value.Files.Select(x => x.ToFileItem());

        return Result.Ok(files);
    }

    public async Task<Result<FileItem>> GetFolderAsync(Guid storageAccountId, string fileId)
    {
        var filter = string.Format(_fileDetailUrl, fileId);
        var contentResult = await GetAsync<GoogleFileModel>(storageAccountId, filter);

        return contentResult.IsSuccess
            ? Result.Ok(contentResult.Value.ToFileItem())
            : Result.Fail(contentResult.Errors);
    }

    public async Task<Result> CreateRepositoryAsync(Guid storageAccountId, string? parentId = null)
    {
        var result = await CreateFolderAsync(storageAccountId, "SaveIt", parentId);

        return result.ToResult();
    }

    public async Task<Result> DeleteFileAsync(Guid storageAccountId, string id)
    {
        var filter = string.Format(_baseFileDetailUrl, id);
        var messageFactory = new Func<HttpRequestMessage>(() => new HttpRequestMessage(HttpMethod.Delete, filter));

        var result = await ExecuteRequestAsync(storageAccountId, messageFactory);

        return result;
    }

    private async Task<Result<GoogleFileModel>> CreateFolderAsync(Guid storageAccountId, string name, string? parentId = null)
    {
        var folderMetadata = new
        {
            name,
            mimeType = "application/vnd.google-apps.folder",
            parents = parentId is not null 
                ? new[] { parentId }
                : null,
        };

        var messageFactory = new Func<HttpRequestMessage>(() => new HttpRequestMessage(HttpMethod.Post, _fileUploadMultipartUrl)
        {
            Content = new StringContent(JsonSerializer.Serialize(folderMetadata))
        });


        var result = await ExecuteRequestAsync<GoogleFileModel>(storageAccountId, messageFactory);

        return result;
    }

    private Task<Result<T>> GetAsync<T>(Guid storageAccountId, string url)
    { 
        var messageFactory = new Func<HttpRequestMessage>(() => new HttpRequestMessage(HttpMethod.Get, url));
        return ExecuteRequestAsync<T>(storageAccountId, messageFactory);
    }

    private async Task<Result> ExecuteRequestAsync(Guid storageAccountId, Func<HttpRequestMessage> requestMessage)
    {
        var responseResult = await GetSuccessResponseMessage(storageAccountId, requestMessage);
        return responseResult.ToResult();
    }

    private async Task<Result<T>> ExecuteRequestAsync<T>(Guid storageAccountId, Func<HttpRequestMessage> requestMessage)
    {
        var responseResult = await GetSuccessResponseMessage(storageAccountId, requestMessage);

        if (responseResult.IsFailed)
        {
            return responseResult.ToResult();
        }

        var content = await responseResult.Value.Content.ReadFromJsonAsync<T>();

        return content is not null
            ? content
            : Result.Fail("Error ocurred during communication with external server");
    }

    private async Task<Result<HttpResponseMessage>> GetSuccessResponseMessage(Guid storageAccountId,
        Func<HttpRequestMessage> requestMessage)
    {
        var token = await _accountSecretsRepo.GetAccessTokenAsync(storageAccountId);

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var response = await _httpClient.SendAsync(requestMessage());

        if (response.StatusCode == HttpStatusCode.Unauthorized || response.StatusCode == HttpStatusCode.Forbidden)
        {
            var result = await RetryRequestAsync(storageAccountId, requestMessage);

            if (result.IsFailed)
            {
                return Result.Fail(result.Errors);
            }

            response = result.Value;
        }

        return response.IsSuccessStatusCode
            ? response
            : Result.Fail("Error ocurred during communication with external server");
    }

    private async Task<Result<HttpResponseMessage>> RetryRequestAsync(Guid storageAccountId, Func<HttpRequestMessage> requestMessage)
    {
        var refreshResult = await TryRefreshTokenAsync(storageAccountId);

        if (refreshResult.IsFailed)
        {
            return refreshResult.ToResult();
        }

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", refreshResult.Value);

        var result = await _httpClient.SendAsync(requestMessage());

        return Result.Ok(result);
    }

    private async Task<Result<string>> TryRefreshTokenAsync(Guid storageAccountId)
    {
        var refreshToken = await _accountSecretsRepo.GetRefreshTokenAsync(storageAccountId);

        if (refreshToken is null)
        {
            return Result.Fail("Refresh token not found");
        }

        string accessToken;
        try
        {
            accessToken = await _saveItService.RefreshAccessTokenAsync(refreshToken);
        }
        catch (Exception)
        {
            return Result.Fail(new AuthError("Unable to refresh access token"));
        }

        var storeResult = await _accountSecretsRepo.StoreAccessTokenAsync(storageAccountId, accessToken);

        return storeResult.IsSuccess
            ? Result.Ok(accessToken)
            : storeResult;
    }
}
