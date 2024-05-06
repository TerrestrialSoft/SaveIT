namespace SaveIt.Server.UI.Options;

public class TokenCachingOptions
{
    public const string Path = "TokenCaching";

    public required int TokenRetrieveDelaySeconds { get; set; }
    public required int CacheEntryLifeTimeSeconds { get; set; }
    public required int MaxTokenRetrieveTimeSeconds { get; set; }
}
