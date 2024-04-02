namespace SaveIt.Server.UI.Options;

public class TokenCachingOptions
{
    public const string Path = "TokenCaching";

    public int TokenRetrieveDelaySeconds { get; set; }
    public int CacheEntryLifeTimeMinutes { get; set; }
    public int MaxTokenRetrieveTimeMinutes { get; set; }
}
