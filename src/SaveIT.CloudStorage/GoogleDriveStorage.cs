using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using Microsoft.Extensions.Options;
using SaveIT.CloudStorage;
using SaveIT.CloudStorage.Options;
using System.IO;
using GoogleFile = Google.Apis.Drive.v3.Data.File;

namespace SaveIT.Core.Storage;

public class GoogleDriveStorage : ICloudStorage
{
	private static DriveService _driveService;
	private static readonly string[] Scopes = { DriveService.Scope.Drive };
	private const string ApplicationName = "SaveIT";

	private readonly GoogleDriveOAuthOptions _googleDriveOptions;

	public GoogleDriveStorage(IOptions<GoogleDriveOAuthOptions> googleDriveOptions)
	{
		_googleDriveOptions = googleDriveOptions.Value;
	}

	public async Task CreateFileAsync(long accountId)
	{
		var file = new GoogleFile { Name = "SaveIT", MimeType = "application/vnd.google-apps.folder", Parents =  new List<string> { "1i1yCYL8nNSRL6G8zTtcTZPm_rjFougkc" } };

		using var stream = new MemoryStream();
		using var writer = new StreamWriter(stream);
		//writer.Write("My first file");
		writer.Flush();
		stream.Position = 0;

		var request = _driveService.Files.Create(file, stream, "application/vnd.google-apps.folder");
		request.Fields = "id";
		await request.UploadAsync();

		var response = request.ResponseBody;
	}

	public async Task GetFolders(long accountId)
	{
		var request = _driveService.Files.Get("root");
		//request.Q = "mimeType='application/vnd.google-apps.folder' and trashed=false";
		var files = await request.ExecuteAsync();
	}

	public Task DownloadFile() => throw new NotImplementedException();
}
