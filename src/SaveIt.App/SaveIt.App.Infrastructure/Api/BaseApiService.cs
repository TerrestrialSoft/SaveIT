using FluentResults;
using System.Net.Http.Headers;
using System.Net;
using System.Net.Http.Json;
using SaveIt.App.Domain.Repositories;
using SaveIt.App.Domain.Errors;
using SaveIt.App.Domain.Auth;

namespace SaveIt.App.Infrastructure.Api;
public class BaseApiService(HttpClient httpClient, IAccountSecretsService accountSecretsRepo, ISaveItApiService saveItService)
{
    protected readonly HttpClient _httpClient = httpClient;
    protected readonly IAccountSecretsService _accountSecretsRepo = accountSecretsRepo;
    protected readonly ISaveItApiService _saveItService = saveItService;

    protected async Task<Result> ExecuteRequestAsync(Guid storageAccountId, Func<HttpRequestMessage> requestFactory)
    {
        var responseResult = await GetSuccessResponseMessage(storageAccountId, requestFactory);
        return responseResult.ToResult();
    }

    protected async Task<Result<T>> ExecuteRequestAsync<T>(Guid storageAccountId, Func<HttpRequestMessage> requestFactory)
    {
        var responseResult = await GetSuccessResponseMessage(storageAccountId, requestFactory);

        if (responseResult.IsFailed)
        {
            return responseResult.ToResult();
        }

        var content = await responseResult.Value.Content.ReadFromJsonAsync<T>();

        return content is not null
            ? content
            : Result.Fail("Error ocurred during communication with external server");
    }

    protected async Task<Result<string>> DownloadContentAsStringAsync(Guid storageAccountId,
        Func<HttpRequestMessage> requestFactory)
    {
        var responseResult = await GetSuccessResponseMessage(storageAccountId, requestFactory);

        if (responseResult.IsFailed)
        {
            return responseResult.ToResult();
        }

        var content = await responseResult.Value.Content.ReadAsStringAsync();

        return content is not null
            ? content
            : Result.Fail("Error ocurred during communication with external server");
    }

    protected async Task<Result<Stream>> DownloadContentAsStreamAsync(Guid storageAccountId,
        Func<HttpRequestMessage> requestFactory)
    {
        var responseResult = await GetSuccessResponseMessage(storageAccountId, requestFactory);

        if (responseResult.IsFailed)
        {
            return responseResult.ToResult();
        }

        var content = await responseResult.Value.Content.ReadAsStreamAsync();

        return content is not null
            ? content
            : Result.Fail("Error ocurred during communication with external server");
    }

    protected async Task<Result<string>> ExecuteRequestForHeaderAsync(Guid storageAccountId, string headerName,
        Func<HttpRequestMessage> requestFactory)
    {
        var responseResult = await GetSuccessResponseMessage(storageAccountId, requestFactory);

        if (responseResult.IsFailed)
        {
            return responseResult.ToResult();
        }

        if (!responseResult.Value.Headers.TryGetValues(headerName, out var values))
        {
            return Result.Fail("Requested header not found.");
        }
        var headerValue = values.First();

        return !string.IsNullOrWhiteSpace(headerValue)
            ? Result.Ok(headerValue)
            : Result.Fail("Requested header is empty.");
    }

    protected async Task<Result> ExecuteResumableRequest(Guid storageAccountId, HttpMethod method, string url, Stream content)
    {
        bool continueUpload = true;
        int processedBytes = 0;

        while (continueUpload)
        {
            content.Seek(processedBytes, SeekOrigin.Begin);
            var requestFactory = GetStreamMessage(content);

            var responseResult = await GetSuccessResponseMessage(storageAccountId, requestFactory);
            
            if (responseResult.IsFailed)
            {
                if (responseResult.HasError<ApiErrors.IncompleteUploadError>())
                {
                    if(responseResult.Value.Headers.TryGetValues("Range", out var rangeValues))
                    {
                        var rangeSplit = rangeValues.FirstOrDefault()?.Split('-');
                        var rangeStr = rangeSplit?[^1];
                        if (int.TryParse(rangeStr, out var range))
                            processedBytes = range;
                    }
                    continue;
                }

                if (responseResult.HasError<ApiErrors.NotFoundError>())
                {
                    Result.Fail("There was a problem during upload. Please try again.");
                }

                return responseResult.ToResult();
            }
            continueUpload = false;
        }

        return Result.Ok();

        Func<HttpRequestMessage> GetStreamMessage(Stream stream)
            => new(() => new HttpRequestMessage(HttpMethod.Put, url)
            {
                Content = new StreamContent(stream),
            });
    }

    private async Task<Result<HttpResponseMessage>> GetSuccessResponseMessage(Guid storageAccountId,
        Func<HttpRequestMessage> requestFactory)
    {
        var token = await _accountSecretsRepo.GetAccessTokenAsync(storageAccountId);

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var requestMessage = requestFactory();
        var response = await _httpClient.SendAsync(requestMessage);

        if (response.StatusCode == HttpStatusCode.Unauthorized || response.StatusCode == HttpStatusCode.Forbidden)
        {
            var result = await RetryRequestAsync(storageAccountId, requestFactory);

            if (result.IsFailed)
            {
                return Result.Fail(result.Errors);
            }

            response = result.Value;
        }

        if(response.StatusCode == HttpStatusCode.NotFound)
        {
            return Result.Fail(ApiErrors.NotFound());
        }

        if(response.StatusCode == HttpStatusCode.PermanentRedirect)
        {
            return Result.Fail(ApiErrors.IncompleteUpload());
        }

        return response.IsSuccessStatusCode
            ? response
            : Result.Fail("Error ocurred during communication with external server");
    }

    private async Task<Result<HttpResponseMessage>> RetryRequestAsync(Guid storageAccountId,
        Func<HttpRequestMessage> requestMessage)
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

    protected Task<Result<T>> GetAsync<T>(Guid storageAccountId, string url)
    {
        var messageFactory = new Func<HttpRequestMessage>(() => new HttpRequestMessage(HttpMethod.Get, url));
        return ExecuteRequestAsync<T>(storageAccountId, messageFactory);
    }
}
