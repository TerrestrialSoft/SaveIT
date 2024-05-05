using System.Net.Http.Json;
using System.Net;

namespace SaveIt.Server.Tests.Helpers;
internal class MockHttpMessageHandler(HttpStatusCode statusCode, object? responseContent = null) : HttpMessageHandler
{
    private readonly HttpStatusCode _statusCode = statusCode;
    private readonly object? _responseContent = responseContent;

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        => await Task.FromResult(new HttpResponseMessage
        {
            StatusCode = _statusCode,
            Content = JsonContent.Create(_responseContent)
        });
}
