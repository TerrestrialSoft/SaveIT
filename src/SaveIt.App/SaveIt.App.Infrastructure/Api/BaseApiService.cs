using FluentResults;
using System.Net.Http.Headers;
using System.Net;
using System.Net.Http.Json;
using SaveIt.App.Domain.Repositories;
using SaveIt.App.Domain.Errors;
using SaveIt.App.Domain.Auth;

namespace SaveIt.App.Infrastructure.Api;
public class BaseApiService(HttpClient httpClient, IAccountSecretsRepository accountSecretsRepo, ISaveItApiService saveItService,
    IStorageAccountRepository storageAccountsRepository)
{
    protected readonly HttpClient _httpClient = httpClient;
    protected readonly IAccountSecretsRepository _accountSecretsRepo = accountSecretsRepo;
    protected readonly ISaveItApiService _saveItService = saveItService;
    protected readonly IStorageAccountRepository _storageAccountsRepository = storageAccountsRepository;

    protected async Task<Result> ExecuteRequestAsync(Guid storageAccountId, Func<HttpRequestMessage> requestFactory,
        CancellationToken cancellationToken = default)
    {
        var responseResult = await GetSuccessResponseMessage(storageAccountId, requestFactory, cancellationToken);
        return responseResult.ToResult();
    }

    protected async Task<Result<T>> ExecuteRequestAsync<T>(Guid storageAccountId, Func<HttpRequestMessage> requestFactory,
        CancellationToken cancellationToken = default)
    {
        var responseResult = await GetSuccessResponseMessage(storageAccountId, requestFactory, cancellationToken);

        if (responseResult.IsFailed)
        {
            return responseResult.ToResult();
        }

        var content = await responseResult.Value.Content.ReadFromJsonAsync<T>(cancellationToken);

        return content is not null
            ? content
            : Result.Fail("Error ocurred during communication with external server");
    }

    protected async Task<Result<string>> DownloadContentAsStringAsync(Guid storageAccountId,
        Func<HttpRequestMessage> requestFactory, CancellationToken cancellationToken = default)
    {
        var responseResult = await GetSuccessResponseMessage(storageAccountId, requestFactory, cancellationToken);

        if (responseResult.IsFailed)
        {
            return responseResult.ToResult();
        }

        var content = await responseResult.Value.Content.ReadAsStringAsync(cancellationToken);

        return content is not null
            ? content
            : Result.Fail("Error ocurred during communication with external server");
    }

    protected async Task<Result<Stream>> DownloadContentAsStreamAsync(Guid storageAccountId,
        Func<HttpRequestMessage> requestFactory, CancellationToken cancellationToken = default)
    {
        var responseResult = await GetSuccessResponseMessage(storageAccountId, requestFactory, cancellationToken);

        if (responseResult.IsFailed)
        {
            return responseResult.ToResult();
        }

        var content = await responseResult.Value.Content.ReadAsStreamAsync(cancellationToken);

        return content is not null
            ? content
            : Result.Fail("Error ocurred during communication with external server");
    }

    protected async Task<Result<string>> ExecuteRequestForHeaderAsync(Guid storageAccountId, string headerName,
        Func<HttpRequestMessage> requestFactory, CancellationToken cancellationToken = default)
    {
        var responseResult = await GetSuccessResponseMessage(storageAccountId, requestFactory, cancellationToken);

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

    protected async Task<Result> ExecuteResumableRequest(Guid storageAccountId, HttpMethod method, string url, Stream content,
        CancellationToken cancellationToken = default)
    {
        bool continueUpload = true;
        int processedBytes = 0;

        while (continueUpload)
        {
            content.Seek(processedBytes, SeekOrigin.Begin);
            var requestFactory = GetStreamHttpMessage(content);

            var responseResult = await GetSuccessResponseMessage(storageAccountId, requestFactory, cancellationToken);
            
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

        Func<HttpRequestMessage> GetStreamHttpMessage(Stream stream)
            => new(() => new HttpRequestMessage(method, url)
            {
                Content = new StreamContent(stream),
            });
    }

    private async Task<Result<HttpResponseMessage>> GetSuccessResponseMessage(Guid storageAccountId,
        Func<HttpRequestMessage> requestFactory, CancellationToken cancellationToken = default)
    {
        var token = await _accountSecretsRepo.GetAccessTokenAsync(storageAccountId);

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var requestMessage = requestFactory();
        var response = await _httpClient.SendAsync(requestMessage, cancellationToken);

        if (response.StatusCode == HttpStatusCode.Unauthorized || response.StatusCode == HttpStatusCode.Forbidden)
        {
            var result = await RetryRequestAsync(storageAccountId, requestFactory, cancellationToken);

            if (result.IsFailed)
            {
                return Result.Fail(result.Errors);
            }

            response = result.Value;
        }

        if (response.IsSuccessStatusCode)
        {
            return response;
        }

        return response.StatusCode switch
        {
            HttpStatusCode.NotFound => Result.Fail(ApiErrors.NotFound()),
            HttpStatusCode.PermanentRedirect => Result.Fail(ApiErrors.IncompleteUpload()),
            _ => Result.Fail("Error ocurred during communication with external server")
        };
    }

    private async Task<Result<HttpResponseMessage>> RetryRequestAsync(Guid storageAccountId,
        Func<HttpRequestMessage> requestMessage, CancellationToken cancellationToken = default)
    {
        var refreshResult = await TryRefreshTokenAsync(storageAccountId, cancellationToken);

        if (refreshResult.IsFailed)
        {
            return refreshResult.ToResult();
        }

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", refreshResult.Value);

        var message = requestMessage();
        var result = await _httpClient.SendAsync(message, cancellationToken);

        return result.IsSuccessStatusCode
            ? Result.Ok(result)
            : Result.Fail("Error ocurred during communication with external server");
    }

    private async Task<Result<string>> TryRefreshTokenAsync(Guid storageAccountId, CancellationToken cancellationToken = default)
    {
        var refreshToken = await _accountSecretsRepo.GetRefreshTokenAsync(storageAccountId);

        if (refreshToken is null)
        {
            await _storageAccountsRepository.UnauthorizeAccountAsync(storageAccountId);
            return Result.Fail("Refresh token not found");
        }

        string accessToken;
        try
        {
            var accessTokenResult = await _saveItService.RefreshAccessTokenAsync(refreshToken, cancellationToken);

            if (accessTokenResult.IsSuccess)
            {
                accessToken = accessTokenResult.Value;
            }
            else
            {
                if (accessTokenResult.HasError<ApiErrors.AuthorizationError>())
                {
                    await _storageAccountsRepository.UnauthorizeAccountAsync(storageAccountId);
                }

                return Result.Fail(new AuthError("Unable to refresh access token"));
            }
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

    protected Task<Result<T>> GetAsync<T>(Guid storageAccountId, string url, CancellationToken cancellationToken = default)
    {
        var messageFactory = new Func<HttpRequestMessage>(() => new HttpRequestMessage(HttpMethod.Get, url));
        return ExecuteRequestAsync<T>(storageAccountId, messageFactory, cancellationToken);
    }
}
