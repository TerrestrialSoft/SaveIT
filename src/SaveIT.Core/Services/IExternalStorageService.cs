namespace SaveIT.Core.Services;
public interface IExternalStorageService
{
	Task CreateFileAsync(long id);
	Task<string> InitializeNewRepositoryAsync(long profileId, string name);
	Task<string> GetExistingRepositoryAsync(long profileId, string name);
}
