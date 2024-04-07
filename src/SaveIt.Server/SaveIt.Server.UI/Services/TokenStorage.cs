using FluentResults;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using SaveIt.Server.UI.Models;
using SaveIt.Server.UI.Options;

namespace SaveIt.Server.UI.Services;

public class TokenStorage(IMemoryCache _cache, IOptions<TokenCachingOptions> _tokenCachingOptions, ILogger<TokenStorage> logger)
    : ITokenStorage
{
    private readonly ILogger<TokenStorage> _logger = logger;
    private readonly TimeSpan _tokenRetrieveDelaySeconds = TimeSpan
        .FromSeconds(_tokenCachingOptions.Value.TokenRetrieveDelaySeconds);
    private readonly TimeSpan _cacheEntryLifeTimeMinutes = TimeSpan
        .FromMinutes(_tokenCachingOptions.Value.CacheEntryLifeTimeMinutes);
    private readonly TimeSpan _maxTokenRetrieveTimeMinutes = TimeSpan
        .FromMinutes(_tokenCachingOptions.Value.MaxTokenRetrieveTimeMinutes);

    private const string _cacheEntryTemplate = "AuthTokenRequest:{0}";

    public void SetToken(Guid key, StoredRequest value)
        => _cache.Set(GetCacheKey(key), value, _cacheEntryLifeTimeMinutes);

    public bool TryGetToken(Guid key, out StoredRequest? value)
        => _cache.TryGetValue(GetCacheKey(key), out value);

    private static string GetCacheKey(Guid key)
        => string.Format(_cacheEntryTemplate, key);

    public async Task<Result<OAuthCompleteTokenResponseModel>> WaitForToken(Guid key, CancellationToken cancellationToken)
    {
        var maxTokenRetrieveTime = DateTime.UtcNow.Add(_maxTokenRetrieveTimeMinutes);

        while (true)
        {
            if (!TryGetToken(key, out var value) || value!.Token is null)
            {
                if (DateTime.UtcNow >= maxTokenRetrieveTime)
                {
                    _logger.LogInformation("Token waiting cancelled");
                    return Result.Fail("Token waiting cancelled");
                }

                await Task.Delay(_tokenRetrieveDelaySeconds, cancellationToken);
                continue;
            }

            return value.Token;
        }
    }
}
