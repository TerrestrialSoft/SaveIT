using SQLite;

namespace SaveIT.Core.Entities;

public class GameProfile
{
	[PrimaryKey, AutoIncrement]
	public long Id { get; set; }

	[NotNull, Unique]
	public string ProfileName { get; set; } = null!;

	[NotNull]
	public string Nickname { get; set; } = null!;

	public bool IsAuthorized { get; set; } = false;

	public DateTime DateCreated { get; set; }
}
