using Newtonsoft.Json;

namespace SaveIT.Core.Dtos;
public class OAuthTokenDto
{
	public string AcccessToken { get; set; } = null!;
	public long ExpiresIn { get; set; }
	public string RefreshToken { get; set; } = null!;
	public string Scope { get; set; } = null!;
	public string TokenType { get; set; } = null!;
}
