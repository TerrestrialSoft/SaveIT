using System.Text.Json.Serialization;

namespace SaveIt.App.Infrastructure.Models;
public class GooglePermissionCreateModel
{
    [JsonPropertyName("role")]
    public string Role { get; set; } = default!;

    [JsonPropertyName("type")]
    public string Type { get; set; } = default!;

    [JsonPropertyName("emailAddress")]
    public string EmailAddress { get; set; } = default!;
}
