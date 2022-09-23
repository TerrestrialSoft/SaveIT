using SaveIT.Core.Storage;

namespace SaveIT.Core.Services;
public class ExternalStorageService : IExternalStorageService
{
	private readonly ICloudStorage _cloudStorage;

	public ExternalStorageService(ICloudStorage cloudStorage)
	{
		_cloudStorage = cloudStorage;
	}

	public async Task AuthorizeAccountAsync(long profileId)
		=> await _cloudStorage.AuthorizeAccount(profileId);

	public async Task InitializeNewRepositoryAsync(long profileId, string name)
	{

	}

	public async Task CreateFileAsync(long id)
	=> await _cloudStorage.CreateFileAsync(id);

}
