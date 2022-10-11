using Microsoft.Extensions.Options;
using Microsoft.VisualBasic.FileIO;
using Newtonsoft.Json;
using SaveIT.CloudStorage.Models;
using SaveIT.CloudStorage.Options;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace SaveIT.Core.Storage;

public class GoogleDriveStorage : ICloudStorage
{
	private readonly GoogleDriveOAuthOptions _googleDriveOptions;
	private readonly IHttpClientFactory _httpClientFactory;

	private const string GoogleApiUrl = "https://www.googleapis.com/";
	private const string UploadFileUrl = "upload/drive/v3/files?uploadType=multipart";

	public GoogleDriveStorage(IOptions<GoogleDriveOAuthOptions> googleDriveOptions, IHttpClientFactory httpClientFactory)
	{
		_googleDriveOptions = googleDriveOptions.Value;
		_httpClientFactory = httpClientFactory;
	}

	public async Task CreateFileAsync(long accountId)
	{
		var token = "[access_token]";
		var filePath = "[file_path]";
		var fileName = "file.txt";

		var client = _httpClientFactory.CreateClient();
		client.BaseAddress = new Uri(GoogleApiUrl);
		client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

		using var fileStream = File.OpenRead(filePath);
		var fileContent = new StreamContent(fileStream);
		fileContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
		{
			Name = "\"file\""
		};

		var metadata = new GoogleMetadata(fileName, "text/plain", new List<string> { "root" });
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

		var result = await client.PostAsync(UploadFileUrl, multiPartFormDataContent);

		if(!result.IsSuccessStatusCode)
		{
			// autoretry
			// refresh token
		}
	}

	public async Task GetFolders(long accountId)
	{

	}
}
