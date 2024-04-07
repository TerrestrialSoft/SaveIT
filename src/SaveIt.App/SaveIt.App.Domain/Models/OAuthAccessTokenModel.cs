using System.Text.Json.Serialization;

namespace SaveIt.App.Domain.Models;

public record OAuthAccessTokenModel
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; } = null!;

    [JsonPropertyName("scope")]
    public string Scope { get; } = null!;


    [JsonPropertyName("token_type")]
    public string TokenType { get; } = null!;


    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; }

    [JsonConstructor]
    public OAuthAccessTokenModel(string accessToken, string scope, string tokenType, int expiresIn) =>
            (AccessToken, Scope, TokenType, ExpiresIn) = (accessToken, scope, tokenType, expiresIn);
}
