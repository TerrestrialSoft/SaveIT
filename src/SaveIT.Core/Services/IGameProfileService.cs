using SaveIT.Core.Entities;

namespace SaveIT.Core.Services
{
	public interface IGameProfileService
	{
		IEnumerable<GameProfile> GetGameProfiles();
		GameProfile? GetGameProfile(Guid id);
	}
}