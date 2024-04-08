using SaveIt.App.Domain.Auth;
using SaveIt.App.Domain.Models;
using SaveIt.App.Infrastructure.Models;
using System.Net.Http.Json;

namespace SaveIt.App.Infrastructure.Api;

public class SaveItApiService(HttpClient httpClient) : ISaveItApiService
{
    private const string _authUrl = "google";
    private const string _tokensUrl = "retrieve";
    private const string _refreshUrl = "refresh";

    private readonly HttpClient _httpClient = httpClient;

    public async Task<Uri> GetAuthorizationUrlAsync(Guid requestId, CancellationToken cancellationToken)
    {
        var response = await _httpClient.PostAsJsonAsync(_authUrl, new { requestId }, cancellationToken);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadFromJsonAsync<AuthorizationUrlModel>(cancellationToken);
        ArgumentNullException.ThrowIfNull(content);

        return new Uri(content.Url);
    }

    public async Task<OAuthCompleteTokenModel> GetTokenAsync(Guid requestId, CancellationToken cancellationToken)
    {
        var response = await _httpClient.PostAsJsonAsync(_tokensUrl, new { requestId }, cancellationToken);
        response.EnsureSuccessStatusCode();

        var token = await response.Content.ReadFromJsonAsync<OAuthCompleteTokenModel>(cancellationToken);
        ArgumentNullException.ThrowIfNull(token);
        
        return token;
    }

    public async Task<string> RefreshAccessTokenAsync(string refreshToken)
    {
        var dictionary = new Dictionary<string, string>
        {
            { "RefreshToken", refreshToken }
        };

        var response = await _httpClient.PostAsJsonAsync(_refreshUrl, dictionary);
        response.EnsureSuccessStatusCode();

        var token = await response.Content.ReadFromJsonAsync<OAuthAccessTokenModel>();

        ArgumentNullException.ThrowIfNull(token);

        return token.AccessToken;
    }
}
