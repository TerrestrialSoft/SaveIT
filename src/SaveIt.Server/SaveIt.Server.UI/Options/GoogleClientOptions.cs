namespace SaveIt.Server.UI.Options;

public record GoogleClientOptions
{
    public const string Path = "GoogleClient";

    public required string ClientId { get; set; }
    public required string ClientSecret { get; set; }
    public required string TokenUrl { get; set; }
    public required string OAuthUrl { get; set; }
    public required string LocalRedirectUrl { get; set; }
}
