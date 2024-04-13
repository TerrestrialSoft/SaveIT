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
    private const string _fileUploadMultipartUrl = $"{_baseFilesUrl}?uploadType=multipart";

    public async Task<Result> CreateFileAsync(Guid storageAccountId, string fileName, string? parentId = null)
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

        var mediaJson = "{\"Prop\":\"a\"}";
        var mediaContent = new StringContent(mediaJson, Encoding.UTF8,
            new MediaTypeHeaderValue("application/json"));

        var content = new MultipartFormDataContent
        {
            { metadataContent, "Metadata" },
            { mediaContent, "Media" }
        };

        var messageFactory = new Func<HttpRequestMessage>(() => new HttpRequestMessage(HttpMethod.Post, _fileUploadMultipartUrl)
        {
            Content = content,
        });

        var result = await ExecuteRequestAsync<GoogleFileModel>(storageAccountId, messageFactory);

        return result.ToResult();
    }
}
