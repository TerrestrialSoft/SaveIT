using SaveIT.Web.Models;

namespace SaveIT.Api.Services;
public interface IOAuthService
{
	OAuthToken? GetToken(string code);
	Task StoreTokenAsync(string state, string code);
	Task<OAuthToken?> RefreshTokenAsync(string refreshToken);
}
