using SaveIT.Core.Dtos;
using SaveIT.Core.Entities;
using SaveIT.Core.Repositories;

namespace SaveIT.Core.Services;
public class TokenService : ITokenService
{
	private readonly IRepository<OAuthToken> _repository;

	private const int SECRET_SIZE = 64;

	public TokenService(IRepository<OAuthToken> repository)
	{
		_repository = repository;
	}

	public async Task<OAuthTokenDto?> GetTokenAsync(long profileId)
	{
		var token =  await _repository.GetAsync(x => x.GameProfileId == profileId);

		if (token is null)
			return null;

		return new OAuthTokenDto
		{
			AcccessToken = token.AcccessToken,
			RefreshToken = token.RefreshToken,
			ExpiresIn = (int)token.ExpiresAt.Subtract(DateTime.UtcNow).TotalSeconds,
		};
	}

	public async Task CreateTokenAsync(long profileId, OAuthTokenDto token)
	{
		var existingToken = await _repository.GetAsync(x => x.GameProfileId == profileId);

		if (existingToken is { })
			throw new Exception("Token already exists");

		var entity = new OAuthToken
		{
			AcccessToken = token.AcccessToken,
			RefreshToken = token.RefreshToken,
			ExpiresAt = DateTime.UtcNow.AddSeconds(token.ExpiresIn),
			GameProfileId = profileId
		};

		await _repository.CreateAsync(entity);
	}

	public async Task UpdateTokenAsync(long profileId, OAuthTokenDto token)
	{
		var existingToken = await _repository.GetAsync(x => x.GameProfileId == profileId);

		if(existingToken is null)
			throw new Exception("Token not found");

		existingToken.AcccessToken = token.AcccessToken;
		existingToken.RefreshToken = token.RefreshToken;
		existingToken.ExpiresAt = DateTime.UtcNow.AddSeconds(token.ExpiresIn);

		await _repository.UpdateAsync(x => x.GameProfileId == profileId, existingToken);
	}

	public async Task<bool> TokenExists(long profileId)
		=> await _repository.GetAsync(x => x.GameProfileId == profileId) is not null;

	public string GetSecret()
		=> RandomGenerator.GetRandomlyGenerateBase64String(SECRET_SIZE);
}
