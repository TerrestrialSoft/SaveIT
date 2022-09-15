using SaveIT.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaveIT.Storage.Database
{
	public static class ProfileService
	{
		private static ICollection<GameProfile> _gameProfiles = new List<GameProfile>()
		{
			new GameProfile()
			{
				Id = Guid.NewGuid(),
				ProfileName = "Minecraft",
				Nickname = "Karel",
				IconPath = null,
				DateCreated = DateTime.Now,
			},
			new GameProfile()
			{
				Id = Guid.NewGuid(),
				DateCreated = DateTime.Now,
				ProfileName = "Kostičky",
				Nickname = "Honza",
				IconPath = "Adsdadad"

			},
			new GameProfile()
			{
				Id = Guid.NewGuid(),
				ProfileName = "Minecraft2",
				Nickname = "Karel01",
				IconPath = null,
				DateCreated = DateTime.Now,
			},
			new GameProfile()
			{
				Id = Guid.NewGuid(),
				ProfileName = "GTA",
				Nickname = "Pepa",
				IconPath = null,
				DateCreated = new DateTime(2022, 6, 6),
			}
		};

		public static IEnumerable<GameProfile> GetProfiles()
		{
			return _gameProfiles;
		}

		public static GameProfile? GetProfile(Guid id)
		{
			return _gameProfiles.FirstOrDefault(x => x.Id == id);
		}

		public static void CreateProfile(GameProfile profile)
		{
			_gameProfiles.Add(profile);
		}
	}
}
