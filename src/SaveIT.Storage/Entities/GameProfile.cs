using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaveIT.Core.Entities
{
	public class GameProfile
	{
		public Guid Id { get; set; }
		public string ProfileName { get; set; } = null!;
		public string Nickname { get; set; } = null!;
		public string? IconPath { get; set; }
		public DateTime DateCreated { get; set; }
	}
}
