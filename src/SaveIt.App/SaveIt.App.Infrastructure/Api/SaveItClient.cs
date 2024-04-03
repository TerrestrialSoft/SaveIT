using Newtonsoft.Json.Linq;
using SaveIt.App.Domain.Auth;
using SaveIt.App.Domain.Models;
using SaveIt.App.Infrastructure.Models;
using System.Net.Http.Json;

namespace SaveIt.App.Infrastructure.Api;

public class SaveItClient(HttpClient httpClient) : IAuthClientService
{
    private const string _authUrl = "google";
    private const string _tokensUrl = "retrieve";

    private readonly HttpClient _httpClient = httpClient;

    public async Task<Uri> GetAuthorizationUrlAsync(Guid requestId, CancellationToken cancellationToken)
    {
        var response = await _httpClient.PostAsJsonAsync(_authUrl, new { requestId }, cancellationToken);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadFromJsonAsync<AuthorizationUrlModel>(cancellationToken);
        ArgumentNullException.ThrowIfNull(content);

        return new Uri(content.Url);
    }

    public async Task<OAuthTokenModel> GetTokenAsync(Guid requestId, CancellationToken cancellationToken)
    {
        var response = await _httpClient.PostAsJsonAsync(_tokensUrl, new { requestId }, cancellationToken);
        response.EnsureSuccessStatusCode();

        var token = await response.Content.ReadFromJsonAsync<OAuthTokenModel>(cancellationToken);
        ArgumentNullException.ThrowIfNull(token);
        
        return token;
    }
}
