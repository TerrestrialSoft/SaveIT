using System.Text.Json.Serialization;

namespace SaveIt.App.Domain.Models;

public record OAuthTokenModel
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; } = null!;


    [JsonPropertyName("refresh_token")]
    public string RefreshToken { get; } = null!;


    [JsonPropertyName("scope")]
    public string Scope { get; } = null!;


    [JsonPropertyName("token_type")]
    public string TokenType { get; } = null!;


    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; }

    [JsonIgnore]
    public DateTime ExpiresAt { get; }


    [JsonConstructor]
    public OAuthTokenModel(string accessToken, string refreshToken, string scope, string tokenType, int expiresIn) =>
            (AccessToken, RefreshToken, Scope, TokenType, ExpiresIn, ExpiresAt)
        = (accessToken, refreshToken, scope, tokenType, expiresIn, DateTime.UtcNow.AddSeconds(expiresIn));
}
