using FluentResults;
using SaveIt.App.Domain.Auth;
using SaveIt.App.Domain.Repositories;
using SaveIt.App.Infrastructure.Models;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace SaveIt.App.Infrastructure.Api;
public class GoogleApiUploadService(HttpClient _httpClient, IAccountSecretsService _accountsSecretsRepo,
    ISaveItApiService _saveItService) 
    : BaseApiService(_httpClient, _accountsSecretsRepo, _saveItService)
{
    private const string _mimeTypeFile = "application/vnd.google-apps.file";
    private const string _baseFilesUrl = "files";
    private const string _fileDetailUrl = "files/{0}";
    private const string _uploadTypeMultipart = $"uploadType=multipart";
    private const string _uploadTypeMedia = $"uploadType=media";

    public async Task<Result> CreateFileSimpleAsync(Guid storageAccountId, string fileName, object fileContent,
        string? parentId = null)
    {
        var fileMetadata = new GoogleFileCreateModel
        {
            Name = fileName,
            MimeType = _mimeTypeFile,
            Parents = parentId is not null ? new[] { parentId } : null
        };

        var metadataJson = JsonSerializer.Serialize(fileMetadata);
        var metadataContent = new StringContent(metadataJson, Encoding.UTF8,
            new MediaTypeHeaderValue("application/json"));

        var mediaJson = JsonSerializer.Serialize(fileContent);
        var mediaContent = new StringContent(mediaJson, Encoding.UTF8,
            new MediaTypeHeaderValue("application/json"));

        var content = new MultipartFormDataContent
        {
            { metadataContent, "Metadata" },
            { mediaContent, "Media" }
        };

        var messageFactory = new Func<HttpRequestMessage>(() => new HttpRequestMessage(HttpMethod.Post,
            $"{_baseFilesUrl}?{_uploadTypeMultipart}")
        {
            Content = content,
        });

        var result = await ExecuteRequestAsync<GoogleFileModel>(storageAccountId, messageFactory);

        return result.ToResult();
    }

    internal async Task<Result> UpdateFileSimpleAsync(Guid storageAccountId, string id, object fileContent)
    {
        var url = string.Format($"{_fileDetailUrl}?{_uploadTypeMedia}", id);

        var fileJson = JsonSerializer.Serialize(fileContent);
        var content = new StringContent(fileJson, Encoding.UTF8,
            new MediaTypeHeaderValue("application/json"));

        var messageFactory = new Func<HttpRequestMessage>(() => new HttpRequestMessage(HttpMethod.Patch, url)
        {
            Content = content,
        });

        var result = await ExecuteRequestAsync<GoogleFileModel>(storageAccountId, messageFactory);

        return result.ToResult();
    }
}
