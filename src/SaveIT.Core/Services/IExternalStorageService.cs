namespace SaveIT.Core.Services;
public interface IExternalStorageService
{
	Task AuthorizeAccountAsync(long profileId);
	Task CreateFileAsync(long id);
	Task InitializeNewRepositoryAsync(long profileId, string name);
}
