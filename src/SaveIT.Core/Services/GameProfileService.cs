using SaveIT.Core.Entities;
using SaveIT.Core.Repositories;
using SaveIT.Core.Storage;

namespace SaveIT.Core.Services;

public class GameProfileService : IGameProfileService
{
	private readonly IRepository<GameProfile> _repository;
	private readonly ICloudStorage _cloudStorage;

	public GameProfileService(IRepository<GameProfile> repository, ICloudStorage cloudStorage)
	{
		_repository = repository;
		_cloudStorage = cloudStorage;
	}

	public async Task CreateGameProfileAsync(string profileName, string nickname)
		=> await _repository.CreateAsync(new GameProfile
		{
			ProfileName = profileName,
			Nickname = nickname,
			DateCreated = DateTime.Now,
		});

	public async Task UpdateGameProfileAsync(GameProfile profile)
		=> await _repository.UpdateAsync(x => x.Id == profile.Id, profile);

	public async Task DeleteGameProfileAsync(long id)
		=> await _repository.DeleteAsync(x => x.Id == id);

	public async Task<GameProfile?> GetGameProfileAsync(long id)
		=> await _repository.GetAsync(x => x.Id == id);

	public async Task<IEnumerable<GameProfile>> GetGameProfilesAsync()
		=> await _repository.GetAllAsync();

	public async Task AuthorizeAccount(long id)
	{
		var profile = await GetGameProfileAsync(id);

		if (profile is null)
			return;

		

		profile.IsAuthorized = true;
		await UpdateGameProfileAsync(profile);
	}
}
