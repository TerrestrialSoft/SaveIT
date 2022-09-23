using SaveIT.Api.Models;

namespace SaveIT.Api.Services;

public class OAuthService : IOAuthService
{
	private readonly IHttpClientFactory _httpClientFactory;

	public OAuthService(IHttpClientFactory httpClientFactory)
	{
		_httpClientFactory = httpClientFactory;
	}

	public async Task<OAuthToken> GetTokenAsync()
	{
		var httpClient = _httpClientFactory.CreateClient();

		return new OAuthToken();
	}
}
