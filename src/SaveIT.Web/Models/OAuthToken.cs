using Newtonsoft.Json;

namespace SaveIT.Web.Models;

public class OAuthToken
{
	[JsonProperty(PropertyName = "access_token")]
	public string AcccessToken { get; set; } = null!;

	[JsonProperty(PropertyName = "expires_in")]
	public int ExpiresIn { get;	set; }

	[JsonProperty(PropertyName = "refresh_token")]
	public string RefreshToken { get; set; } = null!;

	[JsonProperty(PropertyName = "scope")]
	public string Scope { get; set; } = null!;

	[JsonProperty(PropertyName = "token_type")]
	public string TokenType { get; set; } = null!;
}
