namespace SaveIT.Core.Storage;
public interface ICloudStorage
{
	Task AuthorizeAccount(long accountId);
	Task CreateFileAsync(long accountId);
	Task GetFolders(long accountId);
}
