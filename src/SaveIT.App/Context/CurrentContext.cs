using SaveIT.Core.Entities;
using SaveIT.Core.Services;

namespace SaveIT.App.Context;

public class CurrentContext
{
	private readonly IGameProfileService _gameProfileService;

	public CurrentContext(IGameProfileService gameProfileService)
	{
		_gameProfileService = gameProfileService;
		_ = RefreshProfilesAsync();
	}

	public List<GameProfile> GameProfiles = new();

	public event Action OnChange;

	public async Task RefreshProfilesAsync()
	{
		GameProfiles = (await _gameProfileService.GetGameProfilesAsync()).ToList();
		NotifyStateChanged();
	}

	private void NotifyStateChanged()
		=> OnChange?.Invoke();
}
