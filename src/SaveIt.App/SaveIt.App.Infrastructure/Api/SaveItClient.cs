using SaveIt.App.Domain.Auth;
using System.Net.Http.Json;

namespace SaveIt.App.Infrastructure.Api;

public class SaveItClient(HttpClient httpClient) : IAuthClientService
{
    private const string _authUrl = "google";
    private const string _authGetTokens = "auth/retrieve";

    private readonly HttpClient _httpClient = httpClient;

    public async Task<Uri> GetAuthorizationUrlAsync(Guid requestId)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync(_authUrl, new { requestId });

            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            return new Uri(content);

        }
        catch (Exception)
        {
            // problem
            return null;
        }
    }

}
