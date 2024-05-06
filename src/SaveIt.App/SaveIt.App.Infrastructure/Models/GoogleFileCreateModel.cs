using System.Text.Json.Serialization;

namespace SaveIt.App.Infrastructure.Models;
public record GoogleFileCreateModel
{
    [JsonPropertyName("name")]
    public required string Name { get; init; }

    [JsonPropertyName("mimeType")]
    public required string MimeType { get; init; }

    [JsonPropertyName("parents")]
    public required IEnumerable<string>? Parents { get; init; }
}
