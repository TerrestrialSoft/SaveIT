using SQLite;

namespace SaveIT.Core.Entities
{
	public class GameProfile
	{
		[PrimaryKey, AutoIncrement]
		public long Id { get; set; }

		[NotNull, Unique]
		public string ProfileName { get; set; } = null!;

		[NotNull]
		public string Nickname { get; set; } = null!;

		public string? IconPath { get; set; }

		public DateTime DateCreated { get; set; }
	}
}
