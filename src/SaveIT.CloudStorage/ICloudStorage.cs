using SaveIT.CloudStorage.Models;

namespace SaveIT.Core.Storage;
public interface ICloudStorage
{
	Task<GoogleFileModel> CreateFileAsync(long profileId);
	Task<GoogleFileModel> CreateFolderAsync(long profileId, string name);
	Task<GoogleFileModel?> GetFolderAsync(long profileId, string name);
}
