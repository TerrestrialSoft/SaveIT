using FluentResults;
using SaveIt.App.Domain.Auth;
using SaveIt.App.Domain.Errors;
using SaveIt.App.Domain.Models;
using SaveIt.App.Infrastructure.Models;
using System.Net;
using System.Net.Http.Json;

namespace SaveIt.App.Infrastructure.Api;

public class SaveItApiService(HttpClient httpClient) : ISaveItApiService
{
    private const string _authUrl = "google";
    private const string _refreshUrl = "refresh";

    private readonly HttpClient _httpClient = httpClient;

    public async Task<Uri> GetAuthorizationUrlAsync(Guid requestId, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsJsonAsync(_authUrl, new { requestId }, cancellationToken);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadFromJsonAsync<AuthorizationUrlModel>(cancellationToken);
        ArgumentNullException.ThrowIfNull(content);

        return new Uri(content.Url);
    }

    public async Task<Result<string>> RefreshAccessTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        var dictionary = new Dictionary<string, string>
        {
            { "RefreshToken", refreshToken }
        };

        var response = await _httpClient.PostAsJsonAsync(_refreshUrl, dictionary, cancellationToken);

        if (response.StatusCode == HttpStatusCode.Unauthorized || response.StatusCode == HttpStatusCode.Forbidden)
        {
            return Result.Fail(ApiErrors.InvalidAuthorization());
        }

        var token = await response.Content.ReadFromJsonAsync<OAuthAccessTokenModel>(cancellationToken);

        ArgumentNullException.ThrowIfNull(token);

        return Result.Ok(token.AccessToken);
    }
}
