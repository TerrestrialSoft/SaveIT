namespace SaveIt.Server.UI.Options;

public class TokenCachingOptions
{
    public int TokenRetrieveTimeSeconds { get; set; }
    public int CacheEntryLifeTimeMinutes { get; set; }
    public int MaxTokenRetrieveTimeMinutes { get; set; }
}
