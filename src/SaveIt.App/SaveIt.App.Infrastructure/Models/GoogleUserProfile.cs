using System.Text.Json.Serialization;

namespace SaveIt.App.Infrastructure.Models;
public record GoogleProfile
{
    public required GoogleUserProfile User { get; init; } = default!;
}

public record GoogleUserProfile
{
    [JsonPropertyName("emailAddress")]
    public required string Email { get; init; } = default!;
}