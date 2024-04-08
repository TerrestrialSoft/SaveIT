using FluentResults;
using SaveIt.App.Domain.Auth;
using SaveIt.App.Domain.Errors;
using SaveIt.App.Domain.Models;
using SaveIt.App.Domain.Repositories;
using SaveIt.App.Infrastructure.Models;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace SaveIt.App.Infrastructure.Api;
public class GoogleApiService(HttpClient _httpClient, ISaveItApiService _saveItService,
    IAccountSecretsService _accountSecretsRepo) : IExternalStorageService
{
    private const string _profileUrl = "about?fields=user";
    private const string _filesUrl = "files?q=trashed=false and '{0}' in parents and mimeType = 'application/vnd.google-apps.folder'" +
        "&fields=files(id, name, parents, kind, mimeType)";

    private const string _parentId = "root";

    public async Task<string> GetProfileEmailAsync(string accessToken)
    {
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var response = await _httpClient.GetAsync(_profileUrl);
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadFromJsonAsync<GoogleProfile>();
        ArgumentNullException.ThrowIfNull(content);
        
        return content.User.Email;
    }

    public async Task<Result<IEnumerable<FileItem>>> GetFilesAsync(Guid storageAccountId, string? parentId = null)
    {
        parentId ??= _parentId;

        var token = await _accountSecretsRepo.GetAccessTokenAsync(storageAccountId);

        if(token is null)
        {
            return Result.Fail("Token not found");
        }

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var response = await _httpClient.GetAsync(string.Format(_filesUrl, parentId));

        if (response.StatusCode == HttpStatusCode.Unauthorized || response.StatusCode == HttpStatusCode.Forbidden)
        {
            var refreshResult = await TryRefreshTokenAsync(storageAccountId);

            if (refreshResult.IsFailed)
            {
                return Result.Fail(refreshResult.Errors);
            }

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", refreshResult.Value);
            response = await _httpClient.GetAsync(_filesUrl);
        }

        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadFromJsonAsync<GoogleFileListModel>();
        ArgumentNullException.ThrowIfNull(content);

        var files = content.Files.Select(x =>
        {
            var fileType = x.MimeType == "application/vnd.google-apps.folder"
                ? FileItemType.Folder
                : FileItemType.File;

            return new FileItem(x.Name, fileType, x.Parent, x.Id);
        });

        return Result.Ok(files);
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
}
