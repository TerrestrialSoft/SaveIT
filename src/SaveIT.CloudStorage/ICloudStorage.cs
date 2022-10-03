namespace SaveIT.Core.Storage;
public interface ICloudStorage
{
	Task CreateFileAsync(long accountId);
	Task GetFolders(long accountId);
}
