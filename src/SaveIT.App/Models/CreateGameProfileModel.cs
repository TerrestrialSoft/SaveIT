using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace SaveIT.App.Models;

public class CreateGameProfileModel
{
	[DisplayName("Profile name")]
	[Required(ErrorMessage = "Profile name has to be set.")]
	public string ProfileName { get; set; } = null!;

	[DisplayName("Nickname")]
	[Required(ErrorMessage = "Nickname has to be set.")]
	public string Nickname { get; set; } = null!;
}
