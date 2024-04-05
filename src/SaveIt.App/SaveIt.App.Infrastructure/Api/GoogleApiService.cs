using SaveIt.App.Domain.Auth;
using SaveIt.App.Infrastructure.Models;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace SaveIt.App.Infrastructure.Api;
public class GoogleApiService(HttpClient httpClient) : IExternalStorageService
{
    private const string _profileUrl = "about?fields=user";

    public async Task<string> GetProfileEmailAsync(string accessToken)
    {
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var response = await httpClient.GetAsync(_profileUrl);
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadFromJsonAsync<GoogleProfile>();
        ArgumentNullException.ThrowIfNull(content);
        
        return content.User.Email;
    }
}
