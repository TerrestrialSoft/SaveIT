using SaveIt.App.Domain.Auth;
using SaveIt.App.Domain.Models;
using System.Net.Http.Json;

namespace SaveIt.App.Infrastructure.Api;
public class SaveItApiTokenRetrievalService(HttpClient _httpClient) : ISaveItApiTokenRetrievalService
{
    private const string _tokensUrl = "retrieve";

    public async Task<OAuthCompleteTokenModel> GetTokenAsync(Guid requestId, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsJsonAsync(_tokensUrl, new { requestId }, cancellationToken);
        response.EnsureSuccessStatusCode();

        var token = await response.Content.ReadFromJsonAsync<OAuthCompleteTokenModel>(cancellationToken);
        ArgumentNullException.ThrowIfNull(token);
        return token;
    }
}
