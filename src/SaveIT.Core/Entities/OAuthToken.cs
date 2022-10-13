using SQLite;

namespace SaveIT.Core.Entities;
public class OAuthToken
{
	[PrimaryKey, AutoIncrement]
	public long Id { get; set; }

	[Unique, NotNull]
	public long GameProfileId { get; set; }

	[NotNull]
	public string AcccessToken { get; set; } = null!;

	[NotNull]
	public DateTime ExpiresAt { get; set; }

	[NotNull]
	public string RefreshToken { get; set; } = null!;
}
