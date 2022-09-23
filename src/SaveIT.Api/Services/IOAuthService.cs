using SaveIT.Api.Models;

namespace SaveIT.Api.Services;
public interface IOAuthService
{
	Task<OAuthToken> GetTokenAsync();
}