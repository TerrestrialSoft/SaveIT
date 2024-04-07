using System.Text.Json.Serialization;

namespace SaveIt.Server.UI.Models;

public record OAuthCompleteTokenResponseModel : OAuthAccessTokenResponseModel
{
    [JsonPropertyName("refresh_token")]
    public string RefreshToken { get; } = null!;

    [JsonConstructor]
    public OAuthCompleteTokenResponseModel(string accessToken, string refreshToken, string scope, string tokenType, int expiresIn)
        : base(accessToken, scope, tokenType, expiresIn)
    {
        RefreshToken = refreshToken;
    }
}
