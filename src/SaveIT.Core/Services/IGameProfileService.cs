using SaveIT.Core.Entities;

namespace SaveIT.Core.Services;

public interface IGameProfileService
{
	Task<IEnumerable<GameProfile>> GetGameProfilesAsync();
	Task<GameProfile?> GetGameProfileAsync(long id);
}
