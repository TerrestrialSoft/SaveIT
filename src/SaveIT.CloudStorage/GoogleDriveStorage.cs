using Newtonsoft.Json;
using SaveIT.CloudStorage.Models;
using System.Net.Http.Headers;
using System.Text;

namespace SaveIT.Core.Storage;

public class GoogleDriveStorage : ICloudStorage
{
	private readonly IHttpClientFactory _httpClientFactory;

	private const string GoogleApiUrl = "https://www.googleapis.com/";
	private const string UploadFileUrl = "upload/drive/v3/files?uploadType=multipart";
	private const string GetFileUrl = "drive/v3/files?q={0}";

	private const string AccessToken = "[access_token]";

	public GoogleDriveStorage(IHttpClientFactory httpClientFactory)
	{
		_httpClientFactory = httpClientFactory;
	}

	public async Task<GoogleFileModel> CreateFileAsync(long profileId)
	{
		var filePath = "[file_path]";
		var fileName = "file.txt";

		var client = _httpClientFactory.CreateClient();
		client.BaseAddress = new Uri(GoogleApiUrl);
		client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AccessToken);

		using var fileStream = File.OpenRead(filePath);
		var fileContent = new StreamContent(fileStream);
		fileContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
		{
			Name = "\"file\""
		};

		var metadata = new GoogleMetadataModel(fileName, "text/plain", new List<string> { "root" });
		var jsonMetadata = JsonConvert.SerializeObject(metadata);
		var metadataContent = new StringContent(jsonMetadata, Encoding.UTF8, "application/json");
		metadataContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
		{
			Name = "\"metadata\""
		};

		var boundary = DateTime.Now.Ticks.ToString();
		var multiPartFormDataContent = new MultipartFormDataContent(boundary);
		multiPartFormDataContent.Headers.Remove("Content-Type");
		multiPartFormDataContent.Headers.TryAddWithoutValidation("Content-Type", "multipart/related; boundary=" + boundary);
		
		multiPartFormDataContent.Add(metadataContent);
		multiPartFormDataContent.Add(fileContent);

		var response = await client.PostAsync(UploadFileUrl, multiPartFormDataContent);

		if(!response.IsSuccessStatusCode)
		{
			// autoretry
			// refresh token
		}

		var content = await response.Content.ReadAsStringAsync();
		return JsonConvert.DeserializeObject<GoogleFileModel>(content)!;
	}

	public async Task<GoogleFileModel?> GetFolderAsync(long profileId, string name)
	{
		var client = _httpClientFactory.CreateClient();
		client.BaseAddress = new Uri(GoogleApiUrl);
		client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AccessToken);

		var response = await client.GetAsync(string.Format(GetFileUrl, $"trashed=false and mimeType = 'application/vnd.google-apps.folder' and name = '{name}' and 'root' in parents"));

		if(!response.IsSuccessStatusCode)
		{
			// autoretry
			// refresh token
		}

		var content = await response.Content.ReadAsStringAsync();
		var folders = JsonConvert.DeserializeObject<GoogleFilesListModel>(content);

		return folders?.Files.FirstOrDefault();
	}

	public async Task<GoogleFileModel> CreateFolderAsync(long profileId, string name)
	{
		var client = _httpClientFactory.CreateClient();
		client.BaseAddress = new Uri(GoogleApiUrl);
		client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AccessToken);

		var metadata = new GoogleMetadataModel(name, "application/vnd.google-apps.folder", new List<string> { "root" });
		var jsonMetadata = JsonConvert.SerializeObject(metadata);
		var metadataContent = new StringContent(jsonMetadata, Encoding.UTF8, "application/json");
		metadataContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
		{
			Name = "\"metadata\""
		};

		var boundary = DateTime.Now.Ticks.ToString();
		var multiPartFormDataContent = new MultipartFormDataContent(boundary);
		multiPartFormDataContent.Headers.Remove("Content-Type");
		multiPartFormDataContent.Headers.TryAddWithoutValidation("Content-Type", "multipart/related; boundary=" + boundary);

		multiPartFormDataContent.Add(metadataContent);

		var response = await client.PostAsync(UploadFileUrl, multiPartFormDataContent);

		if (!response.IsSuccessStatusCode)
		{
			// autoretry
			// refresh token
		}

		var content = await response.Content.ReadAsStringAsync();
		return JsonConvert.DeserializeObject<GoogleFileModel>(content)!;
	}
}
