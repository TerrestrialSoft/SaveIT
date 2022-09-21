using SaveIT.Core.Entities;

namespace SaveIT.Core.Services;

public interface IGameProfileService
{
	Task<IEnumerable<GameProfile>> GetGameProfilesAsync();
	Task<GameProfile?> GetGameProfileAsync(long id);
	Task CreateGameProfileAsync(string profileName, string nickname);
	Task DeleteGameProfileAsync(long id);
	Task CreateFileAsync(long id);
	Task GetFolders(long id);
	Task AuthorizeAccount(long id);
	Task UpdateGameProfileAsync(GameProfile profile);
}
