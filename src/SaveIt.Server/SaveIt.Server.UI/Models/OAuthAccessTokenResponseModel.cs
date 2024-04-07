using System.Text.Json.Serialization;

namespace SaveIt.Server.UI.Models;

public record OAuthAccessTokenResponseModel
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
    public OAuthAccessTokenResponseModel(string accessToken, string scope, string tokenType, int expiresIn) =>
            (AccessToken, Scope, TokenType, ExpiresIn) = (accessToken, scope, tokenType, expiresIn);
}
