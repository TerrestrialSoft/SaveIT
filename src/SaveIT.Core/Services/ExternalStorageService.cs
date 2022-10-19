using SaveIT.Core.Storage;

namespace SaveIT.Core.Services;
public class ExternalStorageService : IExternalStorageService
{
	private readonly ICloudStorage _cloudStorage;


	public ExternalStorageService(ICloudStorage cloudStorage)
	{
		_cloudStorage = cloudStorage;
	}

	public async Task<string> InitializeNewRepositoryAsync(long profileId, string name)
	{
		var folder = await _cloudStorage.GetFolderAsync(profileId, name);

		if (folder is not null)
			throw new Exception("Name is already taken");

		// More repository initialization steps

		return (await _cloudStorage.CreateFolderAsync(profileId, name)).Id;
	}

	public async Task CreateFileAsync(long profileId)
		=> await _cloudStorage.CreateFileAsync(profileId);

	public async Task<string> GetExistingRepositoryAsync(long profileId, string name)
	{
		var folder = await _cloudStorage.GetFolderAsync(profileId, name);

		if (folder is null)
			throw new Exception("Repository does not exist");
		
		return folder.Id;
	}
}
