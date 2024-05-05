namespace SaveIt.Server.UI.Options;

public class TokenCachingOptions
{
    public const string Path = "TokenCaching";

    public int TokenRetrieveDelaySeconds { get; set; }
    public int CacheEntryLifeTimeSeconds { get; set; }
    public int MaxTokenRetrieveTimeSeconds { get; set; }
}
