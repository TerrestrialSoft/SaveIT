using System.Text.Json.Serialization;

namespace SaveIt.App.Infrastructure.Models;
public record GoogleFileCreateModel
{
    [JsonPropertyName("name")]
    public required string Name { get; init; }

    public required string MimeType { get; init; }

    public required IEnumerable<string>? Parents { get; init; }
}
