using SaveIT.Core.Dtos;

namespace SaveIT.Core.Services;
public interface ITokenService
{
	public Task<OAuthTokenDto?> GetTokenAsync(long profileId);
	public Task CreateTokenAsync(long profileId, OAuthTokenDto token);
	public Task UpdateTokenAsync(long profileId, OAuthTokenDto token);
	public Task<bool> TokenExists(long profileId);
	public string GetSecret();
}
