using System.Text.Json.Serialization;

namespace SaveIt.App.Domain.Models;

public record OAuthCompleteTokenModel : OAuthAccessTokenModel
{
    [JsonPropertyName("refresh_token")]
    public string RefreshToken { get; } = null!;

    [JsonConstructor]
    public OAuthCompleteTokenModel(string accessToken, string refreshToken, string scope, string tokenType, int expiresIn)
        : base(accessToken, scope, tokenType, expiresIn)
    {
        RefreshToken = refreshToken;
    }
}