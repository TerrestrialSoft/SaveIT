using SaveIt.App.Domain.Auth;

namespace SaveIt.App.Infrastructure.Api;

public class SaveItClient(HttpClient httpClient) : IAuthClientService
{
    private const string _authUrl = "authUrl";

    private readonly HttpClient _httpClient = httpClient;

    public async Task<Uri> GetAuthorizationUrlAsync()
    {
        try
        {
            var message = await _httpClient.GetAsync(_authUrl);

            if (!message.IsSuccessStatusCode)
            {
                // problem
                return null;
            }

            var response = await message.Content.ReadAsStringAsync();
            return new Uri(response);

        }
        catch (Exception)
        {
            // problem
            return null;
        }
    }

}
