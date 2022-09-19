using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace SaveIT.App.Models;

public class CreateGameProfileModel
{
	[Required(ErrorMessage = "Profile name has to be set.")]
	[DisplayName("Profile name")]

	public string ProfileName { get; set; } = null!;

	[Required(ErrorMessage = "Nickname has to be set.")]
	[DisplayName("Nickname")]
	public string Nickname { get; set; } = null!;
}
