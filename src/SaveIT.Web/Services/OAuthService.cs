using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using SaveIT.Web.Models;

namespace SaveIT.Api.Services;

public class OAuthService : IOAuthService
{
	private readonly IHttpClientFactory _httpClientFactory;
	private readonly IConfiguration _configuration;
	private readonly IMemoryCache _memoryCache;
	private readonly ILogger<OAuthService> _logger;

	public OAuthService(IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<OAuthService> logger, IMemoryCache memoryCache)
	{
		_httpClientFactory = httpClientFactory;
		_configuration = configuration;
		_logger = logger;
		_memoryCache = memoryCache;
	}

	public async Task StoreTokenAsync(string state, string code)
	{
		var body = new Dictionary<string, string>()
		{
			{ "client_id", _configuration["GoogleDriveOAuth:ClientId"] },
			{ "client_secret", _configuration["GoogleDriveOAuth:ClientSecret"] },
			{ "code", code },
			{ "grant_type", "authorization_code" },
			{ "redirect_uri", "https://localhost:44307/Home/authorized" },
		};

		var client = _httpClientFactory.CreateClient();
		var response = await client.PostAsync("https://oauth2.googleapis.com/token", new FormUrlEncodedContent(body));

		if(!response.IsSuccessStatusCode)
		{
			_logger.LogError("GoogleAPI communication error");
			return;
		}

		var content = await response.Content.ReadAsStringAsync();
		var token = JsonConvert.DeserializeObject<OAuthToken>(content);

		if(token is null)
		{
			_logger.LogError("GoogleAPI Token parsing error");
			return;
		}

		var options = new MemoryCacheEntryOptions()
		{
			AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1)
		};

		options.RegisterPostEvictionCallback(PostEvictionCallback);
		_memoryCache.Set(state, token, options);
	}

	private void PostEvictionCallback(object key, object value, EvictionReason reason, object state)
	{
		if (reason != EvictionReason.TokenExpired)
			return;

		if (value is not OAuthToken token)
			return;

		RevokeToken(token.AcccessToken).Wait();
	}

	public async Task RevokeToken(string token)
	{
		var body = new Dictionary<string, string>()
		{
			{ "token", token },
		};

		var client = _httpClientFactory.CreateClient();
		var response = await client.PostAsync("https://oauth2.googleapis.com/revoke", new FormUrlEncodedContent(body));

		if (!response.IsSuccessStatusCode)
		{
			_logger.LogError("GoogleAPI communication error");
			return;
		}

		_logger.LogInformation("Token was successfully revoked");
	}

	public async Task<OAuthToken?> RefreshTokenAsync(string refreshToken)
	{
		var body = new Dictionary<string, string>()
		{
			{ "client_id", _configuration["GoogleDriveOauth:ClientId"] },
			{ "client_secret", _configuration["GoogleDriveOauth:ClientSecret"] },
			{ "refresh_token", refreshToken },
			{ "grant_type", "refresh_token" },
		};

		var client = _httpClientFactory.CreateClient();
		var response = await client.PostAsync("https://oauth2.googleapis.com/revoke", new FormUrlEncodedContent(body));

		if (!response.IsSuccessStatusCode)
		{
			_logger.LogError("GoogleAPI communication error");
			return null;
		}

		_logger.LogInformation("Token was successfully refreshed");

		var content = await response.Content.ReadAsStringAsync();
		var token = JsonConvert.DeserializeObject<OAuthToken>(content);

		if (token is null)
		{
			_logger.LogError("GoogleAPI Token parsing error");
			return null;
		}

		return token;
	}

	public OAuthToken? GetToken(string code)
	{
		if (!_memoryCache.TryGetValue(code, out OAuthToken token))
			return null;

		_memoryCache.Remove(code);

		return token;
	}
}
