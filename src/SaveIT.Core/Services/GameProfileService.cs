using SaveIT.Core.Entities;
using SaveIT.Core.Repositories;

namespace SaveIT.Core.Services;

public class GameProfileService : IGameProfileService
{
	private readonly IRepository<GameProfile> _repository;

	public GameProfileService(IRepository<GameProfile> repository)
	{
		_repository = repository;
	}

	public async Task CreateGameProfileAsync(string profileName, string nickname)
	{
		await _repository.CreateAsync(new GameProfile
		{
			ProfileName = profileName,
			Nickname = nickname,
			DateCreated = DateTime.Now,
		});
	}

	public async Task<GameProfile?> GetGameProfileAsync(long id)
	{
		return await _repository.GetAsync(x => x.Id == id);
	}

	public async Task<IEnumerable<GameProfile>> GetGameProfilesAsync()
	{
		var x =  await _repository.GetAllAsync();
		return x;
	}
}
