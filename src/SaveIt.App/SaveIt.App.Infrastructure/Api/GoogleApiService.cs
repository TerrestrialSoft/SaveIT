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

namespace SaveIt.App.Infrastructure.Api;
public class GoogleApiService(HttpClient _httpClient, ISaveItApiService _saveItService,
    IAccountSecretsService _accountSecretsRepo) : IExternalStorageService
{
    private const string _profileUrl = "about?fields=user";
    private const string _baseFileUrl = "files?fields=files(id, name, parents, kind, mimeType)&q=trashed=false and " +
        "mimeType='application/vnd.google-apps.folder'";
    private const string _filesWithSpecificParentUrl = _baseFileUrl + "and '{0}' in parents";
    private const string _fileDetailUrl = "files/{0}?fields=id, name, parents, kind, mimeType";

    public async Task<string> GetProfileEmailAsync(string accessToken)
    {
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var response = await _httpClient.GetAsync(_profileUrl);
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadFromJsonAsync<GoogleProfile>();
        ArgumentNullException.ThrowIfNull(content);
        
        return content.User.Email;
    }

    public Task<Result<IEnumerable<FileItem>>> GetFoldersAsync(Guid storageAccountId, string parentId)
    {
        var filter = string.Format(_filesWithSpecificParentUrl, parentId);

        return GetFilesWithFilter(storageAccountId, filter);
    }

    private async Task<Result<IEnumerable<FileItem>>> GetFilesWithFilter(Guid storageAccountId, string filter)
    {
        var contentResult = await GetResponseAsync<GoogleFileListModel>(storageAccountId, filter);

        if(contentResult.IsFailed)
        {
            return Result.Fail(contentResult.Errors);
        }

        var files = contentResult.Value.Files.Select(x => x.ToFileItem());

        return Result.Ok(files);
    }

    private async Task<Result<T>> GetResponseAsync<T>(Guid storageAccountId, string url)
    {
        var token = await _accountSecretsRepo.GetAccessTokenAsync(storageAccountId);

        if (token is null)
        {
            return Result.Fail("Token not found");
        }

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var response = await _httpClient.GetAsync(url);

        if (response.StatusCode == HttpStatusCode.Unauthorized || response.StatusCode == HttpStatusCode.Forbidden)
        {
            var refreshResult = await TryRefreshTokenAsync(storageAccountId);

            if (refreshResult.IsFailed)
            {
                return Result.Fail(refreshResult.Errors);
            }

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", refreshResult.Value);
            response = await _httpClient.GetAsync(_filesWithSpecificParentUrl);
        }

        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadFromJsonAsync<T>();
        ArgumentNullException.ThrowIfNull(content);

        return content;
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
        catch(Exception)
        {
            return Result.Fail(new AuthError("Unable to refresh access token"));
        }

        var storeResult = await _accountSecretsRepo.StoreAccessTokenAsync(storageAccountId, accessToken);

        return storeResult.IsSuccess
            ? Result.Ok(accessToken)
            : Result.Fail(storeResult.Errors);
    }

    public async Task<Result<FileItem>> GetFolderAsync(Guid storageAccountId, string fileId)
    {
        var filter = string.Format(_fileDetailUrl, fileId);
        var contentResult = await GetResponseAsync<GoogleFileModel>(storageAccountId, filter);

        return contentResult.IsSuccess
            ? Result.Ok(contentResult.Value.ToFileItem())
            : Result.Fail(contentResult.Errors);
    }
}
