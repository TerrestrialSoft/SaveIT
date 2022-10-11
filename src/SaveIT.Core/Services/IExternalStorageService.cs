namespace SaveIT.Core.Services;
public interface IExternalStorageService
{
	Task CreateFileAsync(long id);
	Task InitializeNewRepositoryAsync(long profileId, string name);
}
