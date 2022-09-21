namespace SaveIT.CloudStorage.Options;
public class GoogleDriveOAuthOptions
{
	public const string Position = "GoogleDriveOAuth";

	public string ClientId { get; set; } = null!;
	public string ClientSecret { get; set; } = null!;
}
