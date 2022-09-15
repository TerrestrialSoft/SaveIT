using SaveIT.Core.Entities;
using SaveIT.Storage.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaveIT.Core.Services
{
	public class GameProfileService : IGameProfileService
	{
		public GameProfile? GetGameProfile(Guid id)
		{
			return ProfileService.GetProfile(id);
		}

		public IEnumerable<GameProfile> GetGameProfiles()
		{
			return ProfileService.GetProfiles();
		}
	}
}
