using SaveIT.Core.Entities;
using SaveIT.Core.Repositories;

namespace SaveIT.Core.Services
{
	public class GameProfileService : IGameProfileService
	{
		private readonly IRepository<GameProfile> _repository;

		public GameProfileService(IRepository<GameProfile> repository)
		{
			_repository = repository;
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
}
